#if !UNITY_WEBGL || UNITY_EDITOR

using System;

#if !BESTHTTP_DISABLE_ALTERNATE_SSL
using BestHTTP.SecureProtocol.Org.BouncyCastle.Tls;
#endif

using BestHTTP.Core;
using BestHTTP.Timings;

namespace BestHTTP.Connections
{
    /// <summary>
    /// Represents and manages a connection to a server.
    /// </summary>
    public sealed class HTTPConnection : ConnectionBase
    {
        public TCPConnector connector;
        public IHTTPRequestHandler requestHandler;

        public override TimeSpan KeepAliveTime {
            get {
                if (this.requestHandler != null && this.requestHandler.KeepAlive != null)
                {
                    if (this.requestHandler.KeepAlive.MaxRequests > 0)
                    {
                        if (base.KeepAliveTime < this.requestHandler.KeepAlive.TimeOut)
                            return base.KeepAliveTime;
                        else
                            return this.requestHandler.KeepAlive.TimeOut;
                    }
                    else
                        return TimeSpan.Zero;
                }
        
                return base.KeepAliveTime;
            }
        
            protected set
            {
                base.KeepAliveTime = value;
            }
        }

        public override bool CanProcessMultiple
        {
            get
            {
                if (this.requestHandler != null)
                    return this.requestHandler.CanProcessMultiple;
                return base.CanProcessMultiple;
            }
        }

        internal HTTPConnection(string serverAddress)
            :base(serverAddress)
        {}

        public override bool TestConnection()
        {
#if !NETFX_CORE
            try
            {
#if !BESTHTTP_DISABLE_ALTERNATE_SSL
                if (this.connector.Client.Available > 0)
                {
                    TlsStream stream = (this.connector.Stream as TlsStream);
                    if (stream != null)
                    {
                        try
                        {
                            var available = stream.Protocol.TestApplicationData();
                            return !stream.Protocol.IsClosed;
                        }
                        catch {
                            return false;
                        }
                    }
                }
#endif

                bool connected = this.connector.Client.Connected;

                return connected;
            }
            catch
            {
                return false;
            }
#else
            return base.TestConnection();
#endif
        }

        internal override void Process(HTTPRequest request)
        {
            this.LastProcessedUri = request.CurrentUri;

            if (this.requestHandler == null || !this.requestHandler.HasCustomRequestProcessor)
                base.Process(request);
            else
            {
                this.requestHandler.Process(request);
                LastProcessTime = DateTime.Now;
            }
        }

        protected override void ThreadFunc()
        {
            if (this.CurrentRequest.IsRedirected)
                this.CurrentRequest.Timing.Add(TimingEventNames.Queued_For_Redirection);
            else
                this.CurrentRequest.Timing.Add(TimingEventNames.Queued);

            if (this.connector != null && !this.connector.IsConnected)
            {
                // this will send the request back to the queue
                RequestEventHelper.EnqueueRequestEvent(new RequestEventInfo(CurrentRequest, RequestEvents.Resend));
                ConnectionEventHelper.EnqueueConnectionEvent(new ConnectionEventInfo(this, HTTPConnectionStates.Closed));
                return;
            }

            if (this.connector == null)
            {
                this.connector = new Connections.TCPConnector();

                try
                {
                    this.connector.Connect(this.CurrentRequest);
                }
                catch(Exception ex)
                {
                    if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                        HTTPManager.Logger.Exception("HTTPConnection", "Connector.Connect", ex, this.Context, this.CurrentRequest.Context);

                    
                    if (ex is TimeoutException)
                        this.CurrentRequest.State = HTTPRequestStates.ConnectionTimedOut;
                    else if (!this.CurrentRequest.IsTimedOut) // Do nothing here if Abort() got called on the request, its State is already set.
                    {
                        this.CurrentRequest.Exception = ex;
                        this.CurrentRequest.State = HTTPRequestStates.Error;
                    }

                    ConnectionEventHelper.EnqueueConnectionEvent(new ConnectionEventInfo(this, HTTPConnectionStates.Closed));

                    return;
                }

#if !NETFX_CORE
                // data sending is buffered for all protocols, so when we put data into the socket we want to send them asap
                this.connector.Client.NoDelay = true;
#endif
                StartTime = DateTime.UtcNow;

                HTTPManager.Logger.Information("HTTPConnection", "Negotiated protocol through ALPN: '" + this.connector.NegotiatedProtocol + "'", this.Context, this.CurrentRequest.Context);

                switch (this.connector.NegotiatedProtocol)
                {
                    case HTTPProtocolFactory.W3C_HTTP1:
                        this.requestHandler = new Connections.HTTP1Handler(this);
                        ConnectionEventHelper.EnqueueConnectionEvent(new ConnectionEventInfo(this, HostProtocolSupport.HTTP1));
                        break;

#if (!UNITY_WEBGL || UNITY_EDITOR) && !BESTHTTP_DISABLE_ALTERNATE_SSL && !BESTHTTP_DISABLE_HTTP2
                    case HTTPProtocolFactory.W3C_HTTP2:
                        this.requestHandler = new Connections.HTTP2.HTTP2Handler(this);
                        this.CurrentRequest = null;
                        ConnectionEventHelper.EnqueueConnectionEvent(new ConnectionEventInfo(this, HostProtocolSupport.HTTP2));
                        break;
#endif

                    default:
                        HTTPManager.Logger.Error("HTTPConnection", "Unknown negotiated protocol: " + this.connector.NegotiatedProtocol, this.Context, this.CurrentRequest.Context);

                        RequestEventHelper.EnqueueRequestEvent(new RequestEventInfo(CurrentRequest, RequestEvents.Resend));
                        ConnectionEventHelper.EnqueueConnectionEvent(new ConnectionEventInfo(this, HTTPConnectionStates.Closed));
                        return;
                }
            }

            this.requestHandler.Context.Add("Connection", this.GetHashCode());
            this.Context.Add("RequestHandler", this.requestHandler.GetHashCode());

            this.requestHandler.RunHandler();
            LastProcessTime = DateTime.Now;
        }

        public override void Shutdown(ShutdownTypes type)
        {
            base.Shutdown(type);

            if (this.requestHandler != null)
                this.requestHandler.Shutdown(type);

            switch(this.ShutdownType)
            {
                case ShutdownTypes.Immediate:
                    this.connector.Dispose();
                    break;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                LastProcessedUri = null;
                if (this.State != HTTPConnectionStates.WaitForProtocolShutdown)
                {
                    if (this.connector != null)
                    {
                        try
                        {
                            this.connector.Close();
                        }
                        catch
                        { }
                        this.connector = null;
                    }

                    if (this.requestHandler != null)
                    {
                        try
                        {
                            this.requestHandler.Dispose();
                        }
                        catch
                        { }
                        this.requestHandler = null;
                    }
                }
                else
                {
                    // We have to connector to do not close its stream at any cost while disposing. 
                    // All references to this connection will be removed, so this and the connector may be finalized after some time.
                    // But, finalizing (and disposing) the connector while the protocol is still active would be fatal, 
                    //  so we have to make sure that it will not happen. This also means that the protocol has the responsibility (as always had)
                    //  to close the stream and TCP connection properly.
                    if (this.connector != null)
                        this.connector.LeaveOpen = true;
                }
            }

            base.Dispose(disposing);
        }
    }
}

#endif
