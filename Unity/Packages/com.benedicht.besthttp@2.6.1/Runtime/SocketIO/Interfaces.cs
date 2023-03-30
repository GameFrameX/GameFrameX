#if !BESTHTTP_DISABLE_SOCKETIO

namespace BestHTTP.SocketIO
{
    using BestHTTP.SocketIO.Transports;

    /// <summary>
    /// Interface to hide internal functions from the user by implementing it as an explicit interface.
    /// </summary>
    public interface IManager
    {
        void Remove(Socket socket);
        void Close(bool removeSockets = true);
        void TryToReconnect();
        bool OnTransportConnected(ITransport transport);
        void OnTransportError(ITransport trans, string err);
        void OnTransportProbed(ITransport trans);
        void SendPacket(Packet packet);
        void OnPacket(Packet packet);
        void EmitEvent(string eventName, params object[] args);
        void EmitEvent(SocketIOEventTypes type, params object[] args);
        void EmitError(SocketIOErrors errCode, string msg);
        void EmitAll(string eventName, params object[] args);
    }

    /// <summary>
    /// Interface to hide internal functions from the user by implementing it as an explicit interface.
    /// </summary>
    public interface ISocket
    {
        void Open();
        void Disconnect(bool remove);
        void OnPacket(Packet packet);
        void EmitEvent(SocketIOEventTypes type, params object[] args);
        void EmitEvent(string eventName, params object[] args);
        void EmitError(SocketIOErrors errCode, string msg);
    }
}

#endif