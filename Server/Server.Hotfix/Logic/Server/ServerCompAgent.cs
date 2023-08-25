using Server.App.Logic.Login.Entity;
using Server.App.Logic.Server;
using Server.Core.Actors;
using Server.Core.Hotfix.Agent;
using Server.Core.Timer.Handler;
using Server.DBServer;
using Server.Hotfix.Logic.Role.Base;
using Server.Utility;

namespace Server.Hotfix.Logic.Server
{
    public class ServerCompAgent : StateCompAgent<ServerComp, ServerState>
    {
        readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        class DelayTimer : TimerHandler<ServerCompAgent>
        {
            protected override Task HandleTimer(ServerCompAgent agent, Param param)
            {
                return agent.TestDelayTimer();
            }
        }

        class ScheduleTimer : TimerHandler<ServerCompAgent>
        {
            protected override Task HandleTimer(ServerCompAgent agent, Param param)
            {
                return agent.TestScheduleTimer();
            }
        }

        public override void Active()
        {
            Delay<DelayTimer>(TimeSpan.FromSeconds(3));
            Schedule<ScheduleTimer>(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5));
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

        public static async Task OnlineRoleForeach(Action<RoleCompAgent> func)
        {
            var serverComp = await ActorMgr.GetCompAgent<ServerCompAgent>();
            serverComp.Tell(async () =>
            {
                foreach (var roleId in serverComp.Comp.OnlineSet)
                {
                    var roleComp = await ActorMgr.GetCompAgent<RoleCompAgent>(roleId);
                    roleComp.Tell(() => func(roleComp));
                }
            });
        }

        private Task TestDelayTimer()
        {
            LOGGER.Debug("ServerCompAgent.TestDelayTimer.延时3秒执行.执行一次");
            return Task.CompletedTask;
        }

        private Task TestScheduleTimer()
        {
            LOGGER.Debug("ServerCompAgent.TestSchedueTimer.延时1秒执行.每隔10秒执行");
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