using Server.Apps.Server.Server.Component;
using Server.Apps.Server.Server.Entity;
using Server.Core.Timer.Handler;
using Server.Hotfix.Player.Role.Role.Agent;

namespace Server.Hotfix.Server.Server.Agent
{
    public class ServerComponentAgent : StateComponentAgent<ServerComponent, ServerState>
    {
        static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        class DelayTimer : TimerHandler<ServerComponentAgent>
        {
            protected override Task HandleTimer(ServerComponentAgent agent, Param param)
            {
                return agent.TestDelayTimer();
            }
        }

        class ScheduleTimer : TimerHandler<ServerComponentAgent>
        {
            protected override Task HandleTimer(ServerComponentAgent agent, Param param)
            {
                return agent.TestScheduleTimer();
            }
        }

        /// <summary>
        /// 跨天定时器调用
        /// </summary>
        class CrossDayTimeHandler : TimerHandler<ServerComponentAgent>
        {
            protected override async Task HandleTimer(ServerComponentAgent agent, Param param)
            {
                Logger.Debug($"ServerCompAgent.CrossDayTimeHandler.跨天定时器执行{TimeHelper.CurrentTimeWithFullString()}");
                await ActorMgr.RoleCrossDay(1);
                await ActorMgr.CrossDay(1, ActorType.Server);
            }
        }

        public override void Active()
        {
            // 跨天定时器
            WithCronExpression<CrossDayTimeHandler>("0 0 0 * * ? *");
        }


        [Service]
        [Discard]
        public virtual ValueTask AddOnlineRole(long actorId)
        {
            Comp.OnlineSet.Add(actorId);
            return ValueTask.CompletedTask;
        }

        [Service]
        [Discard]
        public virtual ValueTask RemoveOnlineRole(long actorId)
        {
            Comp.OnlineSet.Remove(actorId);
            return ValueTask.CompletedTask;
        }

        public static async Task OnlineRoleForeach(Action<RoleComponentAgent> func)
        {
            var serverComp = await ActorMgr.GetCompAgent<ServerComponentAgent>();
            serverComp.Tell(async () =>
            {
                foreach (var roleId in serverComp.Comp.OnlineSet)
                {
                    var roleComp = await ActorMgr.GetCompAgent<RoleComponentAgent>(roleId);
                    roleComp.Tell(() => func(roleComp));
                }
            });
        }

        private Task TestDelayTimer()
        {
            Logger.Debug("ServerCompAgent.TestDelayTimer.延时3秒执行.执行一次");
            return Task.CompletedTask;
        }

        private Task TestScheduleTimer()
        {
            Logger.Debug("ServerCompAgent.TestSchedueTimer.延时1秒执行.每隔10秒执行");
            //
            // var states = await GameDb.FindListAsync<LoginState>(m => m.Id != 0);
            // LOGGER.Debug(states);
            //
            // var s1 = await GameDb.CountAsync<LoginState>(m => m.Id == 563517512475926528);
            // LOGGER.Debug(s1);
            // var s = await GameDb.FindAsync<LoginState>(m => m.UserName != null);
            // LOGGER.Debug(s);
            // LoginState loginState = new LoginState()
            // {
            //     Id = TimeHelper.UnixTimeSeconds(), CreateId = TimeHelper.UnixTimeSeconds(),
            //     UserName = "Save"
            // };
            // loginState.AfterLoadFromDB(true);
            // var savedState = await GameDb.UpdateAsync<LoginState>(loginState);
            // LOGGER.Debug(savedState);
            // await Task.Delay(TimeSpan.FromSeconds(1));
            // var count = await GameDb.DeleteAsync(savedState);
            // LOGGER.Debug(count);
            return Task.CompletedTask;
        }

        [Service]
        [ThreadSafe]
        public virtual Task<int> GetWorldLevel()
        {
            return Task.FromResult(State.WorldLevel);
        }

        [Service]
        public virtual Task<bool> IsOnline(long roleId)
        {
            foreach (var id in Comp.OnlineSet)
            {
                if (id == roleId)
                    return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        /*******************演示代码**************************/
        [Service]
        [ThreadSafe]
        public virtual int DoSomething0()
        {
            return State.WorldLevel;
        }

        [Discard]
        [ThreadSafe]
        protected virtual Task DoSomething1()
        {
            return Task.CompletedTask;
        }

        [ThreadSafe]
        protected void DoSomething2()
        {
        }

        [Discard]
        [TimeOut(12000)]
        protected virtual Task DoSomething3()
        {
            return Task.CompletedTask;
        }
        /*******************演示代码**************************/
    }
}