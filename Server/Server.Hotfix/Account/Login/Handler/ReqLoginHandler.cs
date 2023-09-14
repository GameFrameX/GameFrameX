using Server.Hotfix.Account.Login.Agent;

namespace Server.Hotfix.Account.Login.Handler
{
    [MsgMapping(typeof(ReqLogin))]
    internal class ReqLoginHandler : GlobalComponentHandler<LoginComponentAgent>
    {
        public override async Task ActionAsync()
        {
            await Comp.OnLogin(Channel, Msg as ReqLogin);
        }
    }
}