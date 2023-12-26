using Server.Core.Net.BaseHandler;
using Server.Hotfix.Account.Login.Agent;
using Server.NetWork.Messages;

namespace Server.Hotfix.Account.Login.Handler
{
    [MessageMapping(typeof(ReqLogin))]
    internal class ReqLoginHandler : GlobalComponentHandler<LoginComponentAgent>
    {
        protected override async Task ActionAsync()
        {
            await Comp.OnLogin(Channel, Message as ReqLogin);
        }
    }
}