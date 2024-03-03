using System.Net;
using System.Net.Sockets;
using Server.NetWork.UDPSocket.Base;

namespace Server.NetWork.UDPSocket;

public class MulticastUdpServer : UdpServer
{
    public MulticastUdpServer(IPAddress address, int port) : base(address, port)
    {
    }

    protected override void OnError(SocketError error)
    {
        Console.WriteLine($"Multicast UDP server caught an error with code {error}");
    }
}