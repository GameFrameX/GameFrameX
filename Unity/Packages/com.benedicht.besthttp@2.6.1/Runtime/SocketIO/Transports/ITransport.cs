#if !BESTHTTP_DISABLE_SOCKETIO

using System.Collections.Generic;

namespace BestHTTP.SocketIO.Transports
{
    public enum TransportTypes
    {
        Polling,

#if !BESTHTTP_DISABLE_WEBSOCKET
        WebSocket
#endif
    }

    /// <summary>
    /// Possible states of an ITransport implementation.
    /// </summary>
    public enum TransportStates : int
    {
        /// <summary>
        /// The transport is connecting to the server.
        /// </summary>
        Connecting = 0,

        /// <summary>
        /// The transport is connected, and started the opening process.
        /// </summary>
        Opening = 1,

        /// <summary>
        /// The transport is open, can send and receive packets.
        /// </summary>
        Open = 2,

        /// <summary>
        /// The transport is closed.
        /// </summary>
        Closed = 3,

        /// <summary>
        /// The transport is paused.
        /// </summary>
        Paused = 4
    }

    /// <summary>
    /// An interface that a Socket.IO transport must implement.
    /// </summary>
    public interface ITransport
    {
        /// <summary>
        /// Type of this transport.
        /// </summary>
        TransportTypes Type { get; }

        /// <summary>
        /// Current state of the transport
        /// </summary>
        TransportStates State { get; }

        /// <summary>
        /// SocketManager instance that this transport is bound to.
        /// </summary>
        SocketManager Manager { get; }

        /// <summary>
        /// True if the transport is busy with sending messages.
        /// </summary>
        bool IsRequestInProgress { get; }

        /// <summary>
        /// True if the transport is busy with a poll request.
        /// </summary>
        bool IsPollingInProgress { get; }

        /// <summary>
        /// Start open/upgrade the transport.
        /// </summary>
        void Open();

        /// <summary>
        /// Do a poll for available messages on the server.
        /// </summary>
        void Poll();

        /// <summary>
        /// Send a single packet to the server.
        /// </summary>
        void Send(Packet packet);

        /// <summary>
        /// Send a list of packets to the server.
        /// </summary>
        void Send(List<Packet> packets);

        /// <summary>
        /// Close this transport.
        /// </summary>
        void Close();
    }
}

#endif