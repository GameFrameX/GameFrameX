using System;
using System.Collections.Generic;

#if !BESTHTTP_DISABLE_CACHING
using BestHTTP.Caching;
#endif

using BestHTTP.Core;
using BestHTTP.Extensions;
using BestHTTP.Logger;
using BestHTTP.PlatformSupport.Memory;

#if !BESTHTTP_DISABLE_COOKIES
using BestHTTP.Cookies;
#endif

using BestHTTP.Connections;

namespace BestHTTP
{
    public enum ShutdownTypes
    {
        Running,
        Gentle,
        Immediate
    }

#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
    public delegate Connections.TLS.AbstractTls13Client TlsClientFactoryDelegate(HTTPRequest request, List<SecureProtocol.Org.BouncyCastle.Tls.ProtocolName> protocols);
#endif

    public delegate System.Security.Cryptography.X509Certificates.X509Certificate ClientCertificateSelector(HTTPRequest request, string targetHost, System.Security.Cryptography.X509Certificates.X509CertificateCollection localCertificates, System.Security.Cryptography.X509Certificates.X509Certificate remoteCertificate, string[] acceptableIssuers);

    /// <summary>
    ///
    /// </summary>
    [BestHTTP.PlatformSupport.IL2CPP.Il2CppEagerStaticClassConstructionAttribute]
    public static partial class HTTPManager
    {
        // Static constructor. Setup default values
        static HTTPManager()
        {
            MaxConnectionPerServer = 6;
            KeepAliveDefaultValue = true;
            MaxPathLength = 255;
            MaxConnectionIdleTime = TimeSpan.FromSeconds(20);

#if !BESTHTTP_DISABLE_COOKIES
#if UNITY_WEBGL && !UNITY_EDITOR
            // Under webgl when IsCookiesEnabled is true, it will set the withCredentials flag for the XmlHTTPRequest
            //  and that's different from the default behavior.
            // https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest/withCredentials
            IsCookiesEnabled = false;
#else
            IsCookiesEnabled = true;
#endif
#endif

            CookieJarSize = 10 * 1024 * 1024;
            EnablePrivateBrowsing = false;
            ConnectTimeout = TimeSpan.FromSeconds(20);
            RequestTimeout = TimeSpan.FromSeconds(60);

            // Set the default logger mechanism
            logger = new BestHTTP.Logger.ThreadedLogger();

#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
            UseAlternateSSLDefaultValue = true;
#endif

#if NETFX_CORE
            IOService = new PlatformSupport.FileSystem.NETFXCOREIOService();
//#elif UNITY_WEBGL && !UNITY_EDITOR
//            IOService = new PlatformSupport.FileSystem.WebGLIOService();
#else
            IOService = new PlatformSupport.FileSystem.DefaultIOService();
#endif
        }

#if (!UNITY_WEBGL || UNITY_EDITOR) && !BESTHTTP_DISABLE_ALTERNATE_SSL && !BESTHTTP_DISABLE_HTTP2
        /// <summary>
        /// HTTP/2 settings
        /// </summary>
        public static Connections.HTTP2.HTTP2PluginSettings HTTP2Settings = new Connections.HTTP2.HTTP2PluginSettings();
#endif

#region Global Options

        /// <summary>
        /// The maximum active TCP connections that the client will maintain to a server. Default value is 6. Minimum value is 1.
        /// </summary>
        public static byte MaxConnectionPerServer
        {
            get{ return maxConnectionPerServer; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("MaxConnectionPerServer must be greater than 0!");

                bool isGrowing = value > maxConnectionPerServer;
                maxConnectionPerServer = value;

                // If the allowed connections per server is growing, go through all hosts and try to send out queueud requests.
                if (isGrowing)
                    HostManager.TryToSendQueuedRequests();
            }
        }
        private static byte maxConnectionPerServer;

        /// <summary>
        /// Default value of a HTTP request's IsKeepAlive value. Default value is true. If you make rare request to the server it should be changed to false.
        /// </summary>
        public static bool KeepAliveDefaultValue { get; set; }

#if !BESTHTTP_DISABLE_CACHING
        /// <summary>
        /// Set to true, if caching is prohibited.
        /// </summary>
        public static bool IsCachingDisabled { get; set; }
#endif

        /// <summary>
        /// How many time must be passed to destroy that connection after a connection finished its last request. Its default value is 20 seconds.
        /// </summary>
        public static TimeSpan MaxConnectionIdleTime { get; set; }

#if !BESTHTTP_DISABLE_COOKIES
        /// <summary>
        /// Set to false to disable all Cookie. It's default value is true.
        /// </summary>
        public static bool IsCookiesEnabled { get; set; }
#endif

        /// <summary>
        /// Size of the Cookie Jar in bytes. It's default value is 10485760 (10 MB).
        /// </summary>
        public static uint CookieJarSize { get; set; }

        /// <summary>
        /// If this property is set to true, then new cookies treated as session cookies and these cookies are not saved to disk. Its default value is false;
        /// </summary>
        public static bool EnablePrivateBrowsing { get; set; }

        /// <summary>
        /// Global, default value of the HTTPRequest's ConnectTimeout property. If set to TimeSpan.Zero or lower, no connect timeout logic is executed. Default value is 20 seconds.
        /// </summary>
        public static TimeSpan ConnectTimeout { get; set; }

        /// <summary>
        /// Global, default value of the HTTPRequest's Timeout property. Default value is 60 seconds.
        /// </summary>
        public static TimeSpan RequestTimeout { get; set; }

        /// <summary>
        /// By default the plugin will save all cache and cookie data under the path returned by Application.persistentDataPath.
        /// You can assign a function to this delegate to return a custom root path to define a new path.
        /// <remarks>This delegate will be called on a non Unity thread!</remarks>
        /// </summary>
        public static System.Func<string> RootCacheFolderProvider { get; set; }

#if !BESTHTTP_DISABLE_PROXY
        /// <summary>
        /// The global, default proxy for all HTTPRequests. The HTTPRequest's Proxy still can be changed per-request. Default value is null.
        /// </summary>
        public static Proxy Proxy { get; set; }
#endif

        /// <summary>
        /// Heartbeat manager to use less threads in the plugin. The heartbeat updates are called from the OnUpdate function.
        /// </summary>
        public static HeartbeatManager Heartbeats
        {
            get
            {
                if (heartbeats == null)
                    heartbeats = new HeartbeatManager();
                return heartbeats;
            }
        }
        private static HeartbeatManager heartbeats;

        /// <summary>
        /// A basic BestHTTP.Logger.ILogger implementation to be able to log intelligently additional informations about the plugin's internal mechanism.
        /// </summary>
        public static BestHTTP.Logger.ILogger Logger
        {
            get
            {
                // Make sure that it has a valid logger instance.
                if (logger == null)
                {
                    logger = new ThreadedLogger();
                    logger.Level = Loglevels.None;
                }

                return logger;
            }

            set { logger = value; }
        }
        private static BestHTTP.Logger.ILogger logger;

#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

        public static TlsClientFactoryDelegate TlsClientFactory;

        public static Connections.TLS.AbstractTls13Client DefaultTlsClientFactory(HTTPRequest request, List<SecureProtocol.Org.BouncyCastle.Tls.ProtocolName> protocols)
        {
            // http://tools.ietf.org/html/rfc3546#section-3.1
            // -It is RECOMMENDED that clients include an extension of type "server_name" in the client hello whenever they locate a server by a supported name type.
            // -Literal IPv4 and IPv6 addresses are not permitted in "HostName".

            // User-defined list has a higher priority
            List<SecureProtocol.Org.BouncyCastle.Tls.ServerName> hostNames = null;

            // If there's no user defined one and the host isn't an IP address, add the default one
            if (!request.CurrentUri.IsHostIsAnIPAddress())
            {
                hostNames = new List<SecureProtocol.Org.BouncyCastle.Tls.ServerName>(1);
                hostNames.Add(new SecureProtocol.Org.BouncyCastle.Tls.ServerName(0, System.Text.Encoding.UTF8.GetBytes(request.CurrentUri.Host)));
            }

            return new Connections.TLS.DefaultTls13Client(request, hostNames, protocols);
        }

        /// <summary>
        /// The default value for the HTTPRequest's UseAlternateSSL property.
        /// </summary>
        public static bool UseAlternateSSLDefaultValue { get; set; }
#endif

#if !NETFX_CORE
        public static Func<HTTPRequest, System.Security.Cryptography.X509Certificates.X509Certificate, System.Security.Cryptography.X509Certificates.X509Chain, System.Net.Security.SslPolicyErrors, bool> DefaultCertificationValidator;
        public static ClientCertificateSelector ClientCertificationProvider;
#endif

        /// <summary>
        /// TCP Client's send buffer size.
        /// </summary>
        public static int? SendBufferSize;

        /// <summary>
        /// TCP Client's receive buffer size.
        /// </summary>
        public static int? ReceiveBufferSize;

        /// <summary>
        /// An IIOService implementation to handle filesystem operations.
        /// </summary>
        public static PlatformSupport.FileSystem.IIOService IOService;

        /// <summary>
        /// On most systems the maximum length of a path is around 255 character. If a cache entity's path is longer than this value it doesn't get cached. There no platform independent API to query the exact value on the current system, but it's
        /// exposed here and can be overridden. It's default value is 255.
        /// </summary>
        internal static int MaxPathLength { get; set; }

        /// <summary>
        /// User-agent string that will be sent with each requests.
        /// </summary>
        public static string UserAgent = "BestHTTP/2 v2.6.0";

        /// <summary>
        /// It's true if the application is quitting and the plugin is shutting down itself.
        /// </summary>
        public static bool IsQuitting { get { return _isQuitting; } private set { _isQuitting = value; } }
        private static volatile bool _isQuitting;
#endregion

#region Manager variables

        private static bool IsSetupCalled;

#endregion

#region Public Interface

        public static void Setup()
        {
            if (IsSetupCalled)
                return;
            IsSetupCalled = true;
            IsQuitting = false;

            HTTPManager.Logger.Information("HTTPManager", "Setup called! UserAgent: " + UserAgent);

            HTTPUpdateDelegator.CheckInstance();

#if !BESTHTTP_DISABLE_CACHING
            HTTPCacheService.CheckSetup();
#endif

#if !BESTHTTP_DISABLE_COOKIES
            Cookies.CookieJar.SetupFolder();
            Cookies.CookieJar.Load();
#endif

            HostManager.Load();
        }

        public static HTTPRequest SendRequest(string url, OnRequestFinishedDelegate callback)
        {
            return SendRequest(new HTTPRequest(new Uri(url), HTTPMethods.Get, callback));
        }

        public static HTTPRequest SendRequest(string url, HTTPMethods methodType, OnRequestFinishedDelegate callback)
        {
            return SendRequest(new HTTPRequest(new Uri(url), methodType, callback));
        }

        public static HTTPRequest SendRequest(string url, HTTPMethods methodType, bool isKeepAlive, OnRequestFinishedDelegate callback)
        {
            return SendRequest(new HTTPRequest(new Uri(url), methodType, isKeepAlive, callback));
        }

        public static HTTPRequest SendRequest(string url, HTTPMethods methodType, bool isKeepAlive, bool disableCache, OnRequestFinishedDelegate callback)
        {
            return SendRequest(new HTTPRequest(new Uri(url), methodType, isKeepAlive, disableCache, callback));
        }

        public static HTTPRequest SendRequest(HTTPRequest request)
        {
            if (!IsSetupCalled)
                Setup();

            if (request.IsCancellationRequested || IsQuitting)
                return request;

#if !BESTHTTP_DISABLE_CACHING
            // If possible load the full response from cache.
            if (Caching.HTTPCacheService.IsCachedEntityExpiresInTheFuture(request))
            {
                DateTime started = DateTime.Now;
                PlatformSupport.Threading.ThreadedRunner.RunShortLiving<HTTPRequest>((req) =>
                {
                    if (Connections.ConnectionHelper.TryLoadAllFromCache("HTTPManager", req, req.Context))
                    {
                        req.Timing.Add("Full Cache Load", DateTime.Now - started);
                        req.State = HTTPRequestStates.Finished;
                    }
                    else
                    {
                        // If for some reason it couldn't load we place back the request to the queue.

                        request.State = HTTPRequestStates.Queued;
                        RequestEventHelper.EnqueueRequestEvent(new RequestEventInfo(request, RequestEvents.Resend));
                    }
                }, request);
            }
            else
#endif
            {
                request.State = HTTPRequestStates.Queued;
                RequestEventHelper.EnqueueRequestEvent(new RequestEventInfo(request, RequestEvents.Resend));
            }

            return request;
        }

#endregion

#region Internal Helper Functions

        /// <summary>
        /// Will return where the various caches should be saved.
        /// </summary>
        public static string GetRootCacheFolder()
        {
            try
            {
                if (RootCacheFolderProvider != null)
                    return RootCacheFolderProvider();
            }
            catch(Exception ex)
            {
                HTTPManager.Logger.Exception("HTTPManager", "GetRootCacheFolder", ex);
            }

#if NETFX_CORE
            return Windows.Storage.ApplicationData.Current.LocalFolder.Path;
#else
            return UnityEngine.Application.persistentDataPath;
#endif
        }

#if UNITY_EDITOR
#if UNITY_2019_3_OR_NEWER
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
        public static void ResetSetup()
        {
            IsSetupCalled = false;
            BufferedReadNetworkStream.ResetNetworkStats();
            HTTPManager.Logger.Information("HTTPManager", "Reset called!");
        }
#endif

#endregion

#region MonoBehaviour Events (Called from HTTPUpdateDelegator)

        /// <summary>
        /// Update function that should be called regularly from a Unity event(Update, LateUpdate). Callbacks are dispatched from this function.
        /// </summary>
        public static void OnUpdate()
        {
            RequestEventHelper.ProcessQueue();
            ConnectionEventHelper.ProcessQueue();
            ProtocolEventHelper.ProcessQueue();
            PluginEventHelper.ProcessQueue();

            BestHTTP.Extensions.Timer.Process();

            if (heartbeats != null)
                heartbeats.Update();

            BufferPool.Maintain();
        }

        public static void OnQuit()
        {
            HTTPManager.Logger.Information("HTTPManager", "OnQuit called!");

            IsQuitting = true;

            AbortAll();

#if !BESTHTTP_DISABLE_CACHING
            HTTPCacheService.SaveLibrary();
#endif

#if !BESTHTTP_DISABLE_COOKIES
            CookieJar.Persist();
#endif

            OnUpdate();

            HostManager.Clear();

            Heartbeats.Clear();
        }

        public static void AbortAll()
        {
            HTTPManager.Logger.Information("HTTPManager", "AbortAll called!");

            // This is an immediate shutdown request!

            RequestEventHelper.Clear();
            ConnectionEventHelper.Clear();
            PluginEventHelper.Clear();
            ProtocolEventHelper.Clear();

            HostManager.Shutdown();

            ProtocolEventHelper.CancelActiveProtocols();
        }

#endregion
    }
}
