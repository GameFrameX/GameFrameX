#if !BESTHTTP_DISABLE_SIGNALR

using System;
using System.Collections.Generic;

using BestHTTP.SignalR.Messages;
using BestHTTP.SignalR.JsonEncoders;

namespace BestHTTP.SignalR.Transports
{
    public delegate void OnTransportStateChangedDelegate(TransportBase transport, TransportStates oldState, TransportStates newState);

    public abstract class TransportBase
    {
        private const int MaxRetryCount = 5;

        #region Public Properties

        /// <summary>
        /// Name of the transport.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// True if the manager has to check the last message received time and reconnect if too much time passes.
        /// </summary>
        public abstract bool SupportsKeepAlive { get; }

        /// <summary>
        /// Type of the transport. Used mainly by the manager in its BuildUri function.
        /// </summary>
        public abstract TransportTypes Type { get; }

        /// <summary>
        /// Reference to the manager.
        /// </summary>
        public IConnection Connection { get; protected set; }

        /// <summary>
        /// The current state of the transport.
        /// </summary>
        public TransportStates State
        {
            get { return _state; }
            protected set
            {
                TransportStates old = _state;
                _state = value;

                if (OnStateChanged != null)
                    OnStateChanged(this, old, _state);
            }
        }
        public TransportStates _state;

        /// <summary>
        /// Thi event called when the transport's State set to a new value.
        /// </summary>
        public event OnTransportStateChangedDelegate OnStateChanged;

        #endregion

        public TransportBase(string name, Connection connection)
        {
            this.Name = name;
            this.Connection = connection;
            this.State = TransportStates.Initial;
        }

        #region Abstract functions

        /// <summary>
        /// Start to connect to the server
        /// </summary>
        public abstract void Connect();

        /// <summary>
        /// Stop the connection
        /// </summary>
        public abstract void Stop();

        /// <summary>
        /// The transport specific implementation to send the given json string to the server.
        /// </summary>
        protected abstract void SendImpl(string json);

        /// <summary>
        /// Called when the Start request finished successfully, or after a reconnect.
        /// Manager.TransportOpened(); called from the TransportBase after this call
        /// </summary>
        protected abstract void Started();

        /// <summary>
        /// Called when the abort request finished successfully.
        /// </summary>
        protected abstract void Aborted();

        #endregion

        /// <summary>
        /// Called after a succesful connect/reconnect. The transport implementations have to call this function.
        /// </summary>
        protected void OnConnected()
        {
            if (this.State != TransportStates.Reconnecting)
            {
                // Send the Start request
                Start();
            }
            else
            {
                Connection.TransportReconnected();

                Started();

                this.State = TransportStates.Started;
            }
        }

        #region Start Request Sending

        /// <summary>
        /// Sends out the /start request to the server.
        /// </summary>
        protected void Start()
        {
            HTTPManager.Logger.Information("Transport - " + this.Name, "Sending Start Request");

            this.State = TransportStates.Starting;

            if (this.Connection.Protocol > ProtocolVersions.Protocol_2_0)
            {
                var request = new HTTPRequest(Connection.BuildUri(RequestTypes.Start, this), HTTPMethods.Get, true, true, OnStartRequestFinished);

                request.Tag = 0;
                request.MaxRetries = 0;

                request.Timeout = Connection.NegotiationResult.ConnectionTimeout + TimeSpan.FromSeconds(10);

                Connection.PrepareRequest(request, RequestTypes.Start);

                request.Send();
            }
            else
            {
                // The transport and the signalr protocol now started
                this.State = TransportStates.Started;

                Started();

                Connection.TransportStarted();
            }
        }

        private void OnStartRequestFinished(HTTPRequest req, HTTPResponse resp)
        {
            switch (req.State)
            {
                case HTTPRequestStates.Finished:
                    if (resp.IsSuccess)
                    {
                        HTTPManager.Logger.Information("Transport - " + this.Name, "Start - Returned: " + resp.DataAsText);

                        string response = Connection.ParseResponse(resp.DataAsText);

                        if (response != "started")
                        {
                            Connection.Error(string.Format("Expected 'started' response, but '{0}' found!", response));
                            return;
                        }

                        // The transport and the signalr protocol now started
                        this.State = TransportStates.Started;

                        Started();

                        Connection.TransportStarted();

                        return;
                    }
                    else
                        HTTPManager.Logger.Warning("Transport - " + this.Name, string.Format("Start - request finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2} Uri: {3}",
                                                                    resp.StatusCode,
                                                                    resp.Message,
                                                                    resp.DataAsText,
                                                                    req.CurrentUri));
                    goto default;

                default:
                    HTTPManager.Logger.Information("Transport - " + this.Name, "Start request state: " + req.State.ToString());

                    // The request may not reached the server. Try it again.
                    int retryCount = (int)req.Tag;
                    if (retryCount++ < MaxRetryCount)
                    {
                        req.Tag = retryCount;
                        req.Send();
                    }
                    else
                        Connection.Error("Failed to send Start request.");

                    break;
            }
        }

        #endregion

        #region Abort Implementation

        /// <summary>
        /// Will abort the transport. In SignalR 'Abort'ing is a graceful process, while 'Close'ing is a hard-abortion...
        /// </summary>
        public virtual void Abort()
        {
            if (this.State != TransportStates.Started)
                return;

            this.State = TransportStates.Closing;

            var request = new HTTPRequest(Connection.BuildUri(RequestTypes.Abort, this), HTTPMethods.Get, true, true, OnAbortRequestFinished);

            // Retry counter
            request.Tag = 0;
            request.MaxRetries = 0;

            Connection.PrepareRequest(request, RequestTypes.Abort);

            request.Send();
        }

        protected void AbortFinished()
        {
            this.State = TransportStates.Closed;

            Connection.TransportAborted();

            this.Aborted();
        }

        private void OnAbortRequestFinished(HTTPRequest req, HTTPResponse resp)
        {
            switch (req.State)
            {
                case HTTPRequestStates.Finished:
                    if (resp.IsSuccess)
                    {
                        HTTPManager.Logger.Information("Transport - " + this.Name, "Abort - Returned: " + resp.DataAsText);

                        if (this.State == TransportStates.Closing)
                            AbortFinished();
                    }
                    else
                    {
                        HTTPManager.Logger.Warning("Transport - " + this.Name, string.Format("Abort - Handshake request finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2} Uri: {3}",
                                                                    resp.StatusCode,
                                                                    resp.Message,
                                                                    resp.DataAsText,
                                                                    req.CurrentUri));

                        // try again
                        goto default;
                    }
                    break;
                default:
                    HTTPManager.Logger.Information("Transport - " + this.Name, "Abort request state: " + req.State.ToString());

                    // The request may not reached the server. Try it again.
                    int retryCount = (int)req.Tag;
                    if (retryCount++ < MaxRetryCount)
                    {
                        req.Tag = retryCount;                        
                        req.Send();
                    }
                    else
                        Connection.Error("Failed to send Abort request!");

                    break;
            }
        }

        #endregion

        #region Send Implementation

        /// <summary>
        /// Sends the given json string to the wire.
        /// </summary>
        /// <param name="jsonStr"></param>
        public void Send(string jsonStr)
        {
            try
            {
                HTTPManager.Logger.Information("Transport - " + this.Name, "Sending: " + jsonStr);

                SendImpl(jsonStr);
            }
            catch (Exception ex)
            {
                HTTPManager.Logger.Exception("Transport - " + this.Name, "Send", ex);
            }
        }

        #endregion

        #region Helper Functions

        /// <summary>
        /// Start the reconnect process
        /// </summary>
        public void Reconnect()
        {
            HTTPManager.Logger.Information("Transport - " + this.Name, "Reconnecting");

            Stop();

            this.State = TransportStates.Reconnecting;

            Connect();
        }

        /// <summary>
        /// When the json string is successfully parsed will return with an IServerMessage implementation.
        /// </summary>
        public static IServerMessage Parse(IJsonEncoder encoder, string json)
        {
            // Nothing to parse?
            if (string.IsNullOrEmpty(json))
            {
                HTTPManager.Logger.Error("MessageFactory", "Parse - called with empty or null string!");
                return null;
            }

            // We don't have to do further decoding, if it's an empty json object, then it's a KeepAlive message from the server
            if (json.Length == 2 && json == "{}")
                return new KeepAliveMessage();

            IDictionary<string, object> msg = null;

            try
            {
                // try to decode the json message with the encoder
                msg = encoder.DecodeMessage(json);
            }
            catch(Exception ex)
            {
                HTTPManager.Logger.Exception("MessageFactory", "Parse - encoder.DecodeMessage", ex);
                return null;
            }

            if (msg == null)
            {
                HTTPManager.Logger.Error("MessageFactory", "Parse - Json Decode failed for json string: \"" + json + "\"");
                return null;
            }

            // "C" is for message id
            IServerMessage result = null;
            if (!msg.ContainsKey("C"))
            {
                // If there are no ErrorMessage in the object, then it was a success
                if (!msg.ContainsKey("E"))
                    result = new ResultMessage();
                else
                    result = new FailureMessage();
            }
            else
              result = new MultiMessage();

            try
            {
                result.Parse(msg);
            }
            catch
            {
                HTTPManager.Logger.Error("MessageFactory", "Can't parse msg: " + json);
                throw;
            }

            return result;
        }

        #endregion
    }
}

#endif