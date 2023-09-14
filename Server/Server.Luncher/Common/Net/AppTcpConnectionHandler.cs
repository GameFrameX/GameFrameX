using Server.Core.Net.Tcp.Codecs;
using Server.Core.Net.Tcp.Handler;
using Server.Luncher.Common.Session;

namespace Server.Luncher.Common.Net
{
    public class AppTcpConnectionHandler : TcpConnectionHandler
    {
        protected override void OnDisconnection(NetChannel channel)
        {
            base.OnDisconnection(channel);
            var sessionId = channel.GetSessionId();
            if (sessionId > 0)
            {
                SessionManager.Remove(sessionId);
            }
        }
    }
}