using Server.Apps.Account.Login.Component;
using Server.Apps.Account.Login.Entity;
using Server.Launcher.Common;
using Server.Core.Net;
using Server.Hotfix.Common;
using Server.NetWork;

namespace Server.Hotfix.Account.Login.Agent
{
    public class LoginComponentAgent : StateComponentAgent<LoginComponent, LoginState>
    {
        public async Task OnLogin(INetChannel channel, ReqLogin reqLogin)
        {
            if (string.IsNullOrEmpty(reqLogin.UserName))
            {
                channel.WriteAsync(null, reqLogin.UniId, (int)OperationStatusCode.AccountCannotBeNull);
                return;
            }

            var loginCompAgent = await ActorManager.GetComponentAgent<LoginComponentAgent>();
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
                CreateTime = loginState.CreateTime,
                Level = Utility.Random.GetRandom(1, 100),
                RoleId = loginState.Id,
                RoleName = Utility.Random.GetRandom(1, 100).ToString(),
                VipLevel = Utility.Random.GetRandom(1, 100),
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