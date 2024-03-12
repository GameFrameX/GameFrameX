using System.Net.Sockets;
using System.Text;
using Server.Log;

namespace Server.NetWork.TCPSocket;

public class TcpClientMessage : Server.NetWork.TCPSocket.Base.TcpClient
{
    public INetWorkChannelHelper? NetWorkChannelHelper { get; set; }

    public TcpClientMessage(string address, int port) : base(address, port)
    {
    }

    public void DisconnectAndStop()
    {
        stop = true;
        DisconnectAsync();
        while (IsConnected)
            Thread.Yield();
    }

    protected override void OnConnected()
    {
        LogHelper.Info($"TCP client connected a new session with Id {Id}");
        NetWorkChannelHelper?.OnConnected?.Invoke();
    }

    protected override void OnDisconnected()
    {
        LogHelper.Info($"TCP client disconnected a session with Id {Id}");
        NetWorkChannelHelper?.OnDisconnected?.Invoke();

        // Wait for a while...
        Thread.Sleep(1000);

        // Try to connect again
        if (!stop)
            ConnectAsync();
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        LogHelper.Info(Encoding.UTF8.GetString(buffer, (int)offset, (int)size));
        NetWorkChannelHelper?.OnReceiveMessage?.Invoke(null, buffer, offset, size);
    }

    protected override void OnError(SocketError error)
    {
        LogHelper.Info($"Chat TCP client caught an error with code {error}");
        NetWorkChannelHelper?.OnError?.Invoke(error.ToString());
    }

    private bool stop;
}