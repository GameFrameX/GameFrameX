#if (!UNITY_WEBGL || UNITY_EDITOR) && !BESTHTTP_DISABLE_WEBSOCKET
using System;

using BestHTTP.Connections;
using BestHTTP.Extensions;
using BestHTTP.WebSocket.Frames;

namespace BestHTTP.WebSocket
{
    internal sealed class OverHTTP1 : WebSocketBaseImplementation
    {
        public override bool IsOpen => webSocket != null && !webSocket.IsClosed;

        public override int BufferedAmount => webSocket.BufferedAmount;

        public override int Latency => this.webSocket.Latency;
        public override DateTime LastMessageReceived => this.webSocket.lastMessage;

        /// <summary>
        /// Indicates whether we sent out the connection request to the server.
        /// </summary>
        private bool requestSent;

        /// <summary>
        /// The internal WebSocketResponse object
        /// </summary>
        private WebSocketResponse webSocket;

        public OverHTTP1(WebSocket parent, Uri uri, string origin, string protocol) : base(parent, uri, origin, protocol)
        {
            string scheme = HTTPProtocolFactory.IsSecureProtocol(uri) ? "wss" : "ws";
            int port = uri.Port != -1 ? uri.Port : (scheme.Equals("wss", StringComparison.OrdinalIgnoreCase) ? 443 : 80);

            // Somehow if i use the UriBuilder it's not the same as if the uri is constructed from a string...
            //uri = new UriBuilder(uri.Scheme, uri.Host, uri.Scheme.Equals("wss", StringComparison.OrdinalIgnoreCase) ? 443 : 80, uri.PathAndQuery).Uri;
            base.Uri = new Uri(scheme + "://" + uri.Host + ":" + port + uri.GetRequestPathAndQueryURL());
        }

        protected override void CreateInternalRequest()
        {
            if (this._internalRequest != null)
                return;

            this._internalRequest = new HTTPRequest(base.Uri, OnInternalRequestCallback);

            this._internalRequest.Context.Add("WebSocket", this.Parent.Context);

            // Called when the regular GET request is successfully upgraded to WebSocket
            this._internalRequest.OnUpgraded = OnInternalRequestUpgraded;

            //http://tools.ietf.org/html/rfc6455#section-4

            // The request MUST contain an |Upgrade| header field whose value MUST include the "websocket" keyword.
            this._internalRequest.SetHeader("Upgrade", "websocket");

            // The request MUST contain a |Connection| header field whose value MUST include the "Upgrade" token.
            this._internalRequest.SetHeader("Connection", "Upgrade");

            // The request MUST include a header field with the name |Sec-WebSocket-Key|.  The value of this header field MUST be a nonce consisting of a
            // randomly selected 16-byte value that has been base64-encoded (see Section 4 of [RFC4648]).  The nonce MUST be selected randomly for each connection.
            this._internalRequest.SetHeader("Sec-WebSocket-Key", WebSocket.GetSecKey(new object[] { this, InternalRequest, base.Uri, new object() }));

            // The request MUST include a header field with the name |Origin| [RFC6454] if the request is coming from a browser client.
            // If the connection is from a non-browser client, the request MAY include this header field if the semantics of that client match the use-case described here for browser clients.
            // More on Origin Considerations: http://tools.ietf.org/html/rfc6455#section-10.2
            if (!string.IsNullOrEmpty(Origin))
                this._internalRequest.SetHeader("Origin", Origin);

            // The request MUST include a header field with the name |Sec-WebSocket-Version|.  The value of this header field MUST be 13.
            this._internalRequest.SetHeader("Sec-WebSocket-Version", "13");

            if (!string.IsNullOrEmpty(Protocol))
                this._internalRequest.SetHeader("Sec-WebSocket-Protocol", Protocol);

            // Disable caching
            this._internalRequest.SetHeader("Cache-Control", "no-cache");
            this._internalRequest.SetHeader("Pragma", "no-cache");

#if !BESTHTTP_DISABLE_CACHING
            this._internalRequest.DisableCache = true;
#endif

#if !BESTHTTP_DISABLE_PROXY
            this._internalRequest.Proxy = this.Parent.GetProxy(this.Uri);
#endif

            if (this.Parent.OnInternalRequestCreated != null)
            {
                try
                {
                    this.Parent.OnInternalRequestCreated(this.Parent, this._internalRequest);
                }
                catch (Exception ex)
                {
                    HTTPManager.Logger.Exception("OverHTTP1", "CreateInternalRequest", ex, this.Parent.Context);
                }
            }
        }

        public override void StartClose(UInt16 code, string message)
        {
            if (this.State == WebSocketStates.Connecting)
            {
                if (this.InternalRequest != null)
                    this.InternalRequest.Abort();

                this.State = WebSocketStates.Closed;
                if (this.Parent.OnClosed != null)
                    this.Parent.OnClosed(this.Parent, (ushort)WebSocketStausCodes.NoStatusCode, string.Empty);
            }
            else
            {
                this.State = WebSocketStates.Closing;
                webSocket.Close(code, message);
            }
        }

        public override void StartOpen()
        {
            if (requestSent)
                throw new InvalidOperationException("Open already called! You can't reuse this WebSocket instance!");

            if (this.Parent.Extensions != null)
            {
                try
                {
                    for (int i = 0; i < this.Parent.Extensions.Length; ++i)
                    {
                        var ext = this.Parent.Extensions[i];
                        if (ext != null)
                            ext.AddNegotiation(InternalRequest);
                    }
                }
                catch (Exception ex)
                {
                    HTTPManager.Logger.Exception("OverHTTP1", "Open", ex, this.Parent.Context);
                }
            }

            InternalRequest.Send();
            requestSent = true;
            this.State = WebSocketStates.Connecting;
        }

        private void OnInternalRequestCallback(HTTPRequest req, HTTPResponse resp)
        {
            string reason = string.Empty;

            switch (req.State)
            {
                case HTTPRequestStates.Finished:
                    HTTPManager.Logger.Information("OverHTTP1", string.Format("Request finished. Status Code: {0} Message: {1}", resp.StatusCode.ToString(), resp.Message), this.Parent.Context);

                    if (resp.StatusCode == 101)
                    {
                        // The request upgraded successfully.
                        return;
                    }
                    else
                        reason = string.Format("Request Finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}",
                                                        resp.StatusCode,
                                                        resp.Message,
                                                        resp.DataAsText);
                    break;

                // The request finished with an unexpected error. The request's Exception property may contain more info about the error.
                case HTTPRequestStates.Error:
                    reason = "Request Finished with Error! " + (req.Exception != null ? ("Exception: " + req.Exception.Message + req.Exception.StackTrace) : string.Empty);
                    break;

                // The request aborted, initiated by the user.
                case HTTPRequestStates.Aborted:
                    reason = "Request Aborted!";
                    break;

                // Connecting to the server is timed out.
                case HTTPRequestStates.ConnectionTimedOut:
                    reason = "Connection Timed Out!";
                    break;

                // The request didn't finished in the given time.
                case HTTPRequestStates.TimedOut:
                    reason = "Processing the request Timed Out!";
                    break;

                default:
                    return;
            }

            if (this.State != WebSocketStates.Connecting || !string.IsNullOrEmpty(reason))
            {
                if (this.Parent.OnError != null)
                    this.Parent.OnError(this.Parent, reason);
                else if (!HTTPManager.IsQuitting)
                    HTTPManager.Logger.Error("OverHTTP1", reason, this.Parent.Context);
            }
            else if (this.Parent.OnClosed != null)
                this.Parent.OnClosed(this.Parent, (ushort)WebSocketStausCodes.NormalClosure, "Closed while opening");

            this.State = WebSocketStates.Closed;

            if (!req.IsKeepAlive && resp != null && resp is WebSocketResponse)
                (resp as WebSocketResponse).CloseStream();
        }

        private void OnInternalRequestUpgraded(HTTPRequest req, HTTPResponse resp)
        {
            HTTPManager.Logger.Information("OverHTTP1", "Internal request upgraded!", this.Parent.Context);

            webSocket = resp as WebSocketResponse;

            if (webSocket == null)
            {
                if (this.Parent.OnError != null)
                {
                    string reason = string.Empty;
                    if (req.Exception != null)
                        reason = req.Exception.Message + " " + req.Exception.StackTrace;

                    this.Parent.OnError(this.Parent, reason);
                }

                this.State = WebSocketStates.Closed;
                return;
            }

            // If Close called while we connected
            if (this.State == WebSocketStates.Closed)
            {
                webSocket.CloseStream();
                return;
            }

            if (!resp.HasHeader("sec-websocket-accept"))
            {
                this.State = WebSocketStates.Closed;
                webSocket.CloseStream();

                if (this.Parent.OnError != null)
                    this.Parent.OnError(this.Parent, "No Sec-Websocket-Accept header is sent by the server!");
                return;
            }

            webSocket.WebSocket = this.Parent;

            if (this.Parent.Extensions != null)
            {
                for (int i = 0; i < this.Parent.Extensions.Length; ++i)
                {
                    var ext = this.Parent.Extensions[i];

                    try
                    {
                        if (ext != null && !ext.ParseNegotiation(webSocket))
                            this.Parent.Extensions[i] = null; // Keep extensions only that successfully negotiated
                    }
                    catch (Exception ex)
                    {
                        HTTPManager.Logger.Exception("OverHTTP1", "ParseNegotiation", ex, this.Parent.Context);

                        // Do not try to use a defective extension in the future
                        this.Parent.Extensions[i] = null;
                    }
                }
            }

            this.State = WebSocketStates.Open;
            if (this.Parent.OnOpen != null)
            {
                try
                {
                    this.Parent.OnOpen(this.Parent);
                }
                catch (Exception ex)
                {
                    HTTPManager.Logger.Exception("OverHTTP1", "OnOpen", ex, this.Parent.Context);
                }
            }

            webSocket.OnText = (ws, msg) =>
            {
                if (this.Parent.OnMessage != null)
                    this.Parent.OnMessage(this.Parent, msg);
            };

            webSocket.OnBinary = (ws, bin) =>
            {
                if (this.Parent.OnBinary != null)
                    this.Parent.OnBinary(this.Parent, bin);
            };

            webSocket.OnClosed = (ws, code, msg) =>
            {
                this.State = WebSocketStates.Closed;

                if (this.Parent.OnClosed != null)
                    this.Parent.OnClosed(this.Parent, code, msg);
            };

            if (this.Parent.OnIncompleteFrame != null)
                webSocket.OnIncompleteFrame = (ws, frame) =>
                {
                    if (this.Parent.OnIncompleteFrame != null)
                        this.Parent.OnIncompleteFrame(this.Parent, frame);
                };

            if (this.Parent.StartPingThread)
                webSocket.StartPinging(Math.Max(this.Parent.PingFrequency, 100));

            webSocket.StartReceive();
        }

        public override void Send(string message)
        {
            webSocket.Send(message);
        }

        public override void Send(byte[] buffer)
        {
            webSocket.Send(buffer);
        }

        public override void Send(byte[] buffer, ulong offset, ulong count)
        {
            webSocket.Send(buffer, offset, count);
        }

        public override void Send(WebSocketFrame frame)
        {
            webSocket.Send(frame);
        }
    }
}
#endif
