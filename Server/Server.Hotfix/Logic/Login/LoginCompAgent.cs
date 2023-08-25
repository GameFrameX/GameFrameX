using Server.App.Common;
using Server.App.Logic.Login.Component;
using Server.App.Logic.Login.Entity;


namespace Server.Hotfix.Logic.Login
{
    public class LoginCompAgent : StateCompAgent<LoginComponent, LoginState>
    {
        private static readonly NLog.Logger Log = LogManager.GetCurrentClassLogger();

        public async Task OnLogin(NetChannel channel, ReqLogin reqLogin)
        {
            if (string.IsNullOrEmpty(reqLogin.UserName))
            {
                channel.WriteAsync(null, reqLogin.UniId, StateCode.AccountCannotBeNull);
                return;
            }

            var loginCompAgent = await ActorMgr.GetCompAgent<LoginCompAgent>();
            var loginState = await loginCompAgent.Comp.OnLogin(reqLogin);
            if (loginState == null)
            {
                var accountId = IdGenerator.GetActorID(ActorType.Account);
                loginState = await loginCompAgent.Comp.Register(accountId, reqLogin);
            }

            RespLogin respLogin = new RespLogin();
            respLogin.Code = loginState.State;
            respLogin.UserInfo = new UserInfo
            {
                CreateTime = loginState.CreateTime
            };
            channel.WriteAsync(respLogin, reqLogin.UniId);
            //查询角色账号，这里设定每个服务器只能有一个角色
            /*var roleId = GetRoleIdOfPlayer(reqLogin.UserName, reqLogin.Password, reqLogin.SdkType);
            var isNewRole = roleId <= 0;
            if (isNewRole)
            {
                //没有老角色，创建新号
                roleId = IdGenerator.GetActorID(ActorType.Account);
                CreateRoleToPlayer(reqLogin.UserName, reqLogin.Password, reqLogin.SdkType, roleId);
                Log.Info("创建新号:" + roleId);
            }

            //添加到session
            var session = new Session(roleId)
            {
                Channel = channel,
                Sign = reqLogin.Device
            };
            SessionManager.Add(session);

            //登陆流程
            var loginCompAgent = await ActorMgr.GetCompAgent<LoginCompAgent>(roleId);

            var actorId = loginCompAgent.ActorId;*/
            // var roleComp = await ActorMgr.GetCompAgent<RoleCompAgent>(roleId);
            // //从登录线程-->调用Role线程 所以需要入队
            // var resLogin = await roleComp.OnLogin(reqLogin, isNewRole);
            // channel.WriteAsync(resLogin, reqLogin.UniId, StateCode.Success);
            //
            // //加入在线玩家
            // var serverComp = await ActorMgr.GetCompAgent<ServerCompAgent>();
            // await serverComp.AddOnlineRole(ActorId);
        }
    }
}