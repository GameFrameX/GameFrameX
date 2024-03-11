using System.Net;
using System.Net.Sockets;
using System.Text;
using Server.Log;
using UdpClient = Server.NetWork.UDPSocket.Base.UdpClient;

namespace Server.NetWork.UDPSocket;

class MulticastUdpClient : UdpClient
{
    public string Multicast;

    public MulticastUdpClient(string address, int port) : base(address, port)
    {
    }

    public void DisconnectAndStop()
    {
        stop = true;
        Disconnect();
        while (IsConnected)
            Thread.Yield();
    }

    protected override void OnConnected()
    {
        LogHelper.Info($"Multicast UDP client connected a new session with Id {Id}");

        // Join UDP multicast group
        JoinMulticastGroup(Multicast);

        // Start receive datagrams
        ReceiveAsync();
    }

    protected override void OnDisconnected()
    {
        LogHelper.Info($"Multicast UDP client disconnected a session with Id {Id}");

        // Wait for a while...
        Thread.Sleep(1000);

        // Try to connect again
        if (!stop)
            Connect();
    }

    protected override void OnReceived(EndPoint endpoint, byte[] buffer, long offset, long size)
    {
        LogHelper.Info("Incoming: " + Encoding.UTF8.GetString(buffer, (int)offset, (int)size));

        // Continue receive datagrams
        ReceiveAsync();
    }

    protected override void OnError(SocketError error)
    {
        LogHelper.Info($"Multicast UDP client caught an error with code {error}");
    }

    private bool stop;
}