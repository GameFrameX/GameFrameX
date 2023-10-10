using Server.Core.Net;
using Server.Core.Net.Tcp;
using Server.Launcher.Common.Session;
using Server.Setting;

namespace Server.Launcher.Common.Net
{
    public class AppTcpConnectionHandler : TcpConnectionHandler
    {
        protected override void OnDisconnection(NetChannel channel)
        {
            base.OnDisconnection(channel);
            var sessionId = channel.GetData<long>(GlobalConst.SESSION_ID_KEY);
            if (sessionId > 0)
            {
                SessionManager.Remove(sessionId);
            }
        }
    }
}