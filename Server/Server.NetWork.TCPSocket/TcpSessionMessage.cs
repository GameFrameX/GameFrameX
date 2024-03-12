using System.Buffers;
using System.Net.Sockets;
using System.Text;
using Server.Extension;
using Server.Log;
using Server.NetWork.TCPSocket.Base;
using Server.Serialize.Serialize;

namespace Server.NetWork.TCPSocket;

public class TcpSessionMessage : TcpSession
{
    public INetWorkChannelHelper? NetWorkChannelHelper { get; }

    public TcpSessionMessage(TcpServerMessage server, INetWorkChannelHelper? netWorkChannelHelper) : base(server)
    {
        NetWorkChannelHelper = netWorkChannelHelper;
    }

    protected override void OnConnected()
    {
        string message = "Hello from TCP chat! Please send a message or '!' to disconnect the client!";
        SendAsync(message);
        NetWorkChannelHelper?.OnConnected?.Invoke();
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        NetWorkChannelHelper?.OnReceiveMessage?.Invoke(this, buffer, offset, size);
        this.Server.UpdateReceiveMessageTime();
    }

    public override bool Disconnect()
    {
        NetWorkChannelHelper?.OnDisconnected?.Invoke();
        return base.Disconnect();
    }

    protected override void OnError(SocketError error)
    {
        base.OnError(error);
        NetWorkChannelHelper?.OnError?.Invoke(error.ToString());
    }
}