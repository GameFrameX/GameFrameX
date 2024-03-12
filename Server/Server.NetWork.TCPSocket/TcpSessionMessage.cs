using System.Buffers;
using System.Text;
using Server.Extension;
using Server.Log;
using Server.NetWork.TCPSocket.Base;
using Server.Serialize.Serialize;

namespace Server.NetWork.TCPSocket;

public class TcpSessionMessage : TcpSession
{
    public IMessageDecoderHandler MessageDecoderHandler { get; }

    public TcpSessionMessage(TcpServerMessage server, IMessageDecoderHandler messageDecoderHandler) : base(server)
    {
        MessageDecoderHandler = messageDecoderHandler;
    }

    protected override void OnConnected()
    {
        string message = "Hello from TCP chat! Please send a message or '!' to disconnect the client!";
        SendAsync(message);
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        var message = buffer.AsSpan((int)offset, (int)size);
        var messageObject = MessageDecoderHandler.Handler(message);
        this.Server.UpdateReceiveMessageTime();
    }
}