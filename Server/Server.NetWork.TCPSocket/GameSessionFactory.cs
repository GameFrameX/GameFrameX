using SuperSocket;

namespace Server.NetWork.TCPSocket;

public class GameSessionFactory : ISessionFactory
{
    public Type SessionType { get; } = typeof(GameTcpSession);
    public INetWorkChannelHelper? NetWorkChannelHelper { get; set; }
    public IAppSession Create() => new GameTcpSession(NetWorkChannelHelper);
}