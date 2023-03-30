#if !BESTHTTP_DISABLE_SOCKETIO

using System;
using System.Collections.Generic;

using BestHTTP.SocketIO3.Transports;
using BestHTTP.Extensions;
using BestHTTP.SocketIO3.Parsers;
using BestHTTP.SocketIO3.Events;
using BestHTTP.Logger;

namespace BestHTTP.SocketIO3
{
    public sealed class SocketManager : IHeartbeat, IManager
    {
        /// <summary>
        /// Possible states of a SocketManager instance.
        /// </summary>
        public enum States
        {
            /// <summary>
            /// Initial state of the SocketManager
            /// </summary>
            Initial,

            /// <summary>
            /// The SocketManager is currently opening.
            /// </summary>
            Opening,

            /// <summary>
            /// The SocketManager is open, events can be sent to the server.
            /// </summary>
            Open,

            /// <summary>
            /// Paused for transport upgrade
            /// </summary>
            Paused,

            /// <summary>
            /// An error occurred, the SocketManager now trying to connect again to the server.
            /// </summary>
            Reconnecting,

            /// <summary>
            /// The SocketManager is closed, initiated by the user or by the server
            /// </summary>
            Closed
        }

        /// <summary>
        /// Supported Socket.IO protocol version
        /// </summary>
        public int ProtocolVersion { get { return 4; } }

        #region Public Properties

        /// <summary>
        /// The current state of this Socket.IO manager.
        /// </summary>
        public States State { get { return state; } private set { PreviousState = state; state = value; } }
        private States state;

        /// <summary>
        /// The SocketOptions instance that this manager will use.
        /// </summary>
        public SocketOptions Options { get; private set; }

        /// <summary>
        /// The Uri to the Socket.IO endpoint.
        /// </summary>
        public Uri Uri { get; private set; }

        /// <summary>
        /// The server sent and parsed Handshake data.
        /// </summary>
        public HandshakeData Handshake { get; private set; }

        /// <summary>
        /// The currently used main transport instance.
        /// </summary>
        public ITransport Transport { get; private set; }

        /// <summary>
        /// The Request counter for request-based transports.
        /// </summary>
        public ulong RequestCounter { get; internal set; }

        /// <summary>
        /// The root("/") Socket.
        /// </summary>
        public Socket Socket { get { return GetSocket(); } }

        /// <summary>
        /// Indexer to access socket associated to the given namespace.
        /// </summary>
        public Socket this[string nsp] { get { return GetSocket(nsp); } }

        /// <summary>
        /// How many reconnect attempts made.
        /// </summary>
        public int ReconnectAttempts { get; private set; }

        /// <summary>
        /// Parser to encode and decode messages and create strongly typed objects.
        /// </summary>
        public IParser Parser { get; set; }

        /// <summary>
        /// Logging context of this socket.io connection.
        /// </summary>
        public LoggingContext Context { get; private set; }

        #endregion

        #region Internal Properties

        /// <summary>
        /// Timestamp support to the request based transports.
        /// </summary>
        internal UInt64 Timestamp { get { return (UInt64)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds; } }

        /// <summary>
        /// Auto-incrementing property to return Ack ids.
        /// </summary>
        internal int NextAckId { get { return System.Threading.Interlocked.Increment(ref nextAckId); } }
        private int nextAckId;

        /// <summary>
        /// Internal property to store the previous state of the manager.
        /// </summary>
        internal States PreviousState { get; private set; }

        /// <summary>
        /// Transport currently upgrading.
        /// </summary>
        internal ITransport UpgradingTransport { get; set; }

        #endregion

        #region Privates

        /// <summary>
        /// Namespace name -> Socket mapping
        /// </summary>
        private Dictionary<string, Socket> Namespaces = new Dictionary<string, Socket>();

        /// <summary>
        /// List of the sockets to able to iterate over them easily.
        /// </summary>
        private List<Socket> Sockets = new List<Socket>();

        /// <summary>
        /// List of unsent packets. Only instantiated when we have to use it.
        /// </summary>
        private List<OutgoingPacket> OfflinePackets;

        /// <summary>
        /// When we sent out the last heartbeat(Ping) message.
        /// </summary>
        private DateTime LastHeartbeat = DateTime.MinValue;

        /// <summary>
        /// When we have to try to do a reconnect attempt
        /// </summary>
        private DateTime ReconnectAt;

        /// <summary>
        /// When we started to connect to the server.
        /// </summary>
        private DateTime ConnectionStarted;

        /// <summary>
        /// Private flag to avoid multiple Close call
        /// </summary>
        private bool closing;

        /// <summary>
        /// In Engine.io v4 / socket.io v3 the server sends the ping messages, not the client.
        /// </summary>
        private DateTime lastPingReceived;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor to create a SocketManager instance that will connect to the given uri.
        /// </summary>
        public SocketManager(Uri uri)
            :this(uri, new DefaultJsonParser(), new SocketOptions())
        { }

        public SocketManager(Uri uri, IParser parser)
            : this(uri, parser, new SocketOptions())
        { }

        public SocketManager(Uri uri, SocketOptions options)
            :this(uri, new DefaultJsonParser(), options)
        { }

        /// <summary>
        /// Constructor to create a SocketManager instance.
        /// </summary>
        public SocketManager(Uri uri, IParser parser, SocketOptions options)
        {
            this.Context = new LoggingContext(this);

            string path = uri.PathAndQuery;
            if (path.Length <= 1)
            {
                string append;
                if (uri.OriginalString[uri.OriginalString.Length - 1] == '/')
                    append = "socket.io/";
                else
                    append = "/socket.io/";

                uri = new Uri(uri.OriginalString + append);
            }

            this.Uri = uri;
            this.Options = options ?? new SocketOptions();
            this.State = States.Initial;
            this.PreviousState = States.Initial;
            this.Parser = parser ?? new DefaultJsonParser();
        }

        #endregion

        /// <summary>
        /// Returns with the "/" namespace, the same as the Socket property.
        /// </summary>
        public Socket GetSocket()
        {
            return GetSocket("/");
        }

        /// <summary>
        /// Returns with the specified namespace
        /// </summary>
        public Socket GetSocket(string nsp)
        {
            if (string.IsNullOrEmpty(nsp))
                throw new ArgumentNullException("Namespace parameter is null or empty!");

            /*if (nsp[0] != '/')
                nsp = "/" + nsp;*/

            Socket socket = null;
            if (!Namespaces.TryGetValue(nsp, out socket))
            {
                // No socket found, create one
                socket = new Socket(nsp, this);

                Namespaces.Add(nsp, socket);
                Sockets.Add(socket);

                (socket as ISocket).Open();
            }

            return socket;
        }

        /// <summary>
        /// Internal function to remove a Socket instance from this manager.
        /// </summary>
        /// <param name="socket"></param>
        void IManager.Remove(Socket socket)
        {
            Namespaces.Remove(socket.Namespace);
            Sockets.Remove(socket);

            if (Sockets.Count == 0)
                Close();
        }

        #region Connection to the server, and upgrading

        /// <summary>
        /// This function will begin to open the Socket.IO connection by sending out the handshake request.
        /// If the Options' AutoConnect is true, it will be called automatically.
        /// </summary>
        public void Open()
        {
            if (State != States.Initial &&
                State != States.Closed &&
                State != States.Reconnecting)
                return;

            HTTPManager.Logger.Information("SocketManager", "Opening", this.Context);

            ReconnectAt = DateTime.MinValue;

            switch (Options.ConnectWith)
            {
                case TransportTypes.Polling: Transport = new PollingTransport(this); break;
#if !BESTHTTP_DISABLE_WEBSOCKET
                case TransportTypes.WebSocket:
                    Transport = new WebSocketTransport(this);
                    break;
#endif
            }
            Transport.Open();


            (this as IManager).EmitEvent("connecting");

            State = States.Opening;

            ConnectionStarted = DateTime.UtcNow;

            HTTPManager.Heartbeats.Subscribe(this);

            // The root namespace will be opened by default
            //GetSocket("/");
        }

        /// <summary>
        /// Closes this Socket.IO connection.
        /// </summary>
        public void Close()
        {
            (this as IManager).Close(true);
        }

        /// <summary>
        /// Closes this Socket.IO connection.
        /// </summary>
        void IManager.Close(bool removeSockets)
        {
            if (State == States.Closed || closing)
                return;
            closing = true;

            HTTPManager.Logger.Information("SocketManager", "Closing", this.Context);

            HTTPManager.Heartbeats.Unsubscribe(this);

            // Disconnect the sockets. The Disconnect function will call the Remove function to remove it from the Sockets list.
            if (removeSockets)
                while (Sockets.Count > 0)
                    (Sockets[Sockets.Count - 1] as ISocket).Disconnect(removeSockets);
            else
                for (int i = 0; i < Sockets.Count; ++i)
                    (Sockets[i] as ISocket).Disconnect(removeSockets);

            // Set to Closed after Socket's Disconnect. This way we can send the disconnect events to the server.
            State = States.Closed;

            LastHeartbeat = DateTime.MinValue;
            lastPingReceived = DateTime.MinValue;

            if (removeSockets && OfflinePackets != null)
                OfflinePackets.Clear();

            // Remove the references from the dictionary too.
            if (removeSockets)
                Namespaces.Clear();

            Handshake = null;

            if (Transport != null)
                Transport.Close();
            Transport = null;

            if (UpgradingTransport != null)
                UpgradingTransport.Close();
            UpgradingTransport = null;

            closing = false;
        }

        /// <summary>
        /// Called from a ITransport implementation when an error occurs and we may have to try to reconnect.
        /// </summary>
        void IManager.TryToReconnect()
        {
            if (State == States.Reconnecting ||
                State == States.Closed)
                return;

            if (!Options.Reconnection || HTTPManager.IsQuitting)
            {
                Close();

                return;
            }

            if (++ReconnectAttempts >= Options.ReconnectionAttempts)
            {
                (this as IManager).EmitEvent("reconnect_failed");
                Close();

                return;
            }

            Random rand = new Random();

            int delay = (int)Options.ReconnectionDelay.TotalMilliseconds * ReconnectAttempts;

            ReconnectAt = DateTime.UtcNow +
                          TimeSpan.FromMilliseconds(Math.Min(rand.Next(/*rand min:*/(int)(delay - (delay * Options.RandomizationFactor)),
                                                                       /*rand max:*/(int)(delay + (delay * Options.RandomizationFactor))),
                                                             (int)Options.ReconnectionDelayMax.TotalMilliseconds));

            (this as IManager).Close(false);

            State = States.Reconnecting;

            for (int i = 0; i < Sockets.Count; ++i)
                (Sockets[i] as ISocket).Open();

            // In the Close() function we unregistered
            HTTPManager.Heartbeats.Subscribe(this);

            HTTPManager.Logger.Information("SocketManager", "Reconnecting", this.Context);
        }

        /// <summary>
        /// Called by transports when they are connected to the server.
        /// </summary>
        bool IManager.OnTransportConnected(ITransport trans)
        {
            HTTPManager.Logger.Information("SocketManager", string.Format("OnTransportConnected State: {0}, PreviousState: {1}, Current Transport: {2}, Upgrading Transport: {3}", this.State, this.PreviousState, trans.Type, UpgradingTransport != null ? UpgradingTransport.Type.ToString() : "null"), this.Context);

            if (State != States.Opening)
                return false;

            if (PreviousState == States.Reconnecting)
                (this as IManager).EmitEvent("reconnect");

            State = States.Open;

            if (PreviousState == States.Reconnecting)
                (this as IManager).EmitEvent("reconnect_before_offline_packets");

            for (int i = 0; i < Sockets.Count; ++i)
            {
                var socket = Sockets[i];
                if (socket != null)
                    socket.OnTransportOpen();
            }

            ReconnectAttempts = 0;

            // Send out packets that we collected while there were no available transport.
            SendOfflinePackets();

#if !BESTHTTP_DISABLE_WEBSOCKET
            // Can we upgrade to WebSocket transport?
            if (Transport.Type != TransportTypes.WebSocket &&
                Handshake.Upgrades.Contains("websocket"))
            {
                UpgradingTransport = new WebSocketTransport(this);
                UpgradingTransport.Open();
            }
#endif

            return true;
        }

        void IManager.OnTransportError(ITransport trans, string err)
        {
            if (UpgradingTransport != null && trans != UpgradingTransport)
                return;

            (this as IManager).EmitError(err);

            trans.Close();
            (this as IManager).TryToReconnect();
        }

        void IManager.OnTransportProbed(ITransport trans)
        {
            HTTPManager.Logger.Information("SocketManager", "\"probe\" packet received", this.Context);

            // If we have to reconnect, we will go straight with the transport we were able to upgrade
            Options.ConnectWith = trans.Type;

            // Pause ourself to wait for any send and receive turn to finish.
            State = States.Paused;
        }

        #endregion

        #region Packet Handling

        /// <summary>
        /// Select the best transport to send out packets.
        /// </summary>
        private ITransport SelectTransport()
        {
            if (State != States.Open || Transport == null)
                return null;

            return Transport.IsRequestInProgress ? null : Transport;
        }

        /// <summary>
        /// Will select the best transport and sends out all packets that are in the OfflinePackets list.
        /// </summary>
        private void SendOfflinePackets()
        {
            ITransport trans = SelectTransport();

            // Send out packets that we not sent while no transport was available.
            // This function is called before the event handlers get the 'connected' event, so
            // theoretically the packet orders are remains.
            if (OfflinePackets != null && OfflinePackets.Count > 0 && trans != null)
            {
                trans.Send(OfflinePackets);
                OfflinePackets.Clear();
            }
        }

        /// <summary>
        /// Internal function that called from the Socket class. It will send out the packet instantly, or if no transport is available it will store
        /// the packet in the OfflinePackets list.
        /// </summary>
        void IManager.SendPacket(OutgoingPacket packet)
        {
            HTTPManager.Logger.Information("SocketManager", "SendPacket " + packet.ToString(), this.Context);

            ITransport trans = SelectTransport();

            if (trans != null)
            {
                try
                {
                    trans.Send(packet);
                }
                catch(Exception ex)
                {
                    (this as IManager).EmitError(ex.Message + " " + ex.StackTrace);
                }
            }
            else
            {
                if (packet.IsVolatile)
                    return;

                HTTPManager.Logger.Information("SocketManager", "SendPacket - Offline stashing packet", this.Context);

                if (OfflinePackets == null)
                    OfflinePackets = new List<OutgoingPacket>();

                // The same packet can be sent through multiple Sockets.
                OfflinePackets.Add(packet);
            }
        }

        /// <summary>
        /// Called from the currently operating Transport. Will pass forward to the Socket that has to call the callbacks.
        /// </summary>
        void IManager.OnPacket(IncomingPacket packet)
        {
            if (State == States.Closed)
            {
                HTTPManager.Logger.Information("SocketManager", "OnPacket - State == States.Closed", this.Context);
                return;
            }

            switch(packet.TransportEvent)
            {
                case TransportEventTypes.Open:
                    if (Handshake == null)
                    {
                        Handshake = packet.DecodedArg as HandshakeData;

                        (this as IManager).OnTransportConnected(Transport);

                        return;
                    }
                    else
                        HTTPManager.Logger.Information("SocketManager", "OnPacket - Already received handshake data!", this.Context);
                    break;

                case TransportEventTypes.Ping:
                    lastPingReceived = DateTime.UtcNow;
                    //IncomingPacket pingPacket = new Packet(TransportEventTypes.Pong, SocketIOEventTypes.Unknown, "/", 0);
                    
                    (this as IManager).SendPacket(this.Parser.CreateOutgoing(TransportEventTypes.Pong, null));
                    break;

                case TransportEventTypes.Pong: break;
            }

            Socket socket = null;
            if (Namespaces.TryGetValue(packet.Namespace, out socket))
                (socket as ISocket).OnPacket(packet);
            else if (packet.TransportEvent == TransportEventTypes.Message)
                HTTPManager.Logger.Warning("SocketManager", "Namespace \"" + packet.Namespace + "\" not found!", this.Context);
        }

        #endregion

        /// <summary>
        /// Sends an event to all available namespaces.
        /// </summary>
        public void EmitAll(string eventName, params object[] args)
        {
            for (int i = 0; i < Sockets.Count; ++i)
                Sockets[i].Emit(eventName, args);
        }

        /// <summary>
        /// Emits an internal packet-less event to the root namespace without creating it if it isn't exists yet.
        /// </summary>
        void IManager.EmitEvent(string eventName, params object[] args)
        {
            Socket socket = null;
            if (Namespaces.TryGetValue("/", out socket))
                (socket as ISocket).EmitEvent(eventName, args);
        }

        /// <summary>
        /// Emits an internal packet-less event to the root namespace without creating it if it isn't exists yet.
        /// </summary>
        void IManager.EmitEvent(SocketIOEventTypes type, params object[] args)
        {
            (this as IManager).EmitEvent(EventNames.GetNameFor(type), args);
        }

        void IManager.EmitError(string msg)
        {
            var outcoming = this.Parser.CreateOutgoing(this.Sockets[0], SocketIOEventTypes.Error, -1, null, new Error(msg));
            IncomingPacket inc = IncomingPacket.Empty;
            if (outcoming.IsBinary)
                inc = this.Parser.Parse(this, outcoming.PayloadData);
            else
                inc = this.Parser.Parse(this, outcoming.Payload);

            (this as IManager).EmitEvent(SocketIOEventTypes.Error, inc.DecodedArg ?? inc.DecodedArgs);
        }

        void IManager.EmitAll(string eventName, params object[] args)
        {
            for (int i = 0; i < Sockets.Count; ++i)
                (Sockets[i] as ISocket).EmitEvent(eventName, args);
        }

        #region IHeartbeat Implementation

        /// <summary>
        /// Called from the HTTPManager's OnUpdate function every frame. It's main function is to send out heartbeat messages.
        /// </summary>
        void IHeartbeat.OnHeartbeatUpdate(TimeSpan dif)
        {
            switch (State)
            {
                case States.Paused:
                    // To ensure no messages are lost, the upgrade packet will only be sent once all the buffers of the existing transport are flushed and the transport is considered paused.
                    if (!Transport.IsRequestInProgress &&
                        !Transport.IsPollingInProgress)
                    {
                        State = States.Open;

                        // Close the current transport
                        Transport.Close();

                        // and switch to the newly upgraded one
                        Transport = UpgradingTransport;
                        UpgradingTransport = null;

                        // We will send an Upgrade("5") packet.
                        Transport.Send(this.Parser.CreateOutgoing(TransportEventTypes.Upgrade, null));

                        goto case States.Open;
                    }
                    break;

                case States.Opening:
                    if (DateTime.UtcNow - ConnectionStarted >= Options.Timeout)
                    {
                        (this as IManager).EmitError("Connection timed out!");
                        (this as IManager).EmitEvent("connect_error");
                        (this as IManager).EmitEvent("connect_timeout");
                        (this as IManager).TryToReconnect();
                    }

                    break;

                case States.Reconnecting:
                    if (ReconnectAt != DateTime.MinValue && DateTime.UtcNow >= ReconnectAt)
                    {
                        (this as IManager).EmitEvent("reconnect_attempt");
                        (this as IManager).EmitEvent("reconnecting");

                        Open();
                    }
                    break;

                case States.Open:
                    ITransport trans = null;

                    // Select transport to use
                    if (Transport != null && Transport.State == TransportStates.Open)
                        trans = Transport;

                    // not yet open?
                    if (trans == null || trans.State != TransportStates.Open)
                        return;

                    // Start to poll the server for events
                    trans.Poll();

                    // Start to send out unsent packets
                    SendOfflinePackets();

                    // First time we reached this point. Set the LastHeartbeat to the current time, 'cause we are just opened.
                    if (LastHeartbeat == DateTime.MinValue)
                    {
                        LastHeartbeat = DateTime.UtcNow;
                        lastPingReceived = DateTime.UtcNow;
                        return;
                    }

                    if (DateTime.UtcNow - lastPingReceived > TimeSpan.FromMilliseconds(Handshake.PingInterval + Handshake.PingTimeout))
                        (this as IManager).TryToReconnect();

                    break; // case States.Open:
            }
        }

        #endregion

    }
}

#endif
