using Server.Core.Net.Tcp.Handler;
using Server.Proto;

namespace Server.Hotfix.Logic.Login
{
    [MsgMapping(typeof(ReqLogin))]
    internal class ReqLoginHandler : GlobalCompHandler<LoginCompAgent>
    {
        public override async Task ActionAsync()
        {
            await Comp.OnLogin(Channel, Msg as ReqLogin);
        }
    }
}