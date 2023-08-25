using Server.App.Logic.Login.Entity;
using Server.Core.Actors;
using Server.Core.Comps;
using Server.DBServer;
using Server.Proto.Proto;

namespace Server.App.Logic.Login.Component
{
    [ComponentType(ActorType.Account)]
    public sealed class LoginComponent : StateComp<LoginState>
    {
        public async Task<LoginState> OnLogin(ReqLogin reqLogin)
        {
            return await GameDb.FindAsync<LoginState>(m => m.UserName == reqLogin.UserName && m.Password == reqLogin.Password);
        }

        public async Task<LoginState> Register(long accountId, ReqLogin reqLogin)
        {
            LoginState loginState = new LoginState() {Id = accountId, UserName = reqLogin.UserName, Password = reqLogin.Password};
            await GameDb.SaveOneAsync<LoginState>(loginState);
            return loginState;
        }
    }
}