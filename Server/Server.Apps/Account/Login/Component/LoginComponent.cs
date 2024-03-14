using Server.Apps.Account.Login.Entity;
using Server.DBServer;

namespace Server.Apps.Account.Login.Component
{
    [ComponentType(ActorType.Account)]
    public sealed class LoginComponent : StateComponent<LoginState>
    {
        public async Task<LoginState> OnLogin(ReqLogin reqLogin)
        {
            return null;
            // return await GameDb.FindAsync<LoginState>(m => m.UserName == reqLogin.UserName && m.Password == reqLogin.Password);
        }

        public async Task<LoginState> Register(long accountId, ReqLogin reqLogin)
        {
            LoginState loginState = new LoginState() { Id = accountId, UserName = reqLogin.UserName, Password = reqLogin.Password };
            // await GameDb.SaveOneAsync<LoginState>(loginState);
            return loginState;
        }
    }
}