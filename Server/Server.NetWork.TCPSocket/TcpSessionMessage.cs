using SuperSocket.Channel;
using SuperSocket.Server;

namespace Server.NetWork.TCPSocket;

public class GameTcpSession : AppSession
{
    private INetWorkChannelHelper? NetWorkChannelHelper { get; }

    public GameTcpSession(INetWorkChannelHelper? netWorkChannelHelper)
    {
        NetWorkChannelHelper = netWorkChannelHelper;
    }

    protected override ValueTask OnSessionClosedAsync(CloseEventArgs e)
    {
        NetWorkChannelHelper?.OnDisconnected?.Invoke(e.Reason.ToString());
        return base.OnSessionClosedAsync(e);
    }

    protected override ValueTask OnSessionConnectedAsync()
    {
        NetWorkChannelHelper?.OnConnected?.Invoke();
        return base.OnSessionConnectedAsync();
    }
}