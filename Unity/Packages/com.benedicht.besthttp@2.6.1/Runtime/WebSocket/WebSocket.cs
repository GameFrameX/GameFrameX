#if !BESTHTTP_DISABLE_WEBSOCKET

using System;
using System.Text;
using BestHTTP.Extensions;
using BestHTTP.Connections;
using BestHTTP.Logger;
using BestHTTP.PlatformSupport.Memory;

#if !UNITY_WEBGL || UNITY_EDITOR
    using BestHTTP.WebSocket.Frames;
    using BestHTTP.WebSocket.Extensions;
#endif

namespace BestHTTP.WebSocket
{
    public sealed class WebSocket
    {
        /// <summary>
        /// Maximum payload size of a websocket frame. Its default value is 32 KiB.
        /// </summary>
        public static uint MaxFragmentSize = UInt16.MaxValue / 2;

        public WebSocketStates State { get { return this.implementation.State; } }

        /// <summary>
        /// The connection to the WebSocket server is open.
        /// </summary>
        public bool IsOpen { get { return this.implementation.IsOpen; } }

        /// <summary>
        /// Data waiting to be written to the wire.
        /// </summary>
        public int BufferedAmount { get { return this.implementation.BufferedAmount; } }

#if !UNITY_WEBGL || UNITY_EDITOR

        /// <summary>
        /// Set to true to start a new thread to send Pings to the WebSocket server
        /// </summary>
        public bool StartPingThread { get; set; }

        /// <summary>
        /// The delay between two Pings in milliseconds. Minimum value is 100, default is 1000.
        /// </summary>
        public int PingFrequency { get; set; }

        /// <summary>
        /// If StartPingThread set to true, the plugin will close the connection and emit an OnError event if no
        /// message is received from the server in the given time. Its default value is 2 sec.
        /// </summary>
        public TimeSpan CloseAfterNoMessage { get; set; }

        /// <summary>
        /// The internal HTTPRequest object.
        /// </summary>
        public HTTPRequest InternalRequest { get { return this.implementation.InternalRequest; } }

        /// <summary>
        /// IExtension implementations the plugin will negotiate with the server to use.
        /// </summary>
        public IExtension[] Extensions { get; private set; }

        /// <summary>
        /// Latency calculated from the ping-pong message round-trip times.
        /// </summary>
        public int Latency { get { return this.implementation.Latency; } }

        /// <summary>
        /// When we received the last message from the server.
        /// </summary>
        public DateTime LastMessageReceived { get { return this.implementation.LastMessageReceived; } }

        /// <summary>
        /// When the Websocket Over HTTP/2 implementation fails to connect and EnableImplementationFallback is true, the plugin tries to fall back to the HTTP/1 implementation.
        /// When this happens a new InternalRequest is created and all previous custom modifications (like added headers) are lost. With OnInternalRequestCreated these modifications can be reapplied.
        /// </summary>
        public Action<WebSocket, HTTPRequest> OnInternalRequestCreated;
#endif

        /// <summary>
        /// Called when the connection to the WebSocket server is established.
        /// </summary>
        public OnWebSocketOpenDelegate OnOpen;

        /// <summary>
        /// Called when a new textual message is received from the server.
        /// </summary>
        public OnWebSocketMessageDelegate OnMessage;

        /// <summary>
        /// Called when a new binary message is received from the server.
        /// </summary>
        public OnWebSocketBinaryDelegate OnBinary;

        /// <summary>
        /// Called when the WebSocket connection is closed.
        /// </summary>
        public OnWebSocketClosedDelegate OnClosed;

        /// <summary>
        /// Called when an error is encountered. The parameter will be the description of the error.
        /// </summary>
        public OnWebSocketErrorDelegate OnError;

#if !UNITY_WEBGL || UNITY_EDITOR
        /// <summary>
        /// Called when an incomplete frame received. No attempt will be made to reassemble these fragments internally, and no reference are stored after this event to this frame.
        /// </summary>
        public OnWebSocketIncompleteFrameDelegate OnIncompleteFrame;
#endif

        /// <summary>
        /// Logging context of this websocket instance.
        /// </summary>
        public LoggingContext Context { get; private set; }

        /// <summary>
        /// The underlying, real implementation.
        /// </summary>
        private WebSocketBaseImplementation implementation;

        /// <summary>
        /// Creates a WebSocket instance from the given uri.
        /// </summary>
        /// <param name="uri">The uri of the WebSocket server</param>
        public WebSocket(Uri uri)
            :this(uri, string.Empty, string.Empty)
        {
#if (!UNITY_WEBGL || UNITY_EDITOR) && !BESTHTTP_DISABLE_GZIP
            this.Extensions = new IExtension[] { new PerMessageCompression(/*compression level: */           Decompression.Zlib.CompressionLevel.Default,
                                                                           /*clientNoContextTakeover: */     false,
                                                                           /*serverNoContextTakeover: */     false,
                                                                           /*clientMaxWindowBits: */         Decompression.Zlib.ZlibConstants.WindowBitsMax,
                                                                           /*desiredServerMaxWindowBits: */  Decompression.Zlib.ZlibConstants.WindowBitsMax,
                                                                           /*minDatalengthToCompress: */     PerMessageCompression.MinDataLengthToCompressDefault) };
#endif
        }

#if !UNITY_WEBGL || UNITY_EDITOR
        public WebSocket(Uri uri, string origin, string protocol)
            :this(uri, origin, protocol, null)
        {
#if !BESTHTTP_DISABLE_GZIP
            this.Extensions = new IExtension[] { new PerMessageCompression(/*compression level: */           Decompression.Zlib.CompressionLevel.Default,
                                                                           /*clientNoContextTakeover: */     false,
                                                                           /*serverNoContextTakeover: */     false,
                                                                           /*clientMaxWindowBits: */         Decompression.Zlib.ZlibConstants.WindowBitsMax,
                                                                           /*desiredServerMaxWindowBits: */  Decompression.Zlib.ZlibConstants.WindowBitsMax,
                                                                           /*minDatalengthToCompress: */     PerMessageCompression.MinDataLengthToCompressDefault) };
#endif
        }
#endif

        /// <summary>
        /// Creates a WebSocket instance from the given uri, protocol and origin.
        /// </summary>
        /// <param name="uri">The uri of the WebSocket server</param>
        /// <param name="origin">Servers that are not intended to process input from any web page but only for certain sites SHOULD verify the |Origin| field is an origin they expect.
        /// If the origin indicated is unacceptable to the server, then it SHOULD respond to the WebSocket handshake with a reply containing HTTP 403 Forbidden status code.</param>
        /// <param name="protocol">The application-level protocol that the client want to use(eg. "chat", "leaderboard", etc.). Can be null or empty string if not used.</param>
        /// <param name="extensions">Optional IExtensions implementations</param>
        public WebSocket(Uri uri, string origin, string protocol
#if !UNITY_WEBGL || UNITY_EDITOR
            , params IExtension[] extensions
#endif
            )

        {
            this.Context = new LoggingContext(this);

#if !UNITY_WEBGL || UNITY_EDITOR
            this.Extensions = extensions;

#if !BESTHTTP_DISABLE_ALTERNATE_SSL && !BESTHTTP_DISABLE_HTTP2
            if (HTTPManager.HTTP2Settings.WebSocketOverHTTP2Settings.EnableWebSocketOverHTTP2 && HTTPProtocolFactory.IsSecureProtocol(uri))
            {
                // Try to find a HTTP/2 connection that supports the connect protocol.
                var con = BestHTTP.Core.HostManager.GetHost(uri.Host).GetHostDefinition(Core.HostDefinition.GetKeyFor(new UriBuilder("https", uri.Host, uri.Port).Uri
#if !BESTHTTP_DISABLE_PROXY
                , GetProxy(uri)
#endif
                )).Find(c => {
                    var httpConnection = c as HTTPConnection;
                    var http2Handler = httpConnection?.requestHandler as Connections.HTTP2.HTTP2Handler;

                    return http2Handler != null && http2Handler.settings.RemoteSettings[Connections.HTTP2.HTTP2Settings.ENABLE_CONNECT_PROTOCOL] != 0;
                });

                if (con != null)
                {
                    HTTPManager.Logger.Information("WebSocket", "Connection with enabled Connect Protocol found!", this.Context);

                    var httpConnection = con as HTTPConnection;
                    var http2Handler = httpConnection?.requestHandler as Connections.HTTP2.HTTP2Handler;

                    this.implementation = new OverHTTP2(this, http2Handler, uri, origin, protocol);
                }
            }
#endif

            if (this.implementation == null)
                this.implementation = new OverHTTP1(this, uri, origin, protocol);
#else
            this.implementation = new WebGLBrowser(this, uri, origin, protocol);
#endif

            // Under WebGL when only the WebSocket protocol is used Setup() isn't called, so we have to call it here.
            HTTPManager.Setup();
        }

#if !UNITY_WEBGL || UNITY_EDITOR
        internal void FallbackToHTTP1()
        {
            if (this.implementation == null)
                return;

            this.implementation = new OverHTTP1(this, this.implementation.Uri, this.implementation.Origin, this.implementation.Protocol);
            this.implementation.StartOpen();
        }
#endif

        /// <summary>
        /// Start the opening process.
        /// </summary>
        public void Open()
        {
            this.implementation.StartOpen();
        }

        /// <summary>
        /// It will send the given message to the server in one frame.
        /// </summary>
        public void Send(string message)
        {
            if (!IsOpen)
                return;

            this.implementation.Send(message);
        }

        /// <summary>
        /// It will send the given data to the server in one frame.
        /// </summary>
        public void Send(byte[] buffer)
        {
            if (!IsOpen)
                return;

            this.implementation.Send(buffer);
        }

        /// <summary>
        /// Will send count bytes from a byte array, starting from offset.
        /// </summary>
        public void Send(byte[] buffer, ulong offset, ulong count)
        {
            if (!IsOpen)
                return;

            this.implementation.Send(buffer, offset, count);
        }

#if !UNITY_WEBGL || UNITY_EDITOR
        /// <summary>
        /// It will send the given frame to the server.
        /// </summary>
        public void Send(WebSocketFrame frame)
        {
            if (!IsOpen)
                return;

            this.implementation.Send(frame);
        }
#endif

        /// <summary>
        /// It will initiate the closing of the connection to the server.
        /// </summary>
        public void Close()
        {
            if (State >= WebSocketStates.Closing)
                return;

            this.implementation.StartClose(1000, "Bye!");
        }

        /// <summary>
        /// It will initiate the closing of the connection to the server sending the given code and message.
        /// </summary>
        public void Close(UInt16 code, string message)
        {
            if (!IsOpen)
                return;

            this.implementation.StartClose(code, message);
        }

#if !BESTHTTP_DISABLE_PROXY
        internal Proxy GetProxy(Uri uri)
        {
            // WebSocket is not a request-response based protocol, so we need a 'tunnel' through the proxy
            HTTPProxy proxy = HTTPManager.Proxy as HTTPProxy;
            if (proxy != null && proxy.UseProxyForAddress(uri))
                proxy = new HTTPProxy(proxy.Address,
                                      proxy.Credentials,
                                      false, /*turn on 'tunneling'*/
                                      false, /*sendWholeUri*/
                                      proxy.NonTransparentForHTTPS);

            return proxy;
        }
#endif

#if !UNITY_WEBGL || UNITY_EDITOR

        public static byte[] EncodeCloseData(UInt16 code, string message)
        {
            //If there is a body, the first two bytes of the body MUST be a 2-byte unsigned integer
            // (in network byte order) representing a status code with value /code/ defined in Section 7.4 (http://tools.ietf.org/html/rfc6455#section-7.4). Following the 2-byte integer,
            // the body MAY contain UTF-8-encoded data with value /reason/, the interpretation of which is not defined by this specification.
            // This data is not necessarily human readable but may be useful for debugging or passing information relevant to the script that opened the connection.
            int msgLen = Encoding.UTF8.GetByteCount(message);
            using (var ms = new BufferPoolMemoryStream(2 + msgLen))
            {
                byte[] buff = BitConverter.GetBytes(code);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(buff, 0, buff.Length);

                ms.Write(buff, 0, buff.Length);

                buff = Encoding.UTF8.GetBytes(message);
                ms.Write(buff, 0, buff.Length);

                return ms.ToArray();
            }
        }

        internal static string GetSecKey(object[] from)
        {
            const int keysLength = 16;
            byte[] keys = BufferPool.Get(keysLength, true);
            int pos = 0;

            for (int i = 0; i < from.Length; ++i)
            {
                byte[] hash = BitConverter.GetBytes((Int32)from[i].GetHashCode());

                for (int cv = 0; cv < hash.Length && pos < keysLength; ++cv)
                    keys[pos++] = hash[cv];
            }

            var result = Convert.ToBase64String(keys, 0, keysLength);
            BufferPool.Release(keys);

            return result;
        }
#endif
    }
}

#endif
