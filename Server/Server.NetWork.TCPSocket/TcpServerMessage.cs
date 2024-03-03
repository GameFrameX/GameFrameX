using System.Net;
using Server.NetWork.TCPSocket.Base;

namespace Server.NetWork.TCPSocket;

public class TcpServerMessage : TcpServer
{
    public IMessageDecoderHandler MessageDecoderHandler { get; set; }
    public TcpServerMessage(string address, int port) : base(address, port)
    {
    }

    public TcpServerMessage(IPAddress address, int port) : base(address, port)
    {
    }

    protected override TcpSessionMessage CreateSession()
    {
        return new TcpSessionMessage(this,MessageDecoderHandler);
    }
    
}