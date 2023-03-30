#if !UNITY_WEBGL || UNITY_EDITOR
using System;
using BestHTTP.Core;
using BestHTTP.Logger;

#if !BESTHTTP_DISABLE_CACHING
using BestHTTP.Caching;
#endif

using BestHTTP.Timings;

namespace BestHTTP.Connections
{
    public sealed class HTTP1Handler : IHTTPRequestHandler
    {
        public bool HasCustomRequestProcessor { get { return false; } }
        public KeepAliveHeader KeepAlive { get { return this._keepAlive; } }
        private KeepAliveHeader _keepAlive;

        public bool CanProcessMultiple { get { return false; } }

        private readonly HTTPConnection conn;

        public LoggingContext Context { get; private set; }

        public HTTP1Handler(HTTPConnection conn)
        {
            this.Context = new LoggingContext(this);
            this.conn = conn;
        }

        public void Process(HTTPRequest request)
        {
        }

        public void RunHandler()
        {
            HTTPManager.Logger.Information("HTTP1Handler", string.Format("[{0}] started processing request '{1}'", this, this.conn.CurrentRequest.CurrentUri.ToString()), this.Context, this.conn.CurrentRequest.Context);

            System.Threading.Thread.CurrentThread.Name = "BestHTTP.HTTP1 R&W";

            HTTPConnectionStates proposedConnectionState = HTTPConnectionStates.Processing;

            bool resendRequest = false;

            try
            {
                if (this.conn.CurrentRequest.IsCancellationRequested)
                    return;

#if !BESTHTTP_DISABLE_CACHING
                // Setup cache control headers before we send out the request
                if (!this.conn.CurrentRequest.DisableCache)
                    HTTPCacheService.SetHeaders(this.conn.CurrentRequest);
#endif

                // Write the request to the stream
                this.conn.CurrentRequest.QueuedAt = DateTime.MinValue;
                this.conn.CurrentRequest.ProcessingStarted = DateTime.UtcNow;
                this.conn.CurrentRequest.SendOutTo(this.conn.connector.Stream);
                this.conn.CurrentRequest.Timing.Add(TimingEventNames.Request_Sent);

                if (this.conn.CurrentRequest.IsCancellationRequested)
                    return;

                this.conn.CurrentRequest.OnCancellationRequested += OnCancellationRequested;

                // Receive response from the server
                bool received = Receive(this.conn.CurrentRequest);

                this.conn.CurrentRequest.Timing.Add(TimingEventNames.Response_Received);

                if (this.conn.CurrentRequest.IsCancellationRequested)
                    return;

                if (!received && this.conn.CurrentRequest.Retries < this.conn.CurrentRequest.MaxRetries)
                {
                    proposedConnectionState = HTTPConnectionStates.Closed;
                    this.conn.CurrentRequest.Retries++;
                    resendRequest = true;
                    return;
                }

                ConnectionHelper.HandleResponse(this.conn.ToString(), this.conn.CurrentRequest, out resendRequest, out proposedConnectionState, ref this._keepAlive, this.conn.Context, this.conn.CurrentRequest.Context);
            }
            catch (TimeoutException e)
            {
                this.conn.CurrentRequest.Response = null;

                // Do nothing here if Abort() got called on the request, its State is already set.
                if (!this.conn.CurrentRequest.IsTimedOut)
                {
                    // We will try again only once
                    if (this.conn.CurrentRequest.Retries < this.conn.CurrentRequest.MaxRetries)
                    {
                        this.conn.CurrentRequest.Retries++;
                        resendRequest = true;
                    }
                    else
                    {
                        this.conn.CurrentRequest.Exception = e;
                        this.conn.CurrentRequest.State = HTTPRequestStates.ConnectionTimedOut;
                    }
                }

                proposedConnectionState = HTTPConnectionStates.Closed;
            }
            catch (Exception e)
            {
                if (this.ShutdownType == ShutdownTypes.Immediate)
                    return;

                string exceptionMessage = string.Empty;
                if (e == null)
                    exceptionMessage = "null";
                else
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();

                    Exception exception = e;
                    int counter = 1;
                    while (exception != null)
                    {
                        sb.AppendFormat("{0}: {1} {2}", counter++.ToString(), exception.Message, exception.StackTrace);

                        exception = exception.InnerException;

                        if (exception != null)
                            sb.AppendLine();
                    }

                    exceptionMessage = sb.ToString();
                }
                HTTPManager.Logger.Verbose("HTTP1Handler", exceptionMessage, this.Context, this.conn.CurrentRequest.Context);

#if !BESTHTTP_DISABLE_CACHING
                if (this.conn.CurrentRequest.UseStreaming)
                    HTTPCacheService.DeleteEntity(this.conn.CurrentRequest.CurrentUri);
#endif

                // Something gone bad, Response must be null!
                this.conn.CurrentRequest.Response = null;

                // Do nothing here if Abort() got called on the request, its State is already set.
                if (!this.conn.CurrentRequest.IsCancellationRequested)
                {
                    this.conn.CurrentRequest.Exception = e;
                    this.conn.CurrentRequest.State = HTTPRequestStates.Error;
                }

                proposedConnectionState = HTTPConnectionStates.Closed;
            }
            finally
            {
                this.conn.CurrentRequest.OnCancellationRequested -= OnCancellationRequested;

                // Exit ASAP
                if (this.ShutdownType != ShutdownTypes.Immediate)
                {
                    if (this.conn.CurrentRequest.IsCancellationRequested)
                    {
                        // we don't know what stage the request is canceled, we can't safely reuse the tcp channel.
                        proposedConnectionState = HTTPConnectionStates.Closed;

                        this.conn.CurrentRequest.Response = null;

                        // The request's State already set, or going to be set soon in RequestEvents.cs.
                        //this.conn.CurrentRequest.State = this.conn.CurrentRequest.IsTimedOut ? HTTPRequestStates.TimedOut : HTTPRequestStates.Aborted;
                    }
                    else if (resendRequest)
                    {
                        // Here introducing a ClosedResendRequest connection state, where we have to process the connection's state change to Closed
                        // than we have to resend the request.
                        // If we would send the Resend request here, than a few lines below the Closed connection state change,
                        //  request events are processed before connection events (just switching the EnqueueRequestEvent and EnqueueConnectionEvent wouldn't work
                        //  see order of ProcessQueues in HTTPManager.OnUpdate!) and it would pick this very same closing/closed connection!

                        if (proposedConnectionState == HTTPConnectionStates.Closed || proposedConnectionState == HTTPConnectionStates.ClosedResendRequest)
                            ConnectionEventHelper.EnqueueConnectionEvent(new ConnectionEventInfo(this.conn, this.conn.CurrentRequest));
                        else
                            RequestEventHelper.EnqueueRequestEvent(new RequestEventInfo(this.conn.CurrentRequest, RequestEvents.Resend));
                    }
                    else if (this.conn.CurrentRequest.Response != null && this.conn.CurrentRequest.Response.IsUpgraded)
                    {
                        proposedConnectionState = HTTPConnectionStates.WaitForProtocolShutdown;
                    }
                    else if (this.conn.CurrentRequest.State == HTTPRequestStates.Processing)
                    {
                        if (this.conn.CurrentRequest.Response != null)
                            this.conn.CurrentRequest.State = HTTPRequestStates.Finished;
                        else
                        {
                            this.conn.CurrentRequest.Exception = new Exception(string.Format("[{0}] Remote server closed the connection before sending response header! Previous request state: {1}. Connection state: {2}",
                                    this.ToString(),
                                    this.conn.CurrentRequest.State.ToString(),
                                    this.conn.State.ToString()));
                            this.conn.CurrentRequest.State = HTTPRequestStates.Error;

                            proposedConnectionState = HTTPConnectionStates.Closed;
                        }
                    }

                    this.conn.CurrentRequest = null;

                    if (proposedConnectionState == HTTPConnectionStates.Processing)
                        proposedConnectionState = HTTPConnectionStates.Recycle;

                    if (proposedConnectionState != HTTPConnectionStates.ClosedResendRequest)
                        ConnectionEventHelper.EnqueueConnectionEvent(new ConnectionEventInfo(this.conn, proposedConnectionState));
                }
            }
        }

        private void OnCancellationRequested(HTTPRequest obj)
        {
            if (this.conn != null && this.conn.connector != null)
                this.conn.connector.Dispose();
        }

        private bool Receive(HTTPRequest request)
        {
            SupportedProtocols protocol = HTTPProtocolFactory.GetProtocolFromUri(request.CurrentUri);

            if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                HTTPManager.Logger.Verbose("HTTPConnection", string.Format("[{0}] - Receive - protocol: {1}", this.ToString(), protocol.ToString()), this.Context, request.Context);

            request.Response = HTTPProtocolFactory.Get(protocol, request, this.conn.connector.Stream, request.UseStreaming, false);

            if (!request.Response.Receive())
            {
                if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                    HTTPManager.Logger.Verbose("HTTP1Handler", string.Format("[{0}] - Receive - Failed! Response will be null, returning with false.", this.ToString()), this.Context, request.Context);
                request.Response = null;
                return false;
            }

            if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                HTTPManager.Logger.Verbose("HTTP1Handler", string.Format("[{0}] - Receive - Finished Successfully!", this.ToString()), this.Context, request.Context);

            return true;
        }

        public ShutdownTypes ShutdownType { get; private set; }

        public void Shutdown(ShutdownTypes type)
        {
            this.ShutdownType = type;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
        }

        ~HTTP1Handler()
        {
            Dispose(false);
        }
    }
}

#endif
