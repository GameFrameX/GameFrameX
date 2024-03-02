using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server.NetWork.UDPSocket;

public class EchoUdpServer : UdpServer
{
    public EchoUdpServer(IPAddress address, int port) : base(address, port)
    {
    }

    protected override void OnStarted()
    {
        // Start receive datagrams
        ReceiveAsync();
    }

    protected override void OnReceived(EndPoint endpoint, byte[] buffer, long offset, long size)
    {
        // Console.WriteLine("Incoming: " + Encoding.UTF8.GetString(buffer, (int)offset, (int)size));

        // Echo the message back to the sender
        SendAsync(endpoint, buffer, 0, size);
    }

    protected override void OnSent(EndPoint endpoint, long sent)
    {
        // Continue receive datagrams
        ReceiveAsync();
    }

    protected override void OnError(SocketError error)
    {
        Console.WriteLine($"Echo UDP server caught an error with code {error}");
    }
}