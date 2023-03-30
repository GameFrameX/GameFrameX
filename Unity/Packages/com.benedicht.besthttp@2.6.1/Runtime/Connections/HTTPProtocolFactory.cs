using System;
using System.IO;

namespace BestHTTP.Connections
{
    public enum SupportedProtocols
    {
        Unknown,
        HTTP,

#if !BESTHTTP_DISABLE_WEBSOCKET
        WebSocket,
#endif

#if !BESTHTTP_DISABLE_SERVERSENT_EVENTS
        ServerSentEvents
#endif
    }

    public static class HTTPProtocolFactory
    {
        public const string W3C_HTTP1 = "http/1.1";
#if (!UNITY_WEBGL || UNITY_EDITOR) && !BESTHTTP_DISABLE_ALTERNATE_SSL && !BESTHTTP_DISABLE_HTTP2
        public const string W3C_HTTP2 = "h2";
#endif

        public static HTTPResponse Get(SupportedProtocols protocol, HTTPRequest request, Stream stream, bool isStreamed, bool isFromCache)
        {
            switch (protocol)
            {
#if !BESTHTTP_DISABLE_WEBSOCKET && (!UNITY_WEBGL || UNITY_EDITOR)
                case SupportedProtocols.WebSocket: return new WebSocket.WebSocketResponse(request, stream, isStreamed, isFromCache);
#endif
                default: return new HTTPResponse(request, stream, isStreamed, isFromCache);
            }
        }

        public static SupportedProtocols GetProtocolFromUri(Uri uri)
        {
            if (uri == null || uri.Scheme == null)
                throw new Exception("Malformed URI in GetProtocolFromUri");

            string scheme = uri.Scheme.ToLowerInvariant();
            switch (scheme)
            {
#if !BESTHTTP_DISABLE_WEBSOCKET
                case "ws":
                case "wss":
                    return SupportedProtocols.WebSocket;
#endif

                default:
                    return SupportedProtocols.HTTP;
            }
        }

        public static bool IsSecureProtocol(Uri uri)
        {
            if (uri == null || uri.Scheme == null)
                throw new Exception("Malformed URI in IsSecureProtocol");

            string scheme = uri.Scheme.ToLowerInvariant();
            switch (scheme)
            {
                // http
                case "https":

#if !BESTHTTP_DISABLE_WEBSOCKET
                // WebSocket
                case "wss":
#endif
                    return true;
            }

            return false;
        }
    }
}
