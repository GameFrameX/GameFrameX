using Server.App.Common;
using Server.App.Common.Session;
using Server.App.Logic.Role.Base.Component;
using Server.Hotfix.Logic.Role.Bag;

namespace Server.Hotfix.Logic.Role.Base
{
    public static class RoleCompAgentExt
    {
        private static readonly NLog.Logger LOGGER = LogManager.GetCurrentClassLogger();

        public static async Task NotifyClient(this ICompAgent agent, Message msg, int uniId = 0, StateCode code = StateCode.Success)
        {
            var roleComp = await agent.GetCompAgent<RoleCompAgent>();
            if (roleComp != null)
                roleComp.NotifyClient(msg, uniId, code);
            else
                LOGGER.Warn($"{agent.OwnerType}未注册RoleComp组件");
        }
    }

    public class RoleCompAgent : StateCompAgent<PlayerComponent, PlayerState>, ICrossDay
    {
        private static readonly NLog.Logger Log = LogManager.GetCurrentClassLogger();


        [Event(EventId.SessionRemove)]
        private class EL : EventListener<RoleCompAgent>
        {
            protected override Task HandleEvent(RoleCompAgent agent, Event evt)
            {
                return agent.OnLogout();
            }
        }

        public async Task<RespLogin> OnLogin(ReqLogin reqLogin, bool isNewRole)
        {
            SetAutoRecycle(false);
            if (isNewRole)
            {
                State.CreateTime = DateTime.Now;
                State.Level = 1;
                State.VipLevel = 1;
                State.RoleName = new System.Random().Next(1000, 10000).ToString(); //随机给一个
                //激活背包组件
                await GetCompAgent<BagCompAgent>();
            }

            State.LoginTime = DateTime.Now;
            return BuildLoginMsg();
        }

        public async Task OnLogout()
        {
            //移除在线玩家
            var serverComp = await ActorMgr.GetCompAgent<ServerCompAgent>();
            await serverComp.RemoveOnlineRole(ActorId);
            //下线后会被自动回收
            SetAutoRecycle(true);
            QuartzTimer.Unschedule(ScheduleIdSet);
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

        public void NotifyClient(Message msg, int uniId = 0, StateCode code = StateCode.Success)
        {
            var channel = SessionManager.GetChannel(ActorId);
            if (channel != null && !channel.IsClose())
            {
                channel.WriteAsync(msg, uniId, code);
            }
        }
    }
}