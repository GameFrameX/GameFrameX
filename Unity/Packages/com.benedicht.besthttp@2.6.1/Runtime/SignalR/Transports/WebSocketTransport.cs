#if !BESTHTTP_DISABLE_SIGNALR
#if !BESTHTTP_DISABLE_WEBSOCKET

using System;
using System.Text;

using BestHTTP;
using BestHTTP.JSON;
using BestHTTP.SignalR.Hubs;
using BestHTTP.SignalR.Messages;
using BestHTTP.SignalR.JsonEncoders;

namespace BestHTTP.SignalR.Transports
{
    public sealed class WebSocketTransport : TransportBase
    {
        #region Overridden Properties

        public override bool SupportsKeepAlive { get { return true; } }
        public override TransportTypes Type { get { return TransportTypes.WebSocket; } }

        #endregion

        private WebSocket.WebSocket wSocket;

        public WebSocketTransport(Connection connection)
            : base("webSockets", connection)
        {
        }

        #region Overrides from TransportBase

        /// <summary>
        /// Websocket transport specific connection logic. It will create a WebSocket instance, and starts to connect to the server.
        /// </summary>
        public override void Connect()
        {
            if (wSocket != null)
            {
                HTTPManager.Logger.Warning("WebSocketTransport", "Start - WebSocket already created!");
                return;
            }

            // Skip the Connecting state if we are reconnecting. If the connect succeeds, we will set the Started state directly
            if (this.State != TransportStates.Reconnecting)
                this.State = TransportStates.Connecting;

            RequestTypes requestType = this.State == TransportStates.Reconnecting ? RequestTypes.Reconnect : RequestTypes.Connect;

            Uri uri = Connection.BuildUri(requestType, this);

            // Create the WebSocket instance
            wSocket = new WebSocket.WebSocket(uri);

            // Set up eventhandlers
            wSocket.OnOpen += WSocket_OnOpen;
            wSocket.OnMessage += WSocket_OnMessage;
            wSocket.OnClosed += WSocket_OnClosed;
            wSocket.OnError += WSocket_OnError;

#if !UNITY_WEBGL || UNITY_EDITOR
            // prepare the internal http request
            wSocket.OnInternalRequestCreated = (ws, internalRequest) => Connection.PrepareRequest(internalRequest, requestType);
#endif

            // start opening the websocket protocol
            wSocket.Open();
        }

        protected override void SendImpl(string json)
        {
            if (wSocket != null && wSocket.IsOpen)
                wSocket.Send(json);
        }

        public override void Stop()
        {
            if (wSocket != null)
            {
                wSocket.OnOpen = null;
                wSocket.OnMessage = null;
                wSocket.OnClosed = null;
                wSocket.OnError = null;
                wSocket.Close();
                wSocket = null;
            }
        }

        protected override void Started()
        {
            // Nothing to be done here for this transport
        }

        /// <summary>
        /// The /abort request successfully finished
        /// </summary>
        protected override void Aborted()
        {
            // if the websocket is still open, close it
            if (wSocket != null && wSocket.IsOpen)
            {
                wSocket.Close();
                wSocket = null;
            }
        }

#endregion

#region WebSocket Events

        void WSocket_OnOpen(WebSocket.WebSocket webSocket)
        {
            if (webSocket != wSocket)
                return;

            HTTPManager.Logger.Information("WebSocketTransport", "WSocket_OnOpen");

            OnConnected();
        }

        void WSocket_OnMessage(WebSocket.WebSocket webSocket, string message)
        {
            if (webSocket != wSocket)
                return;

            IServerMessage msg = TransportBase.Parse(Connection.JsonEncoder, message);

            if (msg != null)
                Connection.OnMessage(msg);
        }

        void WSocket_OnClosed(WebSocket.WebSocket webSocket, ushort code, string message)
        {
            if (webSocket != wSocket)
                return;

            string reason = code.ToString() + " : " + message;

            HTTPManager.Logger.Information("WebSocketTransport", "WSocket_OnClosed " + reason);

            if (this.State == TransportStates.Closing)
                this.State = TransportStates.Closed;
            else
                Connection.Error(reason);
        }

        void WSocket_OnError(WebSocket.WebSocket webSocket, string reason)
        {
            if (webSocket != wSocket)
                return;

            // On WP8.1, somehow we receive an exception that the remote server forcibly closed the connection instead of the
            // WebSocket closed packet... Also, even the /abort request didn't finished.
            if (this.State == TransportStates.Closing ||
                this.State == TransportStates.Closed)
            {
                base.AbortFinished();
            }
            else
            {
                if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                    HTTPManager.Logger.Error("WebSocketTransport", "WSocket_OnError " + reason);

                this.State = TransportStates.Closed;
                Connection.Error(reason);
            }
        }

#endregion
    }
}

#endif
#endif
