using Server.Apps.Player.Player.Component;
using Server.Hotfix.Common;
using Server.Launcher.Common;
using Server.Launcher.Common.Session;
using Server.Hotfix.Player.Role.Bag.Agent;
using Server.Hotfix.Server.Server.Agent;
using Server.NetWork.Messages;

namespace Server.Hotfix.Player.Role.Role.Agent
{
    public static class RoleCompAgentExtension
    {
        public static async Task NotifyClient(this IComponentAgent agent, MessageObject msg, int uniId = 0, OperationStatusCode code = OperationStatusCode.Success)
        {
            var roleComp = await agent.GetComponentAgent<RoleComponentAgent>();
            if (roleComp != null)
            {
                roleComp.NotifyClient(msg, uniId, code);
            }
            else
            {
                LogHelper.Warn($"{agent.OwnerType}未注册RoleComp组件");
            }
        }
    }

    public class RoleComponentAgent : StateComponentAgent<PlayerComponent, PlayerState>, ICrossDay
    {
        [Event(EventId.SessionRemove)]
        private class EL : EventListener<RoleComponentAgent>
        {
            protected override Task HandleEvent(RoleComponentAgent agent, Event evt)
            {
                return agent.OnLogout();
            }
        }

        public async Task<RespLogin> OnLogin(ReqLogin reqLogin, bool isNewRole)
        {
            SetAutoRecycle(false);
            if (isNewRole)
            {
                State.CreateTime = TimeHelper.UnixTimeSeconds();
                State.Level = 1;
                State.VipLevel = 1;
                State.RoleName = new System.Random().Next(1000, 10000).ToString(); //随机给一个
                //激活背包组件
                await GetComponentAgent<BagComponentAgent>();
            }

            // State.LoginTime = DateTime.Now;
            return BuildLoginMsg();
        }

        public async Task OnLogout()
        {
            //移除在线玩家
            var serverComp = await ActorManager.GetComponentAgent<ServerComponentAgent>();
            await serverComp.RemoveOnlineRole(ActorId);
            //下线后会被自动回收
            SetAutoRecycle(true);
            QuartzTimer.UnSchedule(ScheduleIdSet);
        }

        private RespLogin BuildLoginMsg()
        {
            var res = new RespLogin()
            {
                Code = 0,
                UserInfo = new UserInfo()
                {
                    CreateTime = State.CreateTime,
                    Level = State.Level,
                    RoleId = State.RoleId,
                    RoleName = State.RoleName,
                    VipLevel = State.VipLevel
                }
            };
            return res;
        }

        Task ICrossDay.OnCrossDay(int openServerDay)
        {
            return Task.CompletedTask;
        }

        public void NotifyClient(MessageObject msg, int uniId = 0, OperationStatusCode code = OperationStatusCode.Success)
        {
            var channel = SessionManager.GetChannel(ActorId);
            if (channel != null && !channel.IsClose())
            {
                channel.WriteAsync(msg, uniId, code);
            }
        }
    }
}