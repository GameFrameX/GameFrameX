/*using Server.Launcher.Common.Session;
using Server.NetWork;
using Server.NetWork.TCPSocket;
using Server.Setting;

namespace Server.Launcher.Common.Net
{
    public class AppTcpConnectionHandler : TcpConnectionHandler
    {
        protected override void OnDisconnection(BaseNetChannel channel)
        {
            base.OnDisconnection(channel);
            var sessionId = channel.GetData<long>(GlobalConst.SessionIdKey);
            if (sessionId > 0)
            {
                SessionManager.Remove(sessionId);
            }
        }

        // public AppTcpConnectionHandler(Func<int, IMessageHandler> messageHandler, Func<int, Type> typeGetter, Func<Type, int> idGetter) : base(messageHandler, typeGetter, idGetter)
        // {
        // }
    }
}*/