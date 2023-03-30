#if !BESTHTTP_DISABLE_SIGNALR

using System;
using System.Text;
using System.Collections.Generic;

using BestHTTP.Extensions;
using BestHTTP.SignalR.Hubs;
using BestHTTP.SignalR.Messages;
using BestHTTP.SignalR.Transports;
using BestHTTP.SignalR.JsonEncoders;
using BestHTTP.SignalR.Authentication;

using PlatformSupport.Collections.ObjectModel;
using BestHTTP.Connections;

#if !NETFX_CORE
using PlatformSupport.Collections.Specialized;
#else
    using System.Collections.Specialized;
#endif

namespace BestHTTP.SignalR
{
    public delegate void OnNonHubMessageDelegate(Connection connection, object data);
    public delegate void OnConnectedDelegate(Connection connection);
    public delegate void OnClosedDelegate(Connection connection);
    public delegate void OnErrorDelegate(Connection connection, string error);
    public delegate void OnStateChanged(Connection connection, ConnectionStates oldState, ConnectionStates newState);
    public delegate void OnPrepareRequestDelegate(Connection connection, HTTPRequest req, RequestTypes type);

    /// <summary>
    /// Interface to be able to hide internally used functions and properties.
    /// </summary>
    public interface IConnection
    {
        ProtocolVersions Protocol { get; }
        NegotiationData NegotiationResult { get; }
        IJsonEncoder JsonEncoder { get; set; }

        void OnMessage(IServerMessage msg);
        void TransportStarted();
        void TransportReconnected();
        void TransportAborted();
        void Error(string reason);
        Uri BuildUri(RequestTypes type);
        Uri BuildUri(RequestTypes type, TransportBase transport);
        HTTPRequest PrepareRequest(HTTPRequest req, RequestTypes type);
        string ParseResponse(string responseStr);
    }

    /// <summary>
    /// Supported versions of the SignalR protocol.
    /// </summary>
    public enum ProtocolVersions : byte
    {
        Protocol_2_0,
        Protocol_2_1,
        Protocol_2_2
    }

    /// <summary>
    /// The main SignalR class. This is the entry point to connect to a SignalR service.
    /// </summary>
    public sealed class Connection : IHeartbeat, IConnection
    {
        #region Public Properties

        /// <summary>
        /// The default Json encode/decoder that will be used to encode/decode the event arguments.
        /// </summary>
        public static IJsonEncoder DefaultEncoder =
#if BESTHTTP_SIGNALR_WITH_JSONDOTNET
            new JSonDotnetEncoder();
#else
            new DefaultJsonEncoder();
#endif

        /// <summary>
        /// The base url endpoint where the SignalR service can be found.
        /// </summary>
        public Uri Uri { get; private set; }

        /// <summary>
        /// Current State of the SignalR connection.
        /// </summary>
        public ConnectionStates State
        {
            get { return _state; }
            private set
            {
                ConnectionStates old = _state;
                _state = value;

                if (OnStateChanged != null)
                    OnStateChanged(this, old, _state);
            }
        }
        private ConnectionStates _state;

        /// <summary>
        /// Result of the negotiation request from the server.
        /// </summary>
        public NegotiationData NegotiationResult { get; private set; }

        /// <summary>
        /// The hubs that the client is connected to.
        /// </summary>
        public Hub[] Hubs { get; private set; }

        /// <summary>
        /// The transport that is used to send and receive messages.
        /// </summary>
        public TransportBase Transport { get; private set; }

        /// <summary>
        /// Current client protocol in use.
        /// </summary>
        public ProtocolVersions Protocol { get; private set; }

        /// <summary>
        /// Additional query parameters that will be passed for the handshake uri. If the value is null, or an empty string it will be not appended to the query only the key.
        /// <remarks>The keys and values must be escaped properly, as the plugin will not escape these. </remarks>
        /// </summary>
        public ObservableDictionary<string, string> AdditionalQueryParams
        {
            get { return additionalQueryParams; }
            set
            {
                // Unsubscribe from previous dictionary's events
                if (additionalQueryParams != null)
                    additionalQueryParams.CollectionChanged -= AdditionalQueryParams_CollectionChanged;

                additionalQueryParams = value;

                // Clear out the cached value
                BuiltQueryParams = null;

                // Subscribe to the collection changed event
                if (value != null)
                    value.CollectionChanged += AdditionalQueryParams_CollectionChanged;
            }
        }
        private ObservableDictionary<string, string> additionalQueryParams;

        /// <summary>
        /// If it's false, the parameters in the AdditionalQueryParams will be passed for all http requests. Its default value is true.
        /// </summary>
        public bool QueryParamsOnlyForHandshake { get; set; }

        /// <summary>
        /// The Json encoder that will be used by the connection and the transport.
        /// </summary>
        public IJsonEncoder JsonEncoder { get; set; }

        /// <summary>
        /// An IAuthenticationProvider implementation that will be used to authenticate the connection.
        /// </summary>
        public IAuthenticationProvider AuthenticationProvider { get; set; }

        /// <summary>
        /// How much time we have to wait between two pings.
        /// </summary>
        public TimeSpan PingInterval { get; set; }

        /// <summary>
        /// Wait time before the plugin should do a reconnect attempt. Its default value is 5 seconds.
        /// </summary>
        public TimeSpan ReconnectDelay { get; set; }

        #endregion

        #region Public Events

        /// <summary>
        /// Called when the protocol is open for communication.
        /// </summary>
        public event OnConnectedDelegate OnConnected;

        /// <summary>
        /// Called when the connection is closed, and no further messages are sent or received.
        /// </summary>
        public event OnClosedDelegate OnClosed;

        /// <summary>
        /// Called when an error occures. If the connection is already Started, it will try to do a reconnect, otherwise it will close the connection.
        /// </summary>
        public event OnErrorDelegate OnError;

        /// <summary>
        /// This event called when a reconnection attempt are started. If fails to reconnect an OnError and OnClosed events are called.
        /// </summary>
        public event OnConnectedDelegate OnReconnecting;

        /// <summary>
        /// This event called when the reconnection attempt succeded.
        /// </summary>
        public event OnConnectedDelegate OnReconnected;

        /// <summary>
        /// Called every time when the connection's state changes.
        /// </summary>
        public event OnStateChanged OnStateChanged;

        /// <summary>
        /// It's called when a non-Hub message received. The data can be anything from primitive types to array of complex objects.
        /// </summary>
        public event OnNonHubMessageDelegate OnNonHubMessage;

        /// <summary>
        /// With this delegate all requests can be further customized.
        /// </summary>
        public OnPrepareRequestDelegate RequestPreparator { get; set; }

        #endregion

        #region Indexers

        /// <summary>
        /// Indexer property the access hubs by index.
        /// </summary>
        public Hub this[int idx] { get { return Hubs[idx] as Hub; } }

        /// <summary>
        /// Indexer property the access hubs by name.
        /// </summary>
        public Hub this[string hubName]
        {
            get
            {
                for (int i = 0; i < Hubs.Length; ++i)
                {
                    Hub hub = Hubs[i] as Hub;
                    if (hub.Name.Equals(hubName, StringComparison.OrdinalIgnoreCase))
                        return hub;
                }

                return null;
            }
        }

        #endregion

        #region Internals

        /// <summary>
        /// Unique ID for all message sent by the client.
        /// </summary>
        internal long ClientMessageCounter;

        #endregion

        #region Privates

        /// <summary>
        /// Supported client protocol versions.
        /// </summary>
        private readonly string[] ClientProtocols = new string[] { "1.3", "1.4", "1.5" };

        /// <summary>
        /// A timestamp that will be sent with all request for easier debugging.
        /// </summary>
        private UInt32 Timestamp { get { return (UInt32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).Ticks; } }

        /// <summary>
        /// Request counter sent with all request for easier debugging.
        /// </summary>
        private long RequestCounter;

        /// <summary>
        /// Instance of the last received message. Used for its MessageId.
        /// </summary>
        private MultiMessage LastReceivedMessage;

        /// <summary>
        /// The GroupsToken sent by the server that stores what groups we are joined to.
        /// We will send it with the reconnect request.
        /// </summary>
        private string GroupsToken;

        /// <summary>
        /// Received messages before the Start request finishes.
        /// </summary>
        private List<IServerMessage> BufferedMessages;

        /// <summary>
        /// When the last message received from the server. Used for reconnecting.
        /// </summary>
        private DateTime LastMessageReceivedAt;

        /// <summary>
        /// When we started to reconnect. When too much time passes without a successful reconnect, we will close the connection.
        /// </summary>
        private DateTime ReconnectStartedAt;

        private DateTime ReconnectDelayStartedAt;

        /// <summary>
        /// True, if the reconnect process started.
        /// </summary>
        private bool ReconnectStarted;

        /// <summary>
        /// When the last ping request sent out.
        /// </summary>
        private DateTime LastPingSentAt;

        /// <summary>
        /// Reference to the ping request.
        /// </summary>
        private HTTPRequest PingRequest;

        /// <summary>
        /// When the transport started the connection process
        /// </summary>
        private DateTime? TransportConnectionStartedAt;

        /// <summary>
        /// Cached StringBuilder instance used in BuildUri
        /// </summary>
        private StringBuilder queryBuilder = new StringBuilder();

        /// <summary>
        /// Builds and returns with the connection data made from the hub names.
        /// </summary>
        private string ConnectionData
        {
            get
            {
                if (!string.IsNullOrEmpty(BuiltConnectionData))
                    return BuiltConnectionData;

                StringBuilder sb = new StringBuilder("[", Hubs.Length * 4);

                if (Hubs != null)
                    for (int i = 0; i < Hubs.Length; ++i)
                    {
                        sb.Append(@"{""Name"":""");
                        sb.Append(Hubs[i].Name);
                        sb.Append(@"""}");

                        if (i < Hubs.Length - 1)
                            sb.Append(",");
                    }

                sb.Append("]");

                return BuiltConnectionData = Uri.EscapeUriString(sb.ToString());
            }
        }

        /// <summary>
        /// The cached value of the result of the ConnectionData property call.
        /// </summary>
        private string BuiltConnectionData;

        /// <summary>
        /// Builds the keys and values from the AdditionalQueryParams to an key=value form. If AdditionalQueryParams is null or empty, it will return an empty string.
        /// </summary>
        private string QueryParams
        {
            get
            {
                if (AdditionalQueryParams == null || AdditionalQueryParams.Count == 0)
                    return string.Empty;

                if (!string.IsNullOrEmpty(BuiltQueryParams))
                    return BuiltQueryParams;

                StringBuilder sb = new StringBuilder(AdditionalQueryParams.Count * 4);

                foreach (var kvp in AdditionalQueryParams)
                {
                    sb.Append("&");
                    sb.Append(kvp.Key);

                    if (!string.IsNullOrEmpty(kvp.Value))
                    {
                        sb.Append("=");
                        sb.Append(Uri.EscapeDataString(kvp.Value));
                    }
                }

                return BuiltQueryParams = sb.ToString();
            }
        }

        /// <summary>
        /// The cached value of the result of the QueryParams property call.
        /// </summary>
        private string BuiltQueryParams;

        private SupportedProtocols NextProtocolToTry;

        #endregion

        #region Constructors

        public Connection(Uri uri, params string[] hubNames)
            : this(uri)
        {
            if (hubNames != null && hubNames.Length > 0)
            {
                this.Hubs = new Hub[hubNames.Length];

                for (int i = 0; i < hubNames.Length; ++i)
                    this.Hubs[i] = new Hub(hubNames[i], this);
            }
        }

        public Connection(Uri uri, params Hub[] hubs)
            :this(uri)
        {
            this.Hubs = hubs;
            if (hubs != null)
                for (int i = 0; i < hubs.Length; ++i)
                    (hubs[i] as IHub).Connection = this;
        }

        public Connection(Uri uri)
        {
            this.State = ConnectionStates.Initial;
            this.Uri = uri;

            this.JsonEncoder = Connection.DefaultEncoder;
            this.PingInterval = TimeSpan.FromMinutes(5);

            // Expected protocol
            this.Protocol = ProtocolVersions.Protocol_2_2;

            this.ReconnectDelay = TimeSpan.FromSeconds(5);
        }

        #endregion

        #region Starting the protocol

        /// <summary>
        /// This function will start to authenticate if required, and the SignalR protocol negotiation.
        /// </summary>
        public void Open()
        {
            if (State != ConnectionStates.Initial && State != ConnectionStates.Closed)
                return;

            if (AuthenticationProvider != null && AuthenticationProvider.IsPreAuthRequired)
            {
                this.State = ConnectionStates.Authenticating;

                AuthenticationProvider.OnAuthenticationSucceded += OnAuthenticationSucceded;
                AuthenticationProvider.OnAuthenticationFailed += OnAuthenticationFailed;

                // Start the authentication process
                AuthenticationProvider.StartAuthentication();
            }
            else
                StartImpl();
        }

        /// <summary>
        /// Called when the authentication succeeded.
        /// </summary>
        /// <param name="provider"></param>
        private void OnAuthenticationSucceded(IAuthenticationProvider provider)
        {
            provider.OnAuthenticationSucceded -= OnAuthenticationSucceded;
            provider.OnAuthenticationFailed -= OnAuthenticationFailed;

            StartImpl();
        }

        /// <summary>
        /// Called when the authentication failed.
        /// </summary>
        private void OnAuthenticationFailed(IAuthenticationProvider provider, string reason)
        {
            provider.OnAuthenticationSucceded -= OnAuthenticationSucceded;
            provider.OnAuthenticationFailed -= OnAuthenticationFailed;

            (this as IConnection).Error(reason);
        }

        /// <summary>
        /// It's the real Start implementation. It will start the negotiation
        /// </summary>
        private void StartImpl()
        {
            this.State = ConnectionStates.Negotiating;

            NegotiationResult = new NegotiationData(this);
            NegotiationResult.OnReceived = OnNegotiationDataReceived;
            NegotiationResult.OnError = OnNegotiationError;
            NegotiationResult.Start();
        }

        #region Negotiation Event Handlers

        /// <summary>
        /// Protocol negotiation finished successfully.
        /// </summary>
        private void OnNegotiationDataReceived(NegotiationData data)
        {
            // Find out what supported protocol the server speak
            int protocolIdx = -1;
            for (int i = 0; i < ClientProtocols.Length && protocolIdx == -1; ++i)
                if (data.ProtocolVersion == ClientProtocols[i])
                    protocolIdx = i;

            // No supported protocol found? Try using the latest one.
            if (protocolIdx == -1)
            {
                protocolIdx = (byte)ProtocolVersions.Protocol_2_2;
                HTTPManager.Logger.Warning("SignalR Connection", "Unknown protocol version: " + data.ProtocolVersion);
            }

            this.Protocol = (ProtocolVersions)protocolIdx;

            #if !BESTHTTP_DISABLE_WEBSOCKET
            if (data.TryWebSockets)
            {
                Transport = new WebSocketTransport(this);

                #if !BESTHTTP_DISABLE_SERVERSENT_EVENTS
                    NextProtocolToTry = SupportedProtocols.ServerSentEvents;
                #else
                    NextProtocolToTry = SupportedProtocols.HTTP;
                #endif
            }
            else
            #endif
            {
                #if !BESTHTTP_DISABLE_SERVERSENT_EVENTS
                    Transport = new ServerSentEventsTransport(this);

                    // Long-Poll
                    NextProtocolToTry = SupportedProtocols.HTTP;
                #else

                    Transport = new PollingTransport(this);

                    NextProtocolToTry = SupportedProtocols.Unknown;
                #endif
            }

            this.State = ConnectionStates.Connecting;
            TransportConnectionStartedAt = DateTime.UtcNow;

            Transport.Connect();
        }

        /// <summary>
        /// Protocol negotiation failed.
        /// </summary>
        private void OnNegotiationError(NegotiationData data, string error)
        {
            (this as IConnection).Error(error);
        }

        #endregion

        #endregion

        #region Public Interface

        /// <summary>
        /// Closes the connection and shuts down the transport.
        /// </summary>
        public void Close()
        {
            if (this.State == ConnectionStates.Closed)
                return;

            this.State = ConnectionStates.Closed;

            //ReconnectStartedAt = null;
            ReconnectStarted = false;

            TransportConnectionStartedAt = null;

            if (Transport != null)
            {
                Transport.Abort();
                Transport = null;
            }

            NegotiationResult = null;

            HTTPManager.Heartbeats.Unsubscribe(this);

            LastReceivedMessage = null;

            if (Hubs != null)
                for (int i = 0; i < Hubs.Length; ++i)
                    (Hubs[i] as IHub).Close();

            if (BufferedMessages != null)
            {
                BufferedMessages.Clear();
                BufferedMessages = null;
            }

            if (OnClosed != null)
            {
                try
                {
                    OnClosed(this);
                }
                catch (Exception ex)
                {
                    HTTPManager.Logger.Exception("SignalR Connection", "OnClosed", ex);
                }
            }
        }

        /// <summary>
        /// Initiates a reconnect to the SignalR server.
        /// </summary>
        public void Reconnect()
        {
            // Return if reconnect process already started.
            if (ReconnectStarted)
                return;
            ReconnectStarted = true;

            // Set ReconnectStartedAt only when the previous State is not Reconnecting,
            // so we keep the first date&time when we started reconnecting
            if (this.State != ConnectionStates.Reconnecting)
                ReconnectStartedAt = DateTime.UtcNow;

            this.State = ConnectionStates.Reconnecting;

            HTTPManager.Logger.Warning("SignalR Connection", "Reconnecting");

            Transport.Reconnect();

            if (PingRequest != null)
                PingRequest.Abort();

            if (OnReconnecting != null)
            {
                try
                {
                    OnReconnecting(this);
                }
                catch (Exception ex)
                {
                    HTTPManager.Logger.Exception("SignalR Connection", "OnReconnecting", ex);
                }
            }
        }


        /// <summary>
        /// Will encode the argument to a Json string using the Connection's JsonEncoder, then will send it to the server.
        /// </summary>
        /// <returns>True if the plugin was able to send out the message</returns>
        public bool Send(object arg)
        {
            if (arg == null)
                throw new ArgumentNullException("arg");

            if (this.State != ConnectionStates.Connected)
                return false;

            string json = JsonEncoder.Encode(arg);

            if (string.IsNullOrEmpty(json))
                HTTPManager.Logger.Error("SignalR Connection", "Failed to JSon encode the given argument. Please try to use an advanced JSon encoder(check the documentation how you can do it).");
            else
                Transport.Send(json);

            return true;
        }

        /// <summary>
        /// Sends the given json string to the server.
        /// </summary>
        /// <returns>True if the plugin was able to send out the message</returns>
        public bool SendJson(string json)
        {
            if (json == null)
                throw new ArgumentNullException("json");

            if (this.State != ConnectionStates.Connected)
                return false;

            Transport.Send(json);

            return true;
        }

        #endregion

        #region IManager Functions

        /// <summary>
        /// Called when we receive a message from the server
        /// </summary>
        void IConnection.OnMessage(IServerMessage msg)
        {
            if (this.State == ConnectionStates.Closed)
                return;

            // Store messages that we receive while we are connecting
            if (this.State == ConnectionStates.Connecting)
            {
                if (BufferedMessages == null)
                    BufferedMessages = new List<IServerMessage>();

                BufferedMessages.Add(msg);

                return;
            }

            LastMessageReceivedAt = DateTime.UtcNow;

            switch(msg.Type)
            {
                case MessageTypes.Multiple:
                    LastReceivedMessage = msg as MultiMessage;

                    // Not received in the reconnect process, so we can't rely on it
                    if (LastReceivedMessage.IsInitialization)
                        HTTPManager.Logger.Information("SignalR Connection", "OnMessage - Init");

                    if (LastReceivedMessage.GroupsToken != null)
                        GroupsToken = LastReceivedMessage.GroupsToken;

                    if (LastReceivedMessage.ShouldReconnect)
                    {
                        HTTPManager.Logger.Information("SignalR Connection", "OnMessage - Should Reconnect");

                        Reconnect();

                        // Should we return here not processing the messages that may come with it?
                        //return;
                    }

                    if (LastReceivedMessage.Data != null)
                        for (int i = 0; i < LastReceivedMessage.Data.Count; ++i)
                            (this as IConnection).OnMessage(LastReceivedMessage.Data[i]);

                    break;

                case MessageTypes.MethodCall:
                    MethodCallMessage methodCall = msg as MethodCallMessage;

                    Hub hub = this[methodCall.Hub];

                    if (hub != null)
                        (hub as IHub).OnMethod(methodCall);
                    else
                        HTTPManager.Logger.Warning("SignalR Connection", string.Format("Hub \"{0}\" not found!", methodCall.Hub));

                    break;

                case MessageTypes.Result:
                case MessageTypes.Failure:
                case MessageTypes.Progress:
                    UInt64 id = (msg as IHubMessage).InvocationId;
                    hub = FindHub(id);
                    if (hub != null)
                        (hub as IHub).OnMessage(msg);
                    else
                        HTTPManager.Logger.Warning("SignalR Connection", string.Format("No Hub found for Progress message! Id: {0}", id.ToString()));
                    break;

                case MessageTypes.Data:
                    if (OnNonHubMessage != null)
                        OnNonHubMessage(this, (msg as DataMessage).Data);
                    break;

                case MessageTypes.KeepAlive:
                    break;

                default:
                    HTTPManager.Logger.Warning("SignalR Connection", "Unknown message type received: " + msg.Type.ToString());
                    break;
            }
        }

        /// <summary>
        /// Called from the transport implementations when the Start request finishes successfully.
        /// </summary>
        void IConnection.TransportStarted()
        {
            if (this.State != ConnectionStates.Connecting)
                return;

            InitOnStart();

            if (OnConnected != null)
            {
                try
                {
                    OnConnected(this);
                }
                catch (Exception ex)
                {
                    HTTPManager.Logger.Exception("SignalR Connection", "OnOpened", ex);
                }
            }

            // Deliver messages that we received before the /start request returned.
            // This must be after the OnStarted call, to let the clients to subrscribe to these events.
            if (BufferedMessages != null)
            {
                for (int i = 0; i < BufferedMessages.Count; ++i)
                    (this as IConnection).OnMessage(BufferedMessages[i]);

                BufferedMessages.Clear();
                BufferedMessages = null;
            }
        }

        /// <summary>
        /// Called when the transport sucessfully reconnected to the server.
        /// </summary>
        void IConnection.TransportReconnected()
        {
            if (this.State != ConnectionStates.Reconnecting)
                return;

            HTTPManager.Logger.Information("SignalR Connection", "Transport Reconnected");

            InitOnStart();

            if (OnReconnected != null)
            {
                try
                {
                    OnReconnected(this);
                }
                catch (Exception ex)
                {
                    HTTPManager.Logger.Exception("SignalR Connection", "OnReconnected", ex);
                }
            }
        }

        /// <summary>
        /// Called from the transport implementation when the Abort request finishes successfully.
        /// </summary>
        void IConnection.TransportAborted()
        {
            Close();
        }

        /// <summary>
        /// Called when an error occures. If the connection is in the Connected state, it will start the reconnect process, otherwise it will close the connection.
        /// </summary>
        void IConnection.Error(string reason)
        {
            // Not interested about errors we received after we already closed
            if (this.State == ConnectionStates.Closed)
                return;

            // If we are just quitting, don't try to reconnect.
            if (HTTPManager.IsQuitting)
            {
                Close();
                return;
            }

            HTTPManager.Logger.Error("SignalR Connection", reason);

            ReconnectStarted = false;

            if (OnError != null)
                OnError(this, reason);

            if (this.State == ConnectionStates.Connected || this.State == ConnectionStates.Reconnecting)
            {
                this.ReconnectDelayStartedAt = DateTime.UtcNow;
                if (this.State != ConnectionStates.Reconnecting)
                    this.ReconnectStartedAt = DateTime.UtcNow;

                //Reconnect();
            }
            else
            {
                // Fall back if possible
                if (this.State != ConnectionStates.Connecting || !TryFallbackTransport())
                    Close();
            }
        }

        /// <summary>
        /// Creates an Uri instance for the given request type.
        /// </summary>
        Uri IConnection.BuildUri(RequestTypes type)
        {
            return (this as IConnection).BuildUri(type, null);
        }

        /// <summary>
        /// Creates an Uri instance from the given parameters.
        /// </summary>
        Uri IConnection.BuildUri(RequestTypes type, TransportBase transport)
        {
            // make sure that the queryBuilder is reseted
            queryBuilder.Length = 0;

            UriBuilder uriBuilder = new UriBuilder(Uri);

            if (!uriBuilder.Path.EndsWith("/"))
                uriBuilder.Path += "/";

            long newValue, originalValue;
            do
            {
                originalValue = this.RequestCounter;
                newValue = originalValue % long.MaxValue;
            } while (System.Threading.Interlocked.CompareExchange(ref this.RequestCounter, newValue, originalValue) != originalValue);

            switch (type)
            {
                case RequestTypes.Negotiate:
                    uriBuilder.Path += "negotiate";
                    goto default;

                case RequestTypes.Connect:
#if !BESTHTTP_DISABLE_WEBSOCKET
                    if (transport != null && transport.Type == TransportTypes.WebSocket)
                        uriBuilder.Scheme = HTTPProtocolFactory.IsSecureProtocol(Uri) ? "wss" : "ws";
#endif

                    uriBuilder.Path += "connect";
                    goto default;

                case RequestTypes.Start:
                    uriBuilder.Path += "start";
                    goto default;

                case RequestTypes.Poll:
                    uriBuilder.Path += "poll";

                    if (this.LastReceivedMessage != null)
                    {
                        queryBuilder.Append("messageId=");
                        queryBuilder.Append(this.LastReceivedMessage.MessageId);
                    }

                    if (!string.IsNullOrEmpty(GroupsToken))
                    {
                        if (queryBuilder.Length > 0)
                            queryBuilder.Append("&");

                        queryBuilder.Append("groupsToken=");
                        queryBuilder.Append(GroupsToken);
                    }

                    goto default;

                case RequestTypes.Send:
                    uriBuilder.Path += "send";
                    goto default;

                case RequestTypes.Reconnect:
#if !BESTHTTP_DISABLE_WEBSOCKET
                    if (transport != null && transport.Type == TransportTypes.WebSocket)
                        uriBuilder.Scheme = HTTPProtocolFactory.IsSecureProtocol(Uri) ? "wss" : "ws";
#endif

                    uriBuilder.Path += "reconnect";

                    if (this.LastReceivedMessage != null)
                    {
                        queryBuilder.Append("messageId=");
                        queryBuilder.Append(this.LastReceivedMessage.MessageId);
                    }

                    if (!string.IsNullOrEmpty(GroupsToken))
                    {
                        if (queryBuilder.Length > 0)
                            queryBuilder.Append("&");

                        queryBuilder.Append("groupsToken=");
                        queryBuilder.Append(GroupsToken);
                    }

                    goto default;

                case RequestTypes.Abort:
                    uriBuilder.Path += "abort";
                    goto default;

                case RequestTypes.Ping:
                    uriBuilder.Path += "ping";

                    queryBuilder.Append("&tid=");
                    queryBuilder.Append(System.Threading.Interlocked.Increment(ref this.RequestCounter).ToString());

                    queryBuilder.Append("&_=");
                    queryBuilder.Append(Timestamp.ToString());

                    break;

                default:
                    if (queryBuilder.Length > 0)
                        queryBuilder.Append("&");

                    queryBuilder.Append("tid=");
                    queryBuilder.Append(System.Threading.Interlocked.Increment(ref this.RequestCounter).ToString());

                    queryBuilder.Append("&_=");
                    queryBuilder.Append(Timestamp.ToString());

                    if (transport != null)
                    {
                        queryBuilder.Append("&transport=");
                        queryBuilder.Append(transport.Name);
                    }

                    queryBuilder.Append("&clientProtocol=");
                    queryBuilder.Append(ClientProtocols[(byte)Protocol]);

                    if (NegotiationResult != null && !string.IsNullOrEmpty(this.NegotiationResult.ConnectionToken))
                    {
                        queryBuilder.Append("&connectionToken=");
                        queryBuilder.Append(this.NegotiationResult.ConnectionToken);
                    }

                    if (this.Hubs != null && this.Hubs.Length > 0)
                    {
                        queryBuilder.Append("&connectionData=");
                        queryBuilder.Append(this.ConnectionData);
                    }

                    break;
            }

            // Query params are added to all uri
            if (this.AdditionalQueryParams != null && this.AdditionalQueryParams.Count > 0)
                queryBuilder.Append(this.QueryParams);

            uriBuilder.Query = queryBuilder.ToString();

            // reset the string builder
            queryBuilder.Length = 0;

            return uriBuilder.Uri;
        }

        /// <summary>
        /// It's called on every request before sending it out to the server.
        /// </summary>
        HTTPRequest IConnection.PrepareRequest(HTTPRequest req, RequestTypes type)
        {
            if (req != null && AuthenticationProvider != null)
                AuthenticationProvider.PrepareRequest(req, type);

            if (RequestPreparator != null)
                RequestPreparator(this, req, type);

            return req;
        }

        /// <summary>
        /// Will parse a "{ 'Response': 'xyz' }" object and returns with 'xyz'. If it fails to parse, or getting the 'Response' key, it will call the Error function.
        /// </summary>
        string IConnection.ParseResponse(string responseStr)
        {
            Dictionary<string, object> dic = JSON.Json.Decode(responseStr) as Dictionary<string, object>;

            if (dic == null)
            {
                (this as IConnection).Error("Failed to parse Start response: " + responseStr);
                return string.Empty;
            }

            object value;
            if (!dic.TryGetValue("Response", out value) || value == null)
            {
                (this as IConnection).Error("No 'Response' key found in response: " + responseStr);
                return string.Empty;
            }

            return value.ToString();
        }

        #endregion

        #region IHeartbeat Implementation

        /// <summary>
        /// IHeartbeat implementation to manage timeouts.
        /// </summary>
        void IHeartbeat.OnHeartbeatUpdate(TimeSpan dif)
        {
            switch(this.State)
            {
                case ConnectionStates.Connected:
                    if (Transport.SupportsKeepAlive && NegotiationResult.KeepAliveTimeout != null && DateTime.UtcNow - LastMessageReceivedAt >= NegotiationResult.KeepAliveTimeout)
                        Reconnect();

                    if (PingRequest == null && DateTime.UtcNow - LastPingSentAt >= PingInterval)
                        Ping();

                    break;

                case ConnectionStates.Reconnecting:
                    if ( DateTime.UtcNow - ReconnectStartedAt >= NegotiationResult.DisconnectTimeout)
                    {
                        HTTPManager.Logger.Warning("SignalR Connection", "OnHeartbeatUpdate - Failed to reconnect in the given time!");

                        Close();
                    }
                    else if (DateTime.UtcNow - ReconnectDelayStartedAt >= ReconnectDelay)
                    {
                        if (HTTPManager.Logger.Level <= Logger.Loglevels.Warning)
                          HTTPManager.Logger.Warning("SignalR Connection", this.ReconnectStarted.ToString() + " " + this.ReconnectStartedAt.ToString() + " " + NegotiationResult.DisconnectTimeout.ToString());
                        Reconnect();
                    }
                    break;

                default:

                    if (TransportConnectionStartedAt != null && DateTime.UtcNow - TransportConnectionStartedAt >= NegotiationResult.TransportConnectTimeout)
                    {
                        HTTPManager.Logger.Warning("SignalR Connection", "OnHeartbeatUpdate - Transport failed to connect in the given time!");

                        // Using the Error function here instead of Close() will enable us to try to do a transport fallback.
                        (this as IConnection).Error("Transport failed to connect in the given time!");
                    }

                    break;
            }
        }

        #endregion

        #region Private Helper Functions

        /// <summary>
        /// Init function to set the connected states and set up other variables.
        /// </summary>
        private void InitOnStart()
        {
            this.State = ConnectionStates.Connected;

            //ReconnectStartedAt = null;
            ReconnectStarted = false;
            TransportConnectionStartedAt = null;

            LastPingSentAt = DateTime.UtcNow;
            LastMessageReceivedAt = DateTime.UtcNow;

            HTTPManager.Heartbeats.Subscribe(this);
        }

        /// <summary>
        /// Find and return with a Hub that has the message id.
        /// </summary>
        private Hub FindHub(UInt64 msgId)
        {
            if (Hubs != null)
                for (int i = 0; i < Hubs.Length; ++i)
                    if ((Hubs[i] as IHub).HasSentMessageId(msgId))
                        return Hubs[i];
            return null;
        }

        /// <summary>
        /// Try to fall back to next transport. If no more transport to try, it will return false.
        /// </summary>
        private bool TryFallbackTransport()
        {
            if (this.State == ConnectionStates.Connecting)
            {
                if (BufferedMessages != null)
                    BufferedMessages.Clear();

                // stop the current transport
                Transport.Stop();
                Transport = null;

                switch(NextProtocolToTry)
                {
#if !BESTHTTP_DISABLE_WEBSOCKET
                    case SupportedProtocols.WebSocket:
                        Transport = new WebSocketTransport(this);
                        break;
#endif

#if !BESTHTTP_DISABLE_SERVERSENT_EVENTS
                    case SupportedProtocols.ServerSentEvents:
                        Transport = new ServerSentEventsTransport(this);
                        NextProtocolToTry = SupportedProtocols.HTTP;
                        break;
#endif

                    case SupportedProtocols.HTTP:
                        Transport = new PollingTransport(this);
                        NextProtocolToTry = SupportedProtocols.Unknown;
                        break;

                    case SupportedProtocols.Unknown:
                        return false;
                }

                TransportConnectionStartedAt = DateTime.UtcNow;

                Transport.Connect();

                if (PingRequest != null)
                    PingRequest.Abort();

                return true;
            }

            return false;
        }

        /// <summary>
        /// This event will be called when the AdditonalQueryPrams dictionary changed. We have to reset the cached values.
        /// </summary>
        private void AdditionalQueryParams_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            BuiltQueryParams = null;
        }

        #endregion

        #region Ping Implementation

        /// <summary>
        /// Sends a Ping request to the SignalR server.
        /// </summary>
        private void Ping()
        {
            HTTPManager.Logger.Information("SignalR Connection", "Sending Ping request.");

            PingRequest = new HTTPRequest((this as IConnection).BuildUri(RequestTypes.Ping), OnPingRequestFinished);
            PingRequest.ConnectTimeout = PingInterval;

            (this as IConnection).PrepareRequest(PingRequest, RequestTypes.Ping);

            PingRequest.Send();

            LastPingSentAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Called when the Ping request finished.
        /// </summary>
        void OnPingRequestFinished(HTTPRequest req, HTTPResponse resp)
        {
            PingRequest = null;

            string reason = string.Empty;

            switch (req.State)
            {
                // The request finished without any problem.
                case HTTPRequestStates.Finished:
                    if (resp.IsSuccess)
                    {
                        // Parse the response, and do nothing when we receive the "pong" response
                        string response = (this as IConnection).ParseResponse(resp.DataAsText);

                        if (response != "pong")
                            reason = "Wrong answer for ping request: " + response;
                        else
                            HTTPManager.Logger.Information("SignalR Connection", "Pong received.");
                    }
                    else
                        reason = string.Format("Ping - Request Finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}",
                                                                                                    resp.StatusCode,
                                                                                                    resp.Message,
                                                                                                    resp.DataAsText);
                    break;

                // The request finished with an unexpected error. The request's Exception property may contain more info about the error.
                case HTTPRequestStates.Error:
                    reason = "Ping - Request Finished with Error! " + (req.Exception != null ? (req.Exception.Message + "\n" + req.Exception.StackTrace) : "No Exception");
                    break;

                // Connecting to the server is timed out.
                case HTTPRequestStates.ConnectionTimedOut:
                    reason = "Ping - Connection Timed Out!";
                    break;

                // The request didn't finished in the given time.
                case HTTPRequestStates.TimedOut:
                    reason = "Ping - Processing the request Timed Out!";
                    break;
            }

            if (!string.IsNullOrEmpty(reason))
                (this as IConnection).Error(reason);
        }

        #endregion
    }
}

#endif