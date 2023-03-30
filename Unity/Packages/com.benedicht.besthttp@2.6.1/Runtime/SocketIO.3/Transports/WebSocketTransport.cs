#if !BESTHTTP_DISABLE_SOCKETIO
#if !BESTHTTP_DISABLE_WEBSOCKET

using System;
using System.Collections.Generic;

namespace BestHTTP.SocketIO3.Transports
{
    using BestHTTP.Connections;
    using BestHTTP.PlatformSupport.Memory;
    using BestHTTP.WebSocket;
    using Extensions;

    /// <summary>
    /// A transport implementation that can communicate with a SocketIO server.
    /// </summary>
    internal sealed class WebSocketTransport : ITransport
    {
        public TransportTypes Type { get { return TransportTypes.WebSocket; } }
        public TransportStates State { get; private set; }
        public SocketManager Manager { get; private set; }
        public bool IsRequestInProgress { get { return false; } }
        public bool IsPollingInProgress { get { return false; } }
        public WebSocket Implementation { get; private set; }

        public WebSocketTransport(SocketManager manager)
        {
            State = TransportStates.Closed;
            Manager = manager;
        }

        #region Some ITransport Implementation

        public void Open()
        {
            if (State != TransportStates.Closed)
                return;

            Uri uri = null;
            string baseUrl = new UriBuilder(HTTPProtocolFactory.IsSecureProtocol(Manager.Uri) ? "wss" : "ws",
                                                            Manager.Uri.Host,
                                                            Manager.Uri.Port,
                                                            Manager.Uri.GetRequestPathAndQueryURL()).Uri.ToString();
            string format = "{0}?EIO={1}&transport=websocket{3}";
            if (Manager.Handshake != null)
                format += "&sid={2}";

            bool sendAdditionalQueryParams = !Manager.Options.QueryParamsOnlyForHandshake || (Manager.Options.QueryParamsOnlyForHandshake && Manager.Handshake == null);

            uri = new Uri(string.Format(format,
                                        baseUrl,
                                        Manager.ProtocolVersion,
                                        Manager.Handshake != null ? Manager.Handshake.Sid : string.Empty,
                                        sendAdditionalQueryParams ? Manager.Options.BuildQueryParams() : string.Empty));

            Implementation = new WebSocket(uri);

#if !UNITY_WEBGL || UNITY_EDITOR
            if (this.Manager.Options.HTTPRequestCustomizationCallback != null)
                Implementation.OnInternalRequestCreated = (ws, internalRequest) => this.Manager.Options.HTTPRequestCustomizationCallback(this.Manager, internalRequest);
#endif

            Implementation.OnOpen = OnOpen;
            Implementation.OnMessage = OnMessage;
            Implementation.OnBinary = OnBinary;
            Implementation.OnError = OnError;
            Implementation.OnClosed = OnClosed;

            Implementation.Open();

            State = TransportStates.Connecting;
        }

        /// <summary>
        /// Closes the transport and cleans up resources.
        /// </summary>
        public void Close()
        {
            if (State == TransportStates.Closed)
                return;

            State = TransportStates.Closed;

            if (Implementation != null)
                Implementation.Close();
            else
                HTTPManager.Logger.Warning("WebSocketTransport", "Close - WebSocket Implementation already null!", this.Manager.Context);
            Implementation = null;
        }

        /// <summary>
        /// Polling implementation. With WebSocket it's just a skeleton.
        /// </summary>
        public void Poll()
        {
        }

        #endregion

        #region WebSocket Events

        /// <summary>
        /// WebSocket implementation OnOpen event handler.
        /// </summary>
        private void OnOpen(WebSocket ws)
        {
            if (ws != Implementation)
                return;

            HTTPManager.Logger.Information("WebSocketTransport", "OnOpen", this.Manager.Context);

            State = TransportStates.Opening;

            // Send a Probe packet to test the transport. If we receive back a pong with the same payload we can upgrade
            if (Manager.UpgradingTransport == this)
                Send(this.Manager.Parser.CreateOutgoing(TransportEventTypes.Ping, "probe"));
        }

        /// <summary>
        /// WebSocket implementation OnMessage event handler.
        /// </summary>
        private void OnMessage(WebSocket ws, string message)
        {
            if (ws != Implementation)
                return;

            if (HTTPManager.Logger.Level <= BestHTTP.Logger.Loglevels.All)
                HTTPManager.Logger.Verbose("WebSocketTransport", "OnMessage: " + message, this.Manager.Context);

            IncomingPacket packet = IncomingPacket.Empty;
            try
            {
                packet = this.Manager.Parser.Parse(this.Manager, message);

                if (packet.TransportEvent == TransportEventTypes.Open)
                {
                    packet.DecodedArg = BestHTTP.JSON.LitJson.JsonMapper.ToObject<HandshakeData>(packet.DecodedArg as string);
                }
            }
            catch (Exception ex)
            {
                HTTPManager.Logger.Exception("WebSocketTransport", "OnMessage Packet parsing", ex, this.Manager.Context);
            }

            if (!packet.Equals(IncomingPacket.Empty))
            {
                try
                {
                    OnPacket(packet);
                }
                catch (Exception ex)
                {
                    HTTPManager.Logger.Exception("WebSocketTransport", "OnMessage OnPacket", ex, this.Manager.Context);
                }
            }
        }

        /// <summary>
        /// WebSocket implementation OnBinary event handler.
        /// </summary>
        private void OnBinary(WebSocket ws, byte[] data)
        {
            if (ws != Implementation)
                return;

            if (HTTPManager.Logger.Level <= BestHTTP.Logger.Loglevels.All)
                HTTPManager.Logger.Verbose("WebSocketTransport", "OnBinary", this.Manager.Context);

            IncomingPacket packet = IncomingPacket.Empty;
            try
            {
                packet = this.Manager.Parser.Parse(this.Manager, new BufferSegment(data, 0, data.Length));
            }
            catch (Exception ex)
            {
                HTTPManager.Logger.Exception("WebSocketTransport", "OnBinary Packet parsing", ex, this.Manager.Context);
            }

            if (!packet.Equals(IncomingPacket.Empty))
            {
                try
                {
                    OnPacket(packet);
                }
                catch (Exception ex)
                {
                    HTTPManager.Logger.Exception("WebSocketTransport", "OnBinary OnPacket", ex, this.Manager.Context);
                }
            }
        }

        /// <summary>
        /// WebSocket implementation OnError event handler.
        /// </summary>
        private void OnError(WebSocket ws, string error)
        {
            if (ws != Implementation)
                return;

#if !UNITY_WEBGL || UNITY_EDITOR
            if (string.IsNullOrEmpty(error))
            {
                switch (ws.InternalRequest.State)
                {
                    // The request finished without any problem.
                    case HTTPRequestStates.Finished:
                        if (ws.InternalRequest.Response.IsSuccess || ws.InternalRequest.Response.StatusCode == 101)
                            error = string.Format("Request finished. Status Code: {0} Message: {1}", ws.InternalRequest.Response.StatusCode.ToString(), ws.InternalRequest.Response.Message);
                        else
                            error = string.Format("Request Finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}",
                                                            ws.InternalRequest.Response.StatusCode,
                                                            ws.InternalRequest.Response.Message,
                                                            ws.InternalRequest.Response.DataAsText);
                        break;

                    // The request finished with an unexpected error. The request's Exception property may contain more info about the error.
                    case HTTPRequestStates.Error:
                        error = "Request Finished with Error! : " + ws.InternalRequest.Exception != null ? (ws.InternalRequest.Exception.Message + " " + ws.InternalRequest.Exception.StackTrace) : string.Empty;
                        break;

                    // The request aborted, initiated by the user.
                    case HTTPRequestStates.Aborted:
                        error = "Request Aborted!";
                        break;

                    // Connecting to the server is timed out.
                    case HTTPRequestStates.ConnectionTimedOut:
                        error = "Connection Timed Out!";
                        break;

                    // The request didn't finished in the given time.
                    case HTTPRequestStates.TimedOut:
                        error = "Processing the request Timed Out!";
                        break;
                }
            }
#endif

            if (Manager.UpgradingTransport != this)
                (Manager as IManager).OnTransportError(this, error);
            else
                Manager.UpgradingTransport = null;
        }

        /// <summary>
        /// WebSocket implementation OnClosed event handler.
        /// </summary>
        private void OnClosed(WebSocket ws, ushort code, string message)
        {
            if (ws != Implementation)
              return;

            HTTPManager.Logger.Information("WebSocketTransport", "OnClosed", this.Manager.Context);

            Close();

            if (Manager.UpgradingTransport != this)
                (Manager as IManager).TryToReconnect();
            else
                Manager.UpgradingTransport = null;
        }

#endregion

#region Packet Sending Implementation

        /// <summary>
        /// A WebSocket implementation of the packet sending.
        /// </summary>
        public void Send(OutgoingPacket packet)
        {
            if (State == TransportStates.Closed ||
                State == TransportStates.Paused)
            {
                HTTPManager.Logger.Information("WebSocketTransport", string.Format("Send - State == {0}, skipping packet sending!", State), this.Manager.Context);
                return;
            }

            if (packet.IsBinary)
                Implementation.Send(packet.PayloadData.Data, (ulong)packet.PayloadData.Offset, (ulong)packet.PayloadData.Count);
            else
                Implementation.Send(packet.Payload);

            if (packet.Attachements != null)
                for (int i = 0; i < packet.Attachements.Count; ++i)
                    Implementation.Send(packet.Attachements[i]);
        }

        /// <summary>
        /// A WebSocket implementation of the packet sending.
        /// </summary>
        public void Send(List<OutgoingPacket> packets)
        {
            for (int i = 0; i < packets.Count; ++i)
                Send(packets[i]);

            packets.Clear();
        }

#endregion

#region Packet Handling

        /// <summary>
        /// Will only process packets that need to upgrade. All other packets are passed to the Manager.
        /// </summary>
        private void OnPacket(IncomingPacket packet)
        {
            switch (packet.TransportEvent)
            {
                case TransportEventTypes.Open:
                    if (this.State != TransportStates.Opening)
                        HTTPManager.Logger.Warning("WebSocketTransport", "Received 'Open' packet while state is '" + State.ToString() + "'", this.Manager.Context);
                    else
                        State = TransportStates.Open;
                    goto default;

                case TransportEventTypes.Pong:
                    // Answer for a Ping Probe.
                    if ("probe".Equals(packet.DecodedArg))
                    {
                        State = TransportStates.Open;
                        (Manager as IManager).OnTransportProbed(this);
                    }

                    goto default;

                default:
                    if (Manager.UpgradingTransport != this)
                        (Manager as IManager).OnPacket(packet);
                    break;
            }
        }

#endregion
    }
}

#endif
#endif
