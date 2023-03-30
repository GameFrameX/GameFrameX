#if !BESTHTTP_DISABLE_SIGNALR_CORE
using System;
using System.Collections.Generic;
using System.Text;
using BestHTTP.Logger;
using BestHTTP.PlatformSupport.Memory;

namespace BestHTTP.SignalRCore.Transports
{
    internal abstract class TransportBase : ITransport
    {
        public abstract TransportTypes TransportType { get; }

        public TransferModes TransferMode { get { return TransferModes.Binary; } }

        /// <summary>
        /// Current state of the transport. All changes will be propagated to the HubConnection through the onstateChanged event.
        /// </summary>
        public TransportStates State
        {
            get { return this._state; }
            protected set
            {
                if (this._state != value)
                {
                    TransportStates oldState = this._state;
                    this._state = value;

                    if (this.OnStateChanged != null)
                        this.OnStateChanged(oldState, this._state);
                }
            }
        }
        protected TransportStates _state;

        /// <summary>
        /// This will store the reason of failures so HubConnection can include it in its OnError event.
        /// </summary>
        public string ErrorReason { get; protected set; }

        /// <summary>
        /// Called every time when the transport's <see cref="State"/> changed.
        /// </summary>
        public event Action<TransportStates, TransportStates> OnStateChanged;

        public LoggingContext Context { get; protected set; }

        /// <summary>
        /// Cached list of parsed messages.
        /// </summary>
        protected List<Messages.Message> messages = new List<Messages.Message>();

        /// <summary>
        /// Parent HubConnection instance.
        /// </summary>
        protected HubConnection connection;

        internal TransportBase(HubConnection con)
        {
            this.connection = con;
            this.Context = new LoggingContext(this);
            this.Context.Add("Hub", this.connection.Context);
            this.State = TransportStates.Initial;
        }

        /// <summary>
        /// ITransport.StartConnect
        /// </summary>
        public abstract void StartConnect();

        /// <summary>
        /// ITransport.Send
        /// </summary>
        /// <param name="msg"></param>
        public abstract void Send(BufferSegment msg);

        /// <summary>
        /// ITransport.StartClose
        /// </summary>
        public abstract void StartClose();

        protected string ParseHandshakeResponse(string data)
        {
            // The handshake response is
            //  -an empty json object ('{}') if the handshake process is succesfull
            //  -otherwise it has one 'error' field

            Dictionary<string, object> response = BestHTTP.JSON.Json.Decode(data) as Dictionary<string, object>;

            if (response == null)
                return "Couldn't parse json data: " + data;

            object error;
            if (response.TryGetValue("error", out error))
                return error.ToString();

            return null;
        }

        protected void HandleHandshakeResponse(string data)
        {
            this.ErrorReason = ParseHandshakeResponse(data);

            this.State = string.IsNullOrEmpty(this.ErrorReason) ? TransportStates.Connected : TransportStates.Failed;
        }

        StringBuilder queryBuilder = new StringBuilder(3);
        protected Uri BuildUri(Uri baseUri)
        {
            if (this.connection.NegotiationResult == null)
                return baseUri;

            UriBuilder builder = new UriBuilder(baseUri);

            queryBuilder.Length = 0;

            queryBuilder.Append(baseUri.Query);
            if (!string.IsNullOrEmpty(this.connection.NegotiationResult.ConnectionToken))
                queryBuilder.Append("&id=").Append(this.connection.NegotiationResult.ConnectionToken);
            else if (!string.IsNullOrEmpty(this.connection.NegotiationResult.ConnectionId))
                queryBuilder.Append("&id=").Append(this.connection.NegotiationResult.ConnectionId);

            builder.Query = queryBuilder.ToString();

            if (builder.Query.StartsWith("??"))
                builder.Query = builder.Query.Substring(2);

            return builder.Uri;
        }
    }
}
#endif
