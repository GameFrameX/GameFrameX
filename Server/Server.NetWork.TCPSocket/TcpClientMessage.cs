using System.Net.Sockets;
using System.Text;
using Server.Log;
using TcpClient = Server.NetWork.TCPSocket.Base.TcpClient;

namespace Server.NetWork.TCPSocket;

public class TcpClientMessage : TcpClient
{
    public TcpClientMessage(string address, int port) : base(address, port)
    {
    }

    public void DisconnectAndStop()
    {
        _stop = true;
        DisconnectAsync();
        while (IsConnected)
            Thread.Yield();
    }

    protected override void OnConnected()
    {
        LogHelper.Info($"TCP client connected a new session with Id {Id}");
    }

    protected override void OnDisconnected()
    {
        LogHelper.Info($"TCP client disconnected a session with Id {Id}");

        // Wait for a while...
        Thread.Sleep(1000);

        // Try to connect again
        if (!_stop)
            ConnectAsync();
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        LogHelper.Info(Encoding.UTF8.GetString(buffer, (int)offset, (int)size));
    }

    protected override void OnError(SocketError error)
    {
        LogHelper.Info($"Chat TCP client caught an error with code {error}");
    }

    private bool _stop;
}