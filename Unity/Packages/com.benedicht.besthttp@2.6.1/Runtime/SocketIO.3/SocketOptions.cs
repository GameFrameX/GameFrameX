#if !BESTHTTP_DISABLE_SOCKETIO

using System;
using System.Text;

using PlatformSupport.Collections.ObjectModel;

#if !NETFX_CORE
    using PlatformSupport.Collections.Specialized;
#else
    using System.Collections.Specialized;
#endif

namespace BestHTTP.SocketIO3
{
    public delegate void HTTPRequestCallbackDelegate(SocketManager manager, HTTPRequest request);

    public sealed class SocketOptions
    {
        #region Properties

        /// <summary>
        /// The SocketManager will try to connect with this transport.
        /// </summary>
        public Transports.TransportTypes ConnectWith { get; set; }

        /// <summary>
        /// Whether to reconnect automatically after a disconnect (default true)
        /// </summary>
        public bool Reconnection { get; set; }

        /// <summary>
        /// Number of attempts before giving up (default Int.MaxValue)
        /// </summary>
        public int ReconnectionAttempts { get; set; }

        /// <summary>
        /// How long to initially wait before attempting a new reconnection (default 1000ms).
        /// Affected by +/- RandomizationFactor, for example the default initial delay will be between 500ms to 1500ms.
        /// </summary>
        public TimeSpan ReconnectionDelay { get; set; }

        /// <summary>
        /// Maximum amount of time to wait between reconnections (default 5000ms).
        /// Each attempt increases the reconnection delay along with a randomization as above.
        /// </summary>
        public TimeSpan ReconnectionDelayMax { get; set; }

        /// <summary>
        /// (default 0.5`), [0..1]
        /// </summary>
        public float RandomizationFactor { get { return randomizationFactor; } set { randomizationFactor = Math.Min(1.0f, Math.Max(0.0f, value)); } }
        private float randomizationFactor;

        /// <summary>
        /// Connection timeout before a connect_error and connect_timeout events are emitted (default 20000ms)
        /// </summary>
        public TimeSpan Timeout { get; set; }

        /// <summary>
        /// By setting this false, you have to call SocketManager's Open() whenever you decide it's appropriate.
        /// </summary>
        public bool AutoConnect { get; set; }

        /// <summary>
        /// Additional query parameters that will be passed for accessed uris. If the value is null, or an empty string it will be not appended to the query only the key.
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
        /// If it's false, the parameters in the AdditionalQueryParams will be passed for all HTTP requests. Its default value is true.
        /// </summary>
        public bool QueryParamsOnlyForHandshake { get; set; }

        /// <summary>
        /// A callback that called for every HTTPRequest the socket.io protocol sends out. It can be used to further customize (add additional request for example) requests.
        /// </summary>
        public HTTPRequestCallbackDelegate HTTPRequestCustomizationCallback { get; set; }

        /// <summary>
        /// Starting with Socket.IO v3, connecting to a namespace a client can send payload data. When the Auth callback function is set, the plugin going to call it when connecting to a namespace. Its return value must be a json string!
        /// </summary>
        public Func<SocketManager, Socket, object> Auth;

        #endregion

        /// <summary>
        /// The cached value of the result of the BuildQueryParams() call.
        /// </summary>
        private string BuiltQueryParams;

        /// <summary>
        /// Constructor, setting the default option values.
        /// </summary>
        public SocketOptions()
        {
            ConnectWith = Transports.TransportTypes.Polling;
            Reconnection = true;
            ReconnectionAttempts = int.MaxValue;
            ReconnectionDelay = TimeSpan.FromMilliseconds(1000);
            ReconnectionDelayMax = TimeSpan.FromMilliseconds(5000);
            RandomizationFactor = 0.5f;
            Timeout = TimeSpan.FromMilliseconds(20000);
            AutoConnect = true;
            QueryParamsOnlyForHandshake = true;
        }

        #region Helper Functions

        /// <summary>
        /// Builds the keys and values from the AdditionalQueryParams to an key=value form. If AdditionalQueryParams is null or empty, it will return an empty string.
        /// </summary>
        internal string BuildQueryParams()
        {
            if (AdditionalQueryParams == null || AdditionalQueryParams.Count == 0)
                return string.Empty;

            if (!string.IsNullOrEmpty(BuiltQueryParams))
                return BuiltQueryParams;

            StringBuilder sb = new StringBuilder(AdditionalQueryParams.Count * 4);

            foreach(var kvp in AdditionalQueryParams)
            {
                sb.Append("&");
                sb.Append(kvp.Key);

                if (!string.IsNullOrEmpty(kvp.Value))
                {
                    sb.Append("=");
                    sb.Append(kvp.Value);
                }
            }

            return BuiltQueryParams = sb.ToString();
        }

        /// <summary>
        /// This event will be called when the AdditonalQueryPrams dictionary changed. We have to reset the cached values.
        /// </summary>
        private void AdditionalQueryParams_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            BuiltQueryParams = null;
        }

        #endregion
    }
}

#endif
