#if !BESTHTTP_DISABLE_SIGNALR

using System;

using BestHTTP.Extensions;
using BestHTTP.SignalR.Messages;

namespace BestHTTP.SignalR.Transports
{
    public sealed class PollingTransport : PostSendTransportBase, IHeartbeat
    {
        #region Overridden Properties

        public override bool SupportsKeepAlive { get { return false; } }
        public override TransportTypes Type { get { return TransportTypes.LongPoll; } }

        #endregion

        #region Privates

        /// <summary>
        /// When we received the last poll.
        /// </summary>
        private DateTime LastPoll;

        /// <summary>
        /// How much time we have to wait before we can send out a new poll request. This value sent by the server.
        /// </summary>
        private TimeSpan PollDelay;

        /// <summary>
        /// How much time we wait to a poll request to finish. It's value is the server sent negotiation's ConnectionTimeout + 10sec.
        /// </summary>
        private TimeSpan PollTimeout;

        /// <summary>
        /// Reference to the the current poll request.
        /// </summary>
        private HTTPRequest pollRequest;

        #endregion

        public PollingTransport(Connection connection)
            : base("longPolling", connection)
        {
            this.LastPoll = DateTime.MinValue;
            this.PollTimeout = connection.NegotiationResult.ConnectionTimeout + TimeSpan.FromSeconds(10);
        }

        #region Overrides from TransportBase

        /// <summary>
        /// Polling transport specific connection logic. It's a regular GET request to the /connect path.
        /// </summary>
        public override void Connect()
        {
            HTTPManager.Logger.Information("Transport - " + this.Name, "Sending Open Request");

            // Skip the Connecting state if we are reconnecting. If the connect succeeds, we will set the Started state directly
            if (this.State != TransportStates.Reconnecting)
                this.State = TransportStates.Connecting;

            RequestTypes requestType = this.State == TransportStates.Reconnecting ? RequestTypes.Reconnect : RequestTypes.Connect;

            var request = new HTTPRequest(Connection.BuildUri(requestType, this), HTTPMethods.Get, true, true, OnConnectRequestFinished);

            Connection.PrepareRequest(request, requestType);

            request.Send();
        }

        public override void Stop()
        {
            HTTPManager.Heartbeats.Unsubscribe(this);

            if (pollRequest != null)
            {
                pollRequest.Abort();
                pollRequest = null;
            }

            // Should we abort the send requests in the sendRequestQueue?
        }

        protected override void Started()
        {
            LastPoll = DateTime.UtcNow;
            HTTPManager.Heartbeats.Subscribe(this);
        }

        protected override void Aborted()
        {
            HTTPManager.Heartbeats.Unsubscribe(this);
        }

        #endregion

        #region Request Handlers

        void OnConnectRequestFinished(HTTPRequest req, HTTPResponse resp)
        {
            // error reason if there is any. We will call the manager's Error function if it's not empty.
            string reason = string.Empty;

            switch (req.State)
            {
                // The request finished without any problem.
                case HTTPRequestStates.Finished:
                    if (resp.IsSuccess)
                    {
                        HTTPManager.Logger.Information("Transport - " + this.Name, "Connect - Request Finished Successfully! " + resp.DataAsText);

                        OnConnected();

                        IServerMessage msg = TransportBase.Parse(Connection.JsonEncoder, resp.DataAsText);

                        if (msg != null)
                        {
                            Connection.OnMessage(msg);

                            MultiMessage multiple = msg as MultiMessage;
                            if (multiple != null && multiple.PollDelay.HasValue)
                                PollDelay = multiple.PollDelay.Value;
                        }
                    }
                    else
                        reason = string.Format("Connect - Request Finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}",
                                                                                                            resp.StatusCode,
                                                                                                            resp.Message,
                                                                                                            resp.DataAsText);
                    break;

                // The request finished with an unexpected error. The request's Exception property may contain more info about the error.
                case HTTPRequestStates.Error:
                    reason = "Connect - Request Finished with Error! " + (req.Exception != null ? (req.Exception.Message + "\n" + req.Exception.StackTrace) : "No Exception");
                    break;

                // The request aborted, initiated by the user.
                case HTTPRequestStates.Aborted:
                    reason = "Connect - Request Aborted!";
                    break;

                // Connecting to the server is timed out.
                case HTTPRequestStates.ConnectionTimedOut:
                    reason = "Connect - Connection Timed Out!";
                    break;

                // The request didn't finished in the given time.
                case HTTPRequestStates.TimedOut:
                    reason = "Connect - Processing the request Timed Out!";
                    break;
            }

            if (!string.IsNullOrEmpty(reason))
                Connection.Error(reason);
        }

        void OnPollRequestFinished(HTTPRequest req, HTTPResponse resp)
        {
            // When Stop() called on the transport.
            // In Stop() we set the pollRequest to null, but a new poll request can be made after a quick reconnection, and there is a chanse that 
            // in this handler function we can null out the new request. So we return early here.
            if (req.IsCancellationRequested)
            {
                HTTPManager.Logger.Warning("Transport - " + this.Name, "Poll - Request Aborted!");
                return;
            }

            // Set the pollRequest to null, now we can send out a new one
            pollRequest = null;

            // error reason if there is any. We will call the manager's Error function if it's not empty.
            string reason = string.Empty;

            switch (req.State)
            {
                // The request finished without any problem.
                case HTTPRequestStates.Finished:
                    if (resp.IsSuccess)
                    {
                        HTTPManager.Logger.Information("Transport - " + this.Name, "Poll - Request Finished Successfully! " + resp.DataAsText);

                        IServerMessage msg = TransportBase.Parse(Connection.JsonEncoder, resp.DataAsText);

                        if (msg != null)
                        {
                            Connection.OnMessage(msg);

                            MultiMessage multiple = msg as MultiMessage;
                            if (multiple != null && multiple.PollDelay.HasValue)
                                PollDelay = multiple.PollDelay.Value;

                            LastPoll = DateTime.UtcNow;
                        }
                    }
                    else
                        reason = string.Format("Poll - Request Finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}",
                                                                                                    resp.StatusCode,
                                                                                                    resp.Message,
                                                                                                    resp.DataAsText);
                    break;

                // The request finished with an unexpected error. The request's Exception property may contain more info about the error.
                case HTTPRequestStates.Error:
                    reason = "Poll - Request Finished with Error! " + (req.Exception != null ? (req.Exception.Message + "\n" + req.Exception.StackTrace) : "No Exception");
                    break;

                // Connecting to the server is timed out.
                case HTTPRequestStates.ConnectionTimedOut:
                    reason = "Poll - Connection Timed Out!";
                    break;

                // The request didn't finished in the given time.
                case HTTPRequestStates.TimedOut:
                    reason = "Poll - Processing the request Timed Out!";
                    break;
            }

            if (!string.IsNullOrEmpty(reason))
                Connection.Error(reason);
        }

        #endregion

        /// <summary>
        /// Polling transport speficic function. Sends a GET request to the /poll path to receive messages.
        /// </summary>
        private void Poll()
        {
            pollRequest = new HTTPRequest(Connection.BuildUri(RequestTypes.Poll, this), HTTPMethods.Get, true, true, OnPollRequestFinished);

            Connection.PrepareRequest(pollRequest, RequestTypes.Poll);

            pollRequest.Timeout = this.PollTimeout;

            pollRequest.Send();
        }

        #region IHeartbeat Implementation

        void IHeartbeat.OnHeartbeatUpdate(TimeSpan dif)
        {
            switch(State)
            {
                case TransportStates.Started:
                    if (pollRequest == null && DateTime.UtcNow >= (LastPoll + PollDelay + Connection.NegotiationResult.LongPollDelay))
                        Poll();
                    break;
            }
        }

        #endregion
    }
}

#endif