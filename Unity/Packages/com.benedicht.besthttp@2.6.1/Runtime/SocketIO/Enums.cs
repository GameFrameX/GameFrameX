#if !BESTHTTP_DISABLE_SOCKETIO

namespace BestHTTP.SocketIO
{
    /// <summary>
    /// Possible event types on the transport level.
    /// </summary>
    public enum TransportEventTypes : int
    {
        Unknown = -1,
        Open = 0,
        Close = 1,
        Ping = 2,
        Pong = 3,
        Message = 4,
        Upgrade = 5,
        Noop = 6
    }

    /// <summary>
    /// Event types of the SocketIO protocol.
    /// </summary>
    public enum SocketIOEventTypes : int
    {
        Unknown = -1,

        /// <summary>
        /// Connect to a namespace, or we connected to a namespace
        /// </summary>
        Connect = 0,

        /// <summary>
        /// Disconnect a namespace, or we disconnected from a namespace.
        /// </summary>
        Disconnect = 1,

        /// <summary>
        /// A general event. The event's name is in the payload.
        /// </summary>
        Event = 2,

        /// <summary>
        /// Acknowledgment of an event.
        /// </summary>
        Ack = 3,

        /// <summary>
        /// Error sent by the server, or by the plugin
        /// </summary>
        Error = 4,

        /// <summary>
        /// A general event with binary attached to the packet. The event's name is in the payload.
        /// </summary>
        BinaryEvent = 5,

        /// <summary>
        /// Acknowledgment of a binary event.
        /// </summary>
        BinaryAck = 6
    }

    /// <summary>
    /// Possible error codes that the SocketIO server can send.
    /// </summary>
    public enum SocketIOErrors
    {
        /// <summary>
        /// Transport unknown
        /// </summary>
        UnknownTransport = 0,

        /// <summary>
        /// Session ID unknown
        /// </summary>
        UnknownSid = 1,

        /// <summary>
        /// Bad handshake method
        /// </summary>
        BadHandshakeMethod = 2,

        /// <summary>
        /// Bad request
        /// </summary>
        BadRequest = 3,

        /// <summary>
        /// Tried to access a forbidden resource
        /// </summary>
        Forbidden = 4,

        /// <summary>
        /// Plugin internal error!
        /// </summary>
        Internal = 5,

        /// <summary>
        /// Exceptions that caught by the plugin but raised in a user code.
        /// </summary>
        User = 6,

        /// <summary>
        /// A custom, server sent error, most probably from a Socket.IO middleware.
        /// </summary>
        Custom = 7,
    }
}

#endif
