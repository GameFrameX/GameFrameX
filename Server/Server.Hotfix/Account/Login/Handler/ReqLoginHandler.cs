using Server.Core.Net.BaseHandler;
using Server.Hotfix.Account.Login.Agent;

namespace Server.Hotfix.Account.Login.Handler
{
    [MsgMapping(typeof(ReqLogin))]
    internal class ReqLoginHandler : GlobalComponentHandler<LoginComponentAgent>
    {
        protected override async Task ActionAsync()
        {
            await Comp.OnLogin(Channel, Message as ReqLogin);
        }
    }
}