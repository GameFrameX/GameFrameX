#if !BESTHTTP_DISABLE_SIGNALR
#if !BESTHTTP_DISABLE_SERVERSENT_EVENTS

using System;

using BestHTTP.ServerSentEvents;
using BestHTTP.SignalR.Messages;

namespace BestHTTP.SignalR.Transports
{
    /// <summary>
    /// A SignalR transport implementation that uses the Server-Sent Events protocol.
    /// </summary>
    public sealed class ServerSentEventsTransport : PostSendTransportBase
    {
        #region Overridden Properties

        /// <summary>
        /// It's a persistent connection. We support the keep-alive mechanism.
        /// </summary>
        public override bool SupportsKeepAlive { get { return true; } }

        /// <summary>
        /// Type of the transport.
        /// </summary>
        public override TransportTypes Type { get { return TransportTypes.ServerSentEvents; } }

        #endregion

        #region Privates

        /// <summary>
        /// The EventSource object.
        /// </summary>
        private EventSource EventSource;

        #endregion

        public ServerSentEventsTransport(Connection con)
            : base("serverSentEvents", con)
        {

        }

        #region Overrides from TransportBase

        public override void Connect()
        {
            if (EventSource != null)
            {
                HTTPManager.Logger.Warning("ServerSentEventsTransport", "Start - EventSource already created!");
                return;
            }

            // Skip the Connecting state if we are reconnecting. If the connect succeeds, we will set the Started state directly
            if (this.State != TransportStates.Reconnecting)
                this.State = TransportStates.Connecting;

            RequestTypes requestType = this.State == TransportStates.Reconnecting ? RequestTypes.Reconnect : RequestTypes.Connect;

            Uri uri = Connection.BuildUri(requestType, this);

            EventSource = new EventSource(uri);

            EventSource.OnOpen += OnEventSourceOpen;
            EventSource.OnMessage += OnEventSourceMessage;
            EventSource.OnError += OnEventSourceError;
            EventSource.OnClosed += OnEventSourceClosed;

#if !UNITY_WEBGL || UNITY_EDITOR
            // Disable internal retry
            EventSource.OnRetry += (es) => false;
#endif

            // Start connecting to the server.
            EventSource.Open();
        }

        public override void Stop()
        {
            EventSource.OnOpen -= OnEventSourceOpen;
            EventSource.OnMessage -= OnEventSourceMessage;
            EventSource.OnError -= OnEventSourceError;
            EventSource.OnClosed -= OnEventSourceClosed;

            EventSource.Close();

            EventSource = null;
        }

        protected override void Started()
        {
            // Nothing to do here for this transport
        }

        /// <summary>
        /// A custom Abort function where we will start to close the EventSource object.
        /// </summary>
        public override void Abort()
        {
            base.Abort();

            EventSource.Close();
        }

        protected override void Aborted()
        {
            if (this.State == TransportStates.Closing)
                this.State = TransportStates.Closed;
        }

        #endregion

        #region EventSource Event Handlers

        private void OnEventSourceOpen(EventSource eventSource)
        {
            HTTPManager.Logger.Information("Transport - " + this.Name, "OnEventSourceOpen");
        }

        private void OnEventSourceMessage(EventSource eventSource, BestHTTP.ServerSentEvents.Message message)
        {
            if (message.Data.Equals("initialized"))
            {
                base.OnConnected();

                return;
            }

            IServerMessage msg = TransportBase.Parse(Connection.JsonEncoder, message.Data);

            if (msg != null)
                Connection.OnMessage(msg);

        }

        private void OnEventSourceError(EventSource eventSource, string error)
        {
            HTTPManager.Logger.Information("Transport - " + this.Name, "OnEventSourceError");

            // We are in a reconnecting phase, we have to connect now.
            if (this.State == TransportStates.Reconnecting)
            {
                Connect();
                return;
            }

            // Already closed?
            if (this.State == TransportStates.Closed)
                return;

            // Closing? Then we are closed now.
            if (this.State == TransportStates.Closing)
                this.State = TransportStates.Closed;
            else // Errored when we didn't expected it.
                Connection.Error(error);
        }

        private void OnEventSourceClosed(ServerSentEvents.EventSource eventSource)
        {
            HTTPManager.Logger.Information("Transport - " + this.Name, "OnEventSourceClosed");

            OnEventSourceError(eventSource, "EventSource Closed!");
        }

        #endregion
    }
}

#endif
#endif