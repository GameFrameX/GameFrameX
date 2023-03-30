#if !BESTHTTP_DISABLE_WEBSOCKET
using System;

#if !UNITY_WEBGL || UNITY_EDITOR
using BestHTTP.WebSocket.Frames;
#endif

namespace BestHTTP.WebSocket
{
    /// <summary>
    /// States of the underlying implementation's state.
    /// </summary>
    public enum WebSocketStates : byte
    {
        Connecting = 0,
        Open = 1,
        Closing = 2,
        Closed = 3,
        Unknown
    };

    public delegate void OnWebSocketOpenDelegate(WebSocket webSocket);
    public delegate void OnWebSocketMessageDelegate(WebSocket webSocket, string message);
    public delegate void OnWebSocketBinaryDelegate(WebSocket webSocket, byte[] data);
    public delegate void OnWebSocketClosedDelegate(WebSocket webSocket, UInt16 code, string message);
    public delegate void OnWebSocketErrorDelegate(WebSocket webSocket, string reason);

#if !UNITY_WEBGL || UNITY_EDITOR
    public delegate void OnWebSocketIncompleteFrameDelegate(WebSocket webSocket, WebSocketFrameReader frame);
#endif

    public abstract class WebSocketBaseImplementation
    {
        public virtual WebSocketStates State { get; protected set; }
        public virtual bool IsOpen { get; protected set; }
        public virtual int BufferedAmount { get; protected set; }

#if !UNITY_WEBGL || UNITY_EDITOR
        public HTTPRequest InternalRequest
        {
            get
            {
                if (this._internalRequest == null)
                    CreateInternalRequest();

                return this._internalRequest;
            }
        }
        protected HTTPRequest _internalRequest;

        public virtual int Latency { get; protected set; }
        public virtual DateTime LastMessageReceived { get; protected set; }
#endif

        public WebSocket Parent { get; }
        public Uri Uri { get; protected set; }
        public string Origin { get; }
        public string Protocol { get; }

        public WebSocketBaseImplementation(WebSocket parent, Uri uri, string origin, string protocol)
        {
            this.Parent = parent;
            this.Uri = uri;
            this.Origin = origin;
            this.Protocol = protocol;

#if !UNITY_WEBGL || UNITY_EDITOR
            this.LastMessageReceived = DateTime.MinValue;

            // Set up some default values.
            this.Parent.PingFrequency = 1000;
            this.Parent.CloseAfterNoMessage = TimeSpan.FromSeconds(2);
#endif
        }

        public abstract void StartOpen();
        public abstract void StartClose(UInt16 code, string message);

        public abstract void Send(string message);
        public abstract void Send(byte[] buffer);
        public abstract void Send(byte[] buffer, ulong offset, ulong count);

#if !UNITY_WEBGL || UNITY_EDITOR
        protected abstract void CreateInternalRequest();

        /// <summary>
        /// It will send the given frame to the server.
        /// </summary>
        public abstract void Send(WebSocketFrame frame);
#endif
    }
}
#endif
