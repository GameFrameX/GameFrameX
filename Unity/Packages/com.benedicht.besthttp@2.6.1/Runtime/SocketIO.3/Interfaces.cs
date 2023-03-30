#if !BESTHTTP_DISABLE_SOCKETIO

namespace BestHTTP.SocketIO3
{
    using BestHTTP.Logger;
    using BestHTTP.PlatformSupport.Memory;
    using BestHTTP.SocketIO3.Transports;

    /// <summary>
    /// Interface to hide internal functions from the user by implementing it as an explicit interface.
    /// </summary>
    public interface IManager
    {
        LoggingContext Context { get; }
        void Remove(Socket socket);
        void Close(bool removeSockets = true);
        void TryToReconnect();
        bool OnTransportConnected(ITransport transport);
        void OnTransportError(ITransport trans, string err);
        void OnTransportProbed(ITransport trans);
        void SendPacket(OutgoingPacket packet);
        void OnPacket(IncomingPacket packet);
        void EmitEvent(string eventName, params object[] args);
        void EmitEvent(SocketIOEventTypes type, params object[] args);
        void EmitError(string msg);
        void EmitAll(string eventName, params object[] args);
    }

    /// <summary>
    /// Interface to hide internal functions from the user by implementing it as an explicit interface.
    /// </summary>
    public interface ISocket
    {
        LoggingContext Context { get; }
        void Open();
        void Disconnect(bool remove);
        void OnPacket(IncomingPacket packet);
        void EmitEvent(SocketIOEventTypes type, params object[] args);
        void EmitEvent(string eventName, params object[] args);
        void EmitError(string msg);
    }
}

#endif
