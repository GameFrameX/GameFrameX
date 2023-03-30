#if !BESTHTTP_DISABLE_SIGNALR_CORE

using System.Threading;
#if CSHARP_7_OR_LATER
using System.Threading.Tasks;
#endif

using BestHTTP.Futures;
using BestHTTP.SignalRCore.Authentication;
using BestHTTP.SignalRCore.Messages;
using System;
using System.Collections.Generic;
using BestHTTP.Logger;
using System.Collections.Concurrent;
using BestHTTP.PlatformSupport.Threading;

namespace BestHTTP.SignalRCore
{
    public sealed class HubConnection : BestHTTP.Extensions.IHeartbeat
    {
        public static readonly object[] EmptyArgs = new object[0];

        /// <summary>
        /// Uri of the Hub endpoint
        /// </summary>
        public Uri Uri { get; private set; }

        /// <summary>
        /// Current state of this connection.
        /// </summary>
        public ConnectionStates State {
            get { return (ConnectionStates)this._state; }
            private set {
                Interlocked.Exchange(ref this._state, (int)value);
            }
        }
        private volatile int _state;

        /// <summary>
        /// Current, active ITransport instance.
        /// </summary>
        public ITransport Transport { get; private set; }

        /// <summary>
        /// The IProtocol implementation that will parse, encode and decode messages.
        /// </summary>
        public IProtocol Protocol { get; private set; }

        /// <summary>
        /// This event is called when the connection is redirected to a new uri.
        /// </summary>
        public event Action<HubConnection, Uri, Uri> OnRedirected;

        /// <summary>
        /// This event is called when successfully connected to the hub.
        /// </summary>
        public event Action<HubConnection> OnConnected;

        /// <summary>
        /// This event is called when an unexpected error happen and the connection is closed.
        /// </summary>
        public event Action<HubConnection, string> OnError;

        /// <summary>
        /// This event is called when the connection is gracefully terminated.
        /// </summary>
        public event Action<HubConnection> OnClosed;

        /// <summary>
        /// This event is called for every server-sent message. When returns false, no further processing of the message is done by the plugin.
        /// </summary>
        public event Func<HubConnection, Message, bool> OnMessage;

        /// <summary>
        /// Called when the HubConnection start its reconnection process after loosing its underlying connection.
        /// </summary>
        public event Action<HubConnection, string> OnReconnecting;

        /// <summary>
        /// Called after a successful reconnection.
        /// </summary>
        public event Action<HubConnection> OnReconnected;

        /// <summary>
        /// Called for transport related events.
        /// </summary>
        public event Action<HubConnection, ITransport, TransportEvents> OnTransportEvent;

        /// <summary>
        /// An IAuthenticationProvider implementation that will be used to authenticate the connection.
        /// </summary>
        public IAuthenticationProvider AuthenticationProvider { get; set; }

        /// <summary>
        /// Negotiation response sent by the server.
        /// </summary>
        public NegotiationResult NegotiationResult { get; private set; }

        /// <summary>
        /// Options that has been used to create the HubConnection.
        /// </summary>
        public HubOptions Options { get; private set; }

        /// <summary>
        /// How many times this connection is redirected.
        /// </summary>
        public int RedirectCount { get; private set; }

        /// <summary>
        /// The reconnect policy that will be used when the underlying connection is lost. Its default value is null.
        /// </summary>
        public IRetryPolicy ReconnectPolicy { get; set; }

        /// <summary>
        /// Logging context of this HubConnection instance.
        /// </summary>
        public LoggingContext Context { get; private set; }

        /// <summary>
        /// This will be increment to add a unique id to every message the plugin will send.
        /// </summary>
        private long lastInvocationId = 1;

        /// <summary>
        /// Id of the last streaming parameter.
        /// </summary>
        private int lastStreamId = 1;

        /// <summary>
        ///  Store the callback for all sent message that expect a return value from the server. All sent message has
        ///  a unique invocationId that will be sent back from the server.
        /// </summary>
        private ConcurrentDictionary<long, InvocationDefinition> invocations = new ConcurrentDictionary<long, InvocationDefinition>();

        /// <summary>
        /// This is where we store the methodname => callback mapping.
        /// </summary>
        private ConcurrentDictionary<string, Subscription> subscriptions = new ConcurrentDictionary<string, Subscription>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// When we sent out the last message to the server.
        /// </summary>
        private DateTime lastMessageSentAt;
        private DateTime lastMessageReceivedAt;

        private DateTime connectionStartedAt;

        private RetryContext currentContext;
        private DateTime reconnectStartTime = DateTime.MinValue;
        private DateTime reconnectAt;

        private List<TransportTypes> triedoutTransports = new List<TransportTypes>();

        private ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        public HubConnection(Uri hubUri, IProtocol protocol)
            : this(hubUri, protocol, new HubOptions())
        {
        }

        public HubConnection(Uri hubUri, IProtocol protocol, HubOptions options)
        {
            this.Context = new LoggingContext(this);

            this.Uri = hubUri;
            this.State = ConnectionStates.Initial;
            this.Options = options;
            this.Protocol = protocol;
            this.Protocol.Connection = this;
            this.AuthenticationProvider = new DefaultAccessTokenAuthenticator(this);
        }

        public void StartConnect()
        {
            if (this.State != ConnectionStates.Initial &&
                this.State != ConnectionStates.Redirected &&
                this.State != ConnectionStates.Reconnecting)
            {
                HTTPManager.Logger.Warning("HubConnection", "StartConnect - Expected Initial or Redirected state, got " + this.State.ToString(), this.Context);
                return;
            }

            if (this.State == ConnectionStates.Initial)
            {
                this.connectionStartedAt = DateTime.Now;
                HTTPManager.Heartbeats.Subscribe(this);
            }

            HTTPManager.Logger.Verbose("HubConnection", $"StartConnect State: {this.State}, connectionStartedAt: {this.connectionStartedAt.ToString(System.Globalization.CultureInfo.InvariantCulture)}", this.Context);

            if (this.AuthenticationProvider != null && this.AuthenticationProvider.IsPreAuthRequired)
            {
                HTTPManager.Logger.Information("HubConnection", "StartConnect - Authenticating", this.Context);

                SetState(ConnectionStates.Authenticating);

                this.AuthenticationProvider.OnAuthenticationSucceded += OnAuthenticationSucceded;
                this.AuthenticationProvider.OnAuthenticationFailed += OnAuthenticationFailed;

                // Start the authentication process
                this.AuthenticationProvider.StartAuthentication();
            }
            else
                StartNegotiation();
        }

#if CSHARP_7_OR_LATER

        TaskCompletionSource<HubConnection> connectAsyncTaskCompletionSource;

        public Task<HubConnection> ConnectAsync()
        {
            if (this.State != ConnectionStates.Initial && this.State != ConnectionStates.Redirected && this.State != ConnectionStates.Reconnecting)
                throw new Exception("HubConnection - ConnectAsync - Expected Initial or Redirected state, got " + this.State.ToString());

            if (this.connectAsyncTaskCompletionSource != null)
                throw new Exception("Connect process already started!");

            this.connectAsyncTaskCompletionSource = new TaskCompletionSource<HubConnection>();

            this.OnConnected += OnAsyncConnectedCallback;
            this.OnError += OnAsyncConnectFailedCallback;

            this.StartConnect();

            return connectAsyncTaskCompletionSource.Task;
        }

        private void OnAsyncConnectedCallback(HubConnection hub)
        {
            this.OnConnected -= OnAsyncConnectedCallback;
            this.OnError -= OnAsyncConnectFailedCallback;

            this.connectAsyncTaskCompletionSource.TrySetResult(this);
            this.connectAsyncTaskCompletionSource = null;
        }

        private void OnAsyncConnectFailedCallback(HubConnection hub, string error)
        {
            this.OnConnected -= OnAsyncConnectedCallback;
            this.OnError -= OnAsyncConnectFailedCallback;

            this.connectAsyncTaskCompletionSource.TrySetException(new Exception(error));
            this.connectAsyncTaskCompletionSource = null;
        }

#endif

        private void OnAuthenticationSucceded(IAuthenticationProvider provider)
        {
            HTTPManager.Logger.Verbose("HubConnection", "OnAuthenticationSucceded", this.Context);

            this.AuthenticationProvider.OnAuthenticationSucceded -= OnAuthenticationSucceded;
            this.AuthenticationProvider.OnAuthenticationFailed -= OnAuthenticationFailed;

            StartNegotiation();
        }

        private void OnAuthenticationFailed(IAuthenticationProvider provider, string reason)
        {
            HTTPManager.Logger.Error("HubConnection", "OnAuthenticationFailed: " + reason, this.Context);

            this.AuthenticationProvider.OnAuthenticationSucceded -= OnAuthenticationSucceded;
            this.AuthenticationProvider.OnAuthenticationFailed -= OnAuthenticationFailed;

            SetState(ConnectionStates.Closed, reason);
        }

        private void StartNegotiation()
        {
            HTTPManager.Logger.Verbose("HubConnection", "StartNegotiation", this.Context);

            if (this.State == ConnectionStates.CloseInitiated)
            {
                SetState(ConnectionStates.Closed);
                return;
            }

#if !BESTHTTP_DISABLE_WEBSOCKET
            if (this.Options.SkipNegotiation && this.Options.PreferedTransport == TransportTypes.WebSocket)
            {
                HTTPManager.Logger.Verbose("HubConnection", "Skipping negotiation", this.Context);
                ConnectImpl(this.Options.PreferedTransport);

                return;
            }
#endif

            SetState(ConnectionStates.Negotiating);

            // https://github.com/dotnet/aspnetcore/blob/master/src/SignalR/docs/specs/TransportProtocols.md#post-endpoint-basenegotiate-request
            // Send out a negotiation request. While we could skip it and connect right with the websocket transport
            //  it might return with additional information that could be useful.

            UriBuilder builder = new UriBuilder(this.Uri);
            if (builder.Path.EndsWith("/"))
                builder.Path += "negotiate";
            else
                builder.Path += "/negotiate";

            string query = builder.Query;
            if (string.IsNullOrEmpty(query))
                query = "negotiateVersion=1";
            else
                query = query.Remove(0, 1) + "&negotiateVersion=1";

            builder.Query = query;

            var request = new HTTPRequest(builder.Uri, HTTPMethods.Post, OnNegotiationRequestFinished);
            request.Context.Add("Hub", this.Context);

            if (this.AuthenticationProvider != null)
                this.AuthenticationProvider.PrepareRequest(request);

            request.Send();
        }
        
        private void ConnectImpl(TransportTypes transport)
        {
            HTTPManager.Logger.Verbose("HubConnection", "ConnectImpl - " + transport, this.Context);

            switch (transport)
            {
#if !BESTHTTP_DISABLE_WEBSOCKET
                case TransportTypes.WebSocket:
                    if (this.NegotiationResult != null && !IsTransportSupported("WebSockets"))
                    {
                        SetState(ConnectionStates.Closed, "Couldn't use preferred transport, as the 'WebSockets' transport isn't supported by the server!");
                        return;
                    }

                    this.Transport = new Transports.WebSocketTransport(this);
                    this.Transport.OnStateChanged += Transport_OnStateChanged;
                    break;
#endif

                case TransportTypes.LongPolling:
                    if (this.NegotiationResult != null && !IsTransportSupported("LongPolling"))
                    {
                        SetState(ConnectionStates.Closed, "Couldn't use preferred transport, as the 'LongPolling' transport isn't supported by the server!");
                        return;
                    }

                    this.Transport = new Transports.LongPollingTransport(this);
                    this.Transport.OnStateChanged += Transport_OnStateChanged;
                    break;

                default:
                    SetState(ConnectionStates.Closed, "Unsupported transport: " + transport);
                    break;
            }

            try
            {
                if (this.OnTransportEvent != null)
                    this.OnTransportEvent(this, this.Transport, TransportEvents.SelectedToConnect);
            }
            catch(Exception ex)
            {
                HTTPManager.Logger.Exception("HubConnection", "ConnectImpl - OnTransportEvent exception in user code!", ex, this.Context);
            }

            this.Transport.StartConnect();
        }

        private bool IsTransportSupported(string transportName)
        {
            // https://github.com/dotnet/aspnetcore/blob/master/src/SignalR/docs/specs/TransportProtocols.md#post-endpoint-basenegotiate-request
            // If the negotiation response contains only the url and accessToken, no 'availableTransports' list is sent
            if (this.NegotiationResult.SupportedTransports == null)
                return true;

            for (int i = 0; i < this.NegotiationResult.SupportedTransports.Count; ++i)
                if (this.NegotiationResult.SupportedTransports[i].Name.Equals(transportName, StringComparison.OrdinalIgnoreCase))
                    return true;

            return false;
        }

        private void OnNegotiationRequestFinished(HTTPRequest req, HTTPResponse resp)
        {
            if (this.State == ConnectionStates.Closed)
                return;

            if (this.State == ConnectionStates.CloseInitiated)
            {
                SetState(ConnectionStates.Closed);
                return;
            }

            string errorReason = null;

            switch (req.State)
            {
                // The request finished without any problem.
                case HTTPRequestStates.Finished:
                    if (resp.IsSuccess)
                    {
                        HTTPManager.Logger.Information("HubConnection", "Negotiation Request Finished Successfully! Response: " + resp.DataAsText, this.Context);

                        // Parse negotiation
                        this.NegotiationResult = NegotiationResult.Parse(resp, out errorReason, this);

                        // Room for improvement: check validity of the negotiation result:
                        //  If url and accessToken is present, the other two must be null.
                        //  https://github.com/dotnet/aspnetcore/blob/master/src/SignalR/docs/specs/TransportProtocols.md#post-endpoint-basenegotiate-request

                        if (string.IsNullOrEmpty(errorReason))
                        {
                            if (this.NegotiationResult.Url != null)
                            {
                                this.SetState(ConnectionStates.Redirected);

                                if (++this.RedirectCount >= this.Options.MaxRedirects)
                                    errorReason = string.Format("MaxRedirects ({0:N0}) reached!", this.Options.MaxRedirects);
                                else
                                {
                                    var oldUri = this.Uri;
                                    this.Uri = this.NegotiationResult.Url;

                                    if (this.OnRedirected != null)
                                    {
                                        try
                                        {
                                            this.OnRedirected(this, oldUri, Uri);
                                        }
                                        catch (Exception ex)
                                        {
                                            HTTPManager.Logger.Exception("HubConnection", "OnNegotiationRequestFinished - OnRedirected", ex, this.Context);
                                        }
                                    }

                                    StartConnect();
                                }
                            }
                            else
                                ConnectImpl(this.Options.PreferedTransport);
                        }
                    }
                    else // Internal server error?
                        errorReason = string.Format("Negotiation Request Finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}",
                                                        resp.StatusCode,
                                                        resp.Message,
                                                        resp.DataAsText);
                    break;

                // The request finished with an unexpected error. The request's Exception property may contain more info about the error.
                case HTTPRequestStates.Error:
                    errorReason = "Negotiation Request Finished with Error! " + (req.Exception != null ? (req.Exception.Message + "\n" + req.Exception.StackTrace) : "No Exception");
                    break;

                // The request aborted, initiated by the user.
                case HTTPRequestStates.Aborted:
                    errorReason = "Negotiation Request Aborted!";
                    break;

                // Connecting to the server is timed out.
                case HTTPRequestStates.ConnectionTimedOut:
                    errorReason = "Negotiation Request - Connection Timed Out!";
                    break;

                // The request didn't finished in the given time.
                case HTTPRequestStates.TimedOut:
                    errorReason = "Negotiation Request - Processing the request Timed Out!";
                    break;
            }

            if (errorReason != null)
            {
                this.NegotiationResult = new NegotiationResult();
                this.NegotiationResult.NegotiationResponse = resp;

                SetState(ConnectionStates.Closed, errorReason);
            }
        }

        public void StartClose()
        {
            HTTPManager.Logger.Verbose("HubConnection", "StartClose", this.Context);

            switch(this.State)
            {
                case ConnectionStates.Initial:
                    SetState(ConnectionStates.Closed);
                    break;

                case ConnectionStates.Authenticating:
                    this.AuthenticationProvider.OnAuthenticationSucceded -= OnAuthenticationSucceded;
                    this.AuthenticationProvider.OnAuthenticationFailed -= OnAuthenticationFailed;
                    this.AuthenticationProvider.Cancel();
                    SetState(ConnectionStates.Closed);
                    break;

                case ConnectionStates.Reconnecting:
                    SetState(ConnectionStates.Closed);
                    break;

                case ConnectionStates.CloseInitiated:
                case ConnectionStates.Closed:
                    // Already initiated/closed
                    break;

                default:
                    SetState(ConnectionStates.CloseInitiated);

                    if (this.Transport != null)
                        this.Transport.StartClose();
                    break;
            }
        }

#if CSHARP_7_OR_LATER

        TaskCompletionSource<HubConnection> closeAsyncTaskCompletionSource;

        public Task<HubConnection> CloseAsync()
        {
            if (this.closeAsyncTaskCompletionSource != null)
                throw new Exception("CloseAsync already called!");

            this.closeAsyncTaskCompletionSource = new TaskCompletionSource<HubConnection>();

            this.OnClosed += OnClosedAsyncCallback;
            this.OnError += OnClosedAsyncErrorCallback;

            // Avoid race condition by caching task prior to StartClose,
            // which asynchronously calls OnClosedAsyncCallback, which nulls
            // this.closeAsyncTaskCompletionSource immediately before we have
            // a chance to read from it.
            var task = this.closeAsyncTaskCompletionSource.Task;

            this.StartClose();

            return task;
        }

        void OnClosedAsyncCallback(HubConnection hub)
        {
            this.OnClosed -= OnClosedAsyncCallback;
            this.OnError -= OnClosedAsyncErrorCallback;

            this.closeAsyncTaskCompletionSource.TrySetResult(this);
            this.closeAsyncTaskCompletionSource = null;
        }

        void OnClosedAsyncErrorCallback(HubConnection hub, string error)
        {
            this.OnClosed -= OnClosedAsyncCallback;
            this.OnError -= OnClosedAsyncErrorCallback;

            this.closeAsyncTaskCompletionSource.TrySetException(new Exception(error));
            this.closeAsyncTaskCompletionSource = null;
        }

#endif

        public IFuture<TResult> Invoke<TResult>(string target, params object[] args)
        {
            Future<TResult> future = new Future<TResult>();

            long id = InvokeImp(target,
                args,
                (message) =>
                    {
                        bool isSuccess = string.IsNullOrEmpty(message.error);
                        if (isSuccess)
                            future.Assign((TResult)this.Protocol.ConvertTo(typeof(TResult), message.result));
                        else
                            future.Fail(new Exception(message.error));
                    },
                typeof(TResult));

            if (id < 0)
                future.Fail(new Exception("Not in Connected state! Current state: " + this.State));

            return future;
        }

#if CSHARP_7_OR_LATER

        public Task<TResult> InvokeAsync<TResult>(string target, params object[] args)
        {
            return InvokeAsync<TResult>(target, default(CancellationToken), args);
        }

        public Task<TResult> InvokeAsync<TResult>(string target, CancellationToken cancellationToken = default, params object[] args)
        {
            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
            long id = InvokeImp(target,
                args,
                (message) =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        tcs.TrySetCanceled(cancellationToken);
                        return;
                    }

                    bool isSuccess = string.IsNullOrEmpty(message.error);
                    if (isSuccess)
                        tcs.TrySetResult((TResult)this.Protocol.ConvertTo(typeof(TResult), message.result));
                    else
                        tcs.TrySetException(new Exception(message.error));
                },
                typeof(TResult));

            if (id < 0)
                tcs.TrySetException(new Exception("Not in Connected state! Current state: " + this.State));
            else
                cancellationToken.Register(() => tcs.TrySetCanceled());

            return tcs.Task;
        }

#endif

        public IFuture<object> Send(string target, params object[] args)
        {
            Future<object> future = new Future<object>();

            long id = InvokeImp(target,
                args,
                (message) =>
                    {
                        bool isSuccess = string.IsNullOrEmpty(message.error);
                        if (isSuccess)
                            future.Assign(message.item);
                        else
                            future.Fail(new Exception(message.error));
                    },
                typeof(object));

            if (id < 0)
                future.Fail(new Exception("Not in Connected state! Current state: " + this.State));

            return future;
        }

#if CSHARP_7_OR_LATER

        public Task<object> SendAsync(string target, params object[] args)
        {
            return SendAsync(target, default(CancellationToken), args);
        }

        public Task<object> SendAsync(string target, CancellationToken cancellationToken = default, params object[] args)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            long id = InvokeImp(target,
                args,
                (message) =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        tcs.TrySetCanceled(cancellationToken);
                        return;
                    }

                    bool isSuccess = string.IsNullOrEmpty(message.error);
                    if (isSuccess)
                        tcs.TrySetResult(message.item);
                    else
                        tcs.TrySetException(new Exception(message.error));
                },
                typeof(object));

            if (id < 0)
                tcs.TrySetException(new Exception("Not in Connected state! Current state: " + this.State));
            else
                cancellationToken.Register(() => tcs.TrySetCanceled());

            return tcs.Task;
        }

#endif

        private long InvokeImp(string target, object[] args, Action<Message> callback, Type itemType, bool isStreamingInvocation = false)
        {
            if (this.State != ConnectionStates.Connected)
                return -1;

            bool blockingInvocation = callback == null;

            long invocationId = blockingInvocation ? 0 : System.Threading.Interlocked.Increment(ref this.lastInvocationId);
            var message = new Message
            {
                type = isStreamingInvocation ? MessageTypes.StreamInvocation : MessageTypes.Invocation,
                invocationId = blockingInvocation ? null : invocationId.ToString(),
                target = target,
                arguments = args,
                nonblocking = callback == null,
            };

            SendMessage(message);

            if (!blockingInvocation)
                if (!this.invocations.TryAdd(invocationId, new InvocationDefinition { callback = callback, returnType = itemType }))
                    HTTPManager.Logger.Warning("HubConnection", "InvokeImp - invocations already contains id: " + invocationId);

            return invocationId;
        }

        internal void SendMessage(Message message)
        {
            if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                HTTPManager.Logger.Verbose("HubConnection", "SendMessage: " + message.ToString(), this.Context);

            try
            {
                using (new WriteLock(this.rwLock))
                {
                    var encoded = this.Protocol.EncodeMessage(message);
                    if (encoded.Data != null)
                    {
                        this.lastMessageSentAt = DateTime.Now;
                        this.Transport.Send(encoded);
                    }
                }
            }
            catch (Exception ex)
            {
                HTTPManager.Logger.Exception("HubConnection", "SendMessage", ex, this.Context);
            }
        }

        public DownStreamItemController<TDown> GetDownStreamController<TDown>(string target, params object[] args)
        {
            long invocationId = System.Threading.Interlocked.Increment(ref this.lastInvocationId);

            var future = new Future<TDown>();
            future.BeginProcess();

            var controller = new DownStreamItemController<TDown>(this, invocationId, future);

            Action<Message> callback = (Message msg) =>
            {
                switch (msg.type)
                {
                    // StreamItem message contains only one item.
                    case MessageTypes.StreamItem:
                        {
                            if (controller.IsCanceled)
                                break;

                            TDown item = (TDown)this.Protocol.ConvertTo(typeof(TDown), msg.item);

                            future.AssignItem(item);
                            break;
                        }

                    case MessageTypes.Completion:
                        {
                            bool isSuccess = string.IsNullOrEmpty(msg.error);
                            if (isSuccess)
                            {
                                // While completion message must not contain any result, this should be future-proof
                                if (!controller.IsCanceled && msg.result != null)
                                {
                                    TDown result = (TDown)this.Protocol.ConvertTo(typeof(TDown), msg.result);

                                    future.AssignItem(result);
                                }

                                future.Finish();
                            }
                            else
                                future.Fail(new Exception(msg.error));
                            break;
                        }
                }
            };
            
            var message = new Message
            {
                type = MessageTypes.StreamInvocation,
                invocationId = invocationId.ToString(),
                target = target,
                arguments = args,
                nonblocking = false,
            };

            SendMessage(message);

            if (callback != null)
                if (!this.invocations.TryAdd(invocationId, new InvocationDefinition { callback = callback, returnType = typeof(TDown) }))
                    HTTPManager.Logger.Warning("HubConnection", "GetDownStreamController - invocations already contains id: " + invocationId);

            return controller;
        }

        public UpStreamItemController<TResult> GetUpStreamController<TResult>(string target, int paramCount, bool downStream, object[] args)
        {
            Future<TResult> future = new Future<TResult>();
            future.BeginProcess();

            long invocationId = System.Threading.Interlocked.Increment(ref this.lastInvocationId);

            string[] streamIds = new string[paramCount];
            for (int i = 0; i < paramCount; i++)
                streamIds[i] = System.Threading.Interlocked.Increment(ref this.lastStreamId).ToString();

            var controller = new UpStreamItemController<TResult>(this, invocationId, streamIds, future);

            Action<Message> callback = (Message msg) => {
                switch (msg.type)
                {
                    // StreamItem message contains only one item.
                    case MessageTypes.StreamItem:
                        {
                            if (controller.IsCanceled)
                                break;

                            TResult item = (TResult)this.Protocol.ConvertTo(typeof(TResult), msg.item);

                            future.AssignItem(item);
                            break;
                        }

                    case MessageTypes.Completion:
                        {
                            bool isSuccess = string.IsNullOrEmpty(msg.error);
                            if (isSuccess)
                            {
                                // While completion message must not contain any result, this should be future-proof
                                if (!controller.IsCanceled && msg.result != null)
                                {
                                    TResult result = (TResult)this.Protocol.ConvertTo(typeof(TResult), msg.result);

                                    future.AssignItem(result);
                                }

                                future.Finish();
                            }
                            else
                            {
                                var ex = new Exception(msg.error);
                                future.Fail(ex);
                            }
                            break;
                        }
                }
            };

            var messageToSend = new Message
            {
                type = downStream ? MessageTypes.StreamInvocation : MessageTypes.Invocation,
                invocationId = invocationId.ToString(),
                target = target,
                arguments = args,
                streamIds = streamIds,
                nonblocking = false,
            };

            SendMessage(messageToSend);

            if (!this.invocations.TryAdd(invocationId, new InvocationDefinition { callback = callback, returnType = typeof(TResult) }))
                HTTPManager.Logger.Warning("HubConnection", "GetUpStreamController - invocations already contains id: " + invocationId);

            return controller;
        }

        public void On(string methodName, Action callback)
        {
            On(methodName, null, (args) => callback());
        }

        public void On<T1>(string methodName, Action<T1> callback)
        {
            On(methodName, new Type[] { typeof(T1) }, (args) => callback((T1)args[0]));
        }

        public void On<T1, T2>(string methodName, Action<T1, T2> callback)
        {
            On(methodName,
                new Type[] { typeof(T1), typeof(T2) },
                (args) => callback((T1)args[0], (T2)args[1]));
        }

        public void On<T1, T2, T3>(string methodName, Action<T1, T2, T3> callback)
        {
            On(methodName,
                new Type[] { typeof(T1), typeof(T2), typeof(T3) },
                (args) => callback((T1)args[0], (T2)args[1], (T3)args[2]));
        }

        public void On<T1, T2, T3, T4>(string methodName, Action<T1, T2, T3, T4> callback)
        {
            On(methodName,
                new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) },
                (args) => callback((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3]));
        }

        private void On(string methodName, Type[] paramTypes, Action<object[]> callback)
        {
            if (this.State >= ConnectionStates.CloseInitiated)
                throw new Exception("Hub connection already closing or closed!");

            this.subscriptions.GetOrAdd(methodName, _ => new Subscription())
                .Add(paramTypes, callback);
        }

        /// <summary>
        /// Remove all event handlers for <paramref name="methodName"/> that subscribed with an On call.
        /// </summary>
        public void Remove(string methodName)
        {
            if (this.State >= ConnectionStates.CloseInitiated)
                throw new Exception("Hub connection already closing or closed!");

            Subscription _;
            this.subscriptions.TryRemove(methodName, out _);
        }

        internal Subscription GetSubscription(string methodName)
        {
            Subscription subscribtion = null;
            this.subscriptions.TryGetValue(methodName, out subscribtion);
            return subscribtion;
        }

        internal Type GetItemType(long invocationId)
        {
            InvocationDefinition def;
            this.invocations.TryGetValue(invocationId, out def);
            return def.returnType;
        }

        internal void OnMessages(List<Message> messages)
        {
            this.lastMessageReceivedAt = DateTime.Now;
            for (int messageIdx = 0; messageIdx < messages.Count; ++messageIdx)
            {
                var message = messages[messageIdx];

                if (this.OnMessage != null)
                {
                    try
                    {
                        if (!this.OnMessage(this, message))
                            continue;
                    }
                    catch (Exception ex)
                    {
                        HTTPManager.Logger.Exception("HubConnection", "Exception in OnMessage user code!", ex, this.Context);
                    }
                }

                switch (message.type)
                {
                    case MessageTypes.Invocation:
                        {
                            Subscription subscribtion = null;
                            if (this.subscriptions.TryGetValue(message.target, out subscribtion))
                            {
                                for (int i = 0; i < subscribtion.callbacks.Count; ++i)
                                {
                                    var callbackDesc = subscribtion.callbacks[i];

                                    object[] realArgs = null;
                                    try
                                    {
                                        realArgs = this.Protocol.GetRealArguments(callbackDesc.ParamTypes, message.arguments);
                                    }
                                    catch (Exception ex)
                                    {
                                        HTTPManager.Logger.Exception("HubConnection", "OnMessages - Invocation - GetRealArguments", ex, this.Context);
                                    }

                                    try
                                    {
                                        callbackDesc.Callback.Invoke(realArgs);
                                    }
                                    catch (Exception ex)
                                    {
                                        HTTPManager.Logger.Exception("HubConnection", "OnMessages - Invocation - Invoke", ex, this.Context);
                                    }
                                }
                            }

                            break;
                        }

                    case MessageTypes.StreamItem:
                        {
                            long invocationId;
                            if (long.TryParse(message.invocationId, out invocationId))
                            {
                                InvocationDefinition def;
                                if (this.invocations.TryGetValue(invocationId, out def) && def.callback != null)
                                {
                                    try
                                    {
                                        def.callback(message);
                                    }
                                    catch (Exception ex)
                                    {
                                        HTTPManager.Logger.Exception("HubConnection", "OnMessages - StreamItem - callback", ex, this.Context);
                                    }
                                }
                            }
                            break;
                        }

                    case MessageTypes.Completion:
                        {
                            long invocationId;
                            if (long.TryParse(message.invocationId, out invocationId))
                            {
                                InvocationDefinition def;
                                if (this.invocations.TryRemove(invocationId, out def) && def.callback != null)
                                {
                                    try
                                    {
                                        def.callback(message);
                                    }
                                    catch (Exception ex)
                                    {
                                        HTTPManager.Logger.Exception("HubConnection", "OnMessages - Completion - callback", ex, this.Context);
                                    }
                                }
                            }
                            break;
                        }

                    case MessageTypes.Ping:
                        // Send back an answer
                        SendMessage(new Message() { type = MessageTypes.Ping });
                        break;

                    case MessageTypes.Close:
                        SetState(ConnectionStates.Closed, message.error, message.allowReconnect);
                        if (this.Transport != null)
                            this.Transport.StartClose();
                        return;
                }
            }
        }

        private void Transport_OnStateChanged(TransportStates oldState, TransportStates newState)
        {
            HTTPManager.Logger.Verbose("HubConnection", string.Format("Transport_OnStateChanged - oldState: {0} newState: {1}", oldState.ToString(), newState.ToString()), this.Context);

            if (this.State == ConnectionStates.Closed)
            {
                HTTPManager.Logger.Verbose("HubConnection", "Transport_OnStateChanged - already closed!", this.Context);
                return;
            }

            switch (newState)
            {
                case TransportStates.Connected:
                    try
                    {
                        if (this.OnTransportEvent != null)
                            this.OnTransportEvent(this, this.Transport, TransportEvents.Connected);
                    }
                    catch (Exception ex)
                    {
                        HTTPManager.Logger.Exception("HubConnection", "Exception in OnTransportEvent user code!", ex, this.Context);
                    }

                    SetState(ConnectionStates.Connected);
                    break;

                case TransportStates.Failed:
                    if (this.State == ConnectionStates.Negotiating && !HTTPManager.IsQuitting)
                    {
                        try
                        {
                            if (this.OnTransportEvent != null)
                                this.OnTransportEvent(this, this.Transport, TransportEvents.FailedToConnect);
                        }
                        catch (Exception ex)
                        {
                            HTTPManager.Logger.Exception("HubConnection", "Exception in OnTransportEvent user code!", ex, this.Context);
                        }

                        this.triedoutTransports.Add(this.Transport.TransportType);

                        var nextTransport = GetNextTransportToTry();
                        if (nextTransport == null)
                            SetState(ConnectionStates.Closed, this.Transport.ErrorReason);
                        else
                            ConnectImpl(nextTransport.Value);
                    }
                    else
                    {
                        try
                        {
                            if (this.OnTransportEvent != null)
                                this.OnTransportEvent(this, this.Transport, TransportEvents.ClosedWithError);
                        }
                        catch (Exception ex)
                        {
                            HTTPManager.Logger.Exception("HubConnection", "Exception in OnTransportEvent user code!", ex, this.Context);
                        }

                        SetState(ConnectionStates.Closed, HTTPManager.IsQuitting ? null : this.Transport.ErrorReason);
                    }
                    break;

                case TransportStates.Closed:
                    {
                        try
                        {
                            if (this.OnTransportEvent != null)
                                this.OnTransportEvent(this, this.Transport, TransportEvents.Closed);
                        }
                        catch (Exception ex)
                        {
                            HTTPManager.Logger.Exception("HubConnection", "Exception in OnTransportEvent user code!", ex, this.Context);
                        }

                        SetState(ConnectionStates.Closed);
                    }
                    break;
            }
        }

        private TransportTypes? GetNextTransportToTry()
        {
            foreach (TransportTypes val in Enum.GetValues(typeof(TransportTypes)))
                if (!this.triedoutTransports.Contains(val) && IsTransportSupported(val.ToString()))
                    return val;

            return null;
        }

        private void SetState(ConnectionStates state, string errorReason = null, bool allowReconnect = true)
        {
            if (string.IsNullOrEmpty(errorReason))
                HTTPManager.Logger.Information("HubConnection", string.Format("SetState - from State: '{0}' to State: '{1}', allowReconnect: {2}", this.State, state, allowReconnect), this.Context);
            else
                HTTPManager.Logger.Information("HubConnection", string.Format("SetState - from State: '{0}' to State: '{1}', errorReason: '{2}', allowReconnect: {3}", this.State, state, errorReason, allowReconnect), this.Context);

            if (this.State == state)
                return;

            var previousState = this.State;

            this.State = state;

            switch (state)
            {
                case ConnectionStates.Initial:
                case ConnectionStates.Authenticating:
                case ConnectionStates.Negotiating:
                case ConnectionStates.CloseInitiated:
                    break;

                case ConnectionStates.Reconnecting:
                    break;

                case ConnectionStates.Connected:
                    // If reconnectStartTime isn't its default value we reconnected
                    if (this.reconnectStartTime != DateTime.MinValue)
                    {
                        try
                        {
                            if (this.OnReconnected != null)
                                this.OnReconnected(this);
                        }
                        catch (Exception ex)
                        {
                            HTTPManager.Logger.Exception("HubConnection", "OnReconnected", ex, this.Context);
                        }
                    }
                    else
                    {
                        try
                        {
                            if (this.OnConnected != null)
                                this.OnConnected(this);
                        }
                        catch (Exception ex)
                        {
                            HTTPManager.Logger.Exception("HubConnection", "Exception in OnConnected user code!", ex, this.Context);
                        }
                    }

                    this.lastMessageSentAt = DateTime.Now;
                    this.lastMessageReceivedAt = DateTime.Now;

                    // Clean up reconnect related fields
                    this.currentContext = new RetryContext();
                    this.reconnectStartTime = DateTime.MinValue;
                    this.reconnectAt = DateTime.MinValue;

                    break;

                case ConnectionStates.Closed:
                    // Go through all invocations and cancel them.
                    var error = new Message();
                    error.type = MessageTypes.Close;
                    error.error = errorReason;

                    foreach (var kvp in this.invocations)
                    {
                        try
                        {
                            kvp.Value.callback(error);
                        }
                        catch
                        { }
                    }

                    this.invocations.Clear();

                    // No errorReason? It's an expected closure.
                    if (errorReason == null)
                    {
                        if (this.OnClosed != null)
                        {
                            try
                            {
                                this.OnClosed(this);
                            }
                            catch(Exception ex)
                            {
                                HTTPManager.Logger.Exception("HubConnection", "Exception in OnClosed user code!", ex, this.Context);
                            }
                        }
                    }
                    else
                    {
                        // If possible, try to reconnect
                        if (allowReconnect && this.ReconnectPolicy != null && (previousState == ConnectionStates.Connected || this.reconnectStartTime != DateTime.MinValue))
                        {
                            // It's the first attempt after a successful connection
                            if (this.reconnectStartTime == DateTime.MinValue)
                            {
                                this.connectionStartedAt = this.reconnectStartTime = DateTime.Now;

                                try
                                {
                                    if (this.OnReconnecting != null)
                                        this.OnReconnecting(this, errorReason);
                                }
                                catch (Exception ex)
                                {
                                    HTTPManager.Logger.Exception("HubConnection", "SetState - ConnectionStates.Reconnecting", ex, this.Context);
                                }
                            }

                            RetryContext context = new RetryContext
                            {
                                ElapsedTime = DateTime.Now - this.reconnectStartTime,
                                PreviousRetryCount = this.currentContext.PreviousRetryCount,
                                RetryReason = errorReason
                            };

                            TimeSpan? nextAttempt = null;
                            try
                            {
                                nextAttempt = this.ReconnectPolicy.GetNextRetryDelay(context);
                            }
                            catch (Exception ex)
                            {
                                HTTPManager.Logger.Exception("HubConnection", "ReconnectPolicy.GetNextRetryDelay", ex, this.Context);
                            }

                            // No more reconnect attempt, we are closing
                            if (nextAttempt == null)
                            {
                                HTTPManager.Logger.Warning("HubConnecction", "No more reconnect attempt!", this.Context);

                                // Clean up everything
                                this.currentContext = new RetryContext();
                                this.reconnectStartTime = DateTime.MinValue;
                                this.reconnectAt = DateTime.MinValue;
                            }
                            else
                            {
                                HTTPManager.Logger.Information("HubConnecction", "Next reconnect attempt after " + nextAttempt.Value.ToString(), this.Context);

                                this.currentContext = context;
                                this.currentContext.PreviousRetryCount += 1;

                                this.reconnectAt = DateTime.Now + nextAttempt.Value;

                                this.SetState(ConnectionStates.Reconnecting);

                                return;
                            }
                        }

                        if (this.OnError != null)
                        {
                            try
                            {
                                this.OnError(this, errorReason);
                            }
                            catch(Exception ex)
                            {
                                HTTPManager.Logger.Exception("HubConnection", "Exception in OnError user code!", ex, this.Context);
                            }
                        }
                    }

                    HTTPManager.Heartbeats.Unsubscribe(this);
                    this.rwLock.Dispose();
                    this.rwLock = null;
                    break;
            }
        }

        void BestHTTP.Extensions.IHeartbeat.OnHeartbeatUpdate(TimeSpan dif)
        {
            switch (this.State)
            {
                case ConnectionStates.Negotiating:
                case ConnectionStates.Authenticating:
                case ConnectionStates.Redirected:
                    if (DateTime.Now >= this.connectionStartedAt + this.Options.ConnectTimeout)
                    {
                        if (this.AuthenticationProvider != null)
                        {
                            this.AuthenticationProvider.OnAuthenticationSucceded -= OnAuthenticationSucceded;
                            this.AuthenticationProvider.OnAuthenticationFailed -= OnAuthenticationFailed;

                            try
                            {
                                this.AuthenticationProvider.Cancel();
                            }
                            catch(Exception ex)
                            {
                                HTTPManager.Logger.Exception("HubConnection", "Exception in AuthenticationProvider.Cancel !", ex, this.Context);
                            }
                        }

                        if (this.Transport != null)
                        {
                            this.Transport.OnStateChanged -= Transport_OnStateChanged;
                            this.Transport.StartClose();
                        }

                        SetState(ConnectionStates.Closed, string.Format("Couldn't connect in the given time({0})!", this.Options.ConnectTimeout));
                    }
                    break;

                case ConnectionStates.Connected:
                    if (this.Options.PingInterval != TimeSpan.Zero && DateTime.Now - this.lastMessageReceivedAt >= this.Options.PingTimeoutInterval)
                    {
                        // The transport itself can be in a failure state or in a completely valid one, so while we do not want to receive anything from it, we have to try to close it
                        if (this.Transport != null)
                        {
                            this.Transport.OnStateChanged -= Transport_OnStateChanged;
                            this.Transport.StartClose();
                        }

                        SetState(ConnectionStates.Closed, string.Format("PingInterval set to '{0}' and no message is received since '{1}'. PingTimeoutInterval: '{2}'", this.Options.PingInterval, this.lastMessageReceivedAt, this.Options.PingTimeoutInterval));
                    }
                    else if (this.Options.PingInterval != TimeSpan.Zero && DateTime.Now - this.lastMessageSentAt >= this.Options.PingInterval)
                        SendMessage(new Message() { type = MessageTypes.Ping });
                    break;

                case ConnectionStates.Reconnecting:
                    if (this.reconnectAt != DateTime.MinValue && DateTime.Now >= this.reconnectAt)
                    {
                        this.connectionStartedAt = DateTime.Now;
                        this.reconnectAt = DateTime.MinValue;
                        this.triedoutTransports.Clear();
                        this.StartConnect();
                    }
                    break;
            }
        }
    }
}

#endif
