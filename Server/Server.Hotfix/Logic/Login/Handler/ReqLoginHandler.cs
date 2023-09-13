namespace Server.Hotfix.Logic.Login
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