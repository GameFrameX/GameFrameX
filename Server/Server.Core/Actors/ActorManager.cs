using System.Collections.Concurrent;
using Server.Core.Actors.Impl;
using Server.Core.Comps;
using Server.Core.Hotfix;
using Server.Core.Hotfix.Agent;
using Server.Core.Timer;
using Server.Core.Utility;
using Server.Log;

namespace Server.Core.Actors
{
    /// <summary>
    /// Actor管理器
    /// </summary>
    public static class ActorManager
    {

        private static readonly ConcurrentDictionary<long, Actor> ActorMap = new ConcurrentDictionary<long, Actor>();

        /// <summary>
        /// 根据ActorId获取对应的IComponentAgent对象
        /// </summary>
        /// <param name="actorId">ActorId</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<T> GetComponentAgent<T>(long actorId) where T : IComponentAgent
        {
            var actor = await GetOrNew(actorId);
            return await actor.GetComponentAgent<T>();
        }

        /// <summary>
        /// 是否存在指定的Actor
        /// </summary>
        /// <param name="actorId">ActorId</param>
        /// <returns></returns>
        public static bool HasActor(long actorId)
        {
            return ActorMap.ContainsKey(actorId);
        }

        /// <summary>
        /// 根据ActorId获取对应的Actor
        /// </summary>
        /// <param name="actorId">ActorId</param>
        /// <returns></returns>
        internal static Actor GetActor(long actorId)
        {
            ActorMap.TryGetValue(actorId, out var actor);
            return actor;
        }

        /// <summary>
        /// 根据ActorId和组件类型获取对应的IComponentAgent数据
        /// </summary>
        /// <param name="actorId">ActorId</param>
        /// <param name="agentType">组件类型</param>
        /// <returns></returns>
        internal static async Task<IComponentAgent> GetComponentAgent(long actorId, Type agentType)
        {
            var actor = await GetOrNew(actorId);
            return await actor.GetComponentAgent(agentType);
        }

        /// <summary>
        /// 根据组件类型获取对应的IComponentAgent数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Task<T> GetComponentAgent<T>() where T : IComponentAgent
        {
            var compType = HotfixMgr.GetCompType(typeof(T));
            var actorType = ComponentRegister.GetActorType(compType);
            var actorId = IdGenerator.GetActorID(actorType);
            return GetComponentAgent<T>(actorId);
        }

        /// <summary>
        /// 根据actorId获取对应的actor实例，不存在则新生成一个Actor对象
        /// </summary>
        /// <param name="actorId">actorId</param>
        /// <returns></returns>
        internal static async Task<Actor> GetOrNew(long actorId)
        {
            var actorType = IdGenerator.GetActorType(actorId);
            if (actorType == ActorType.Player)
            {
                var now = DateTime.Now;
                if (activeTimeDic.TryGetValue(actorId, out var activeTime)
                    && (now - activeTime).TotalMinutes < 10
                    && ActorMap.TryGetValue(actorId, out var actor))
                {
                    activeTimeDic[actorId] = now;
                    return actor;
                }
                else
                {
                    return await GetLifeActor(actorId).SendAsync(() =>
                    {
                        activeTimeDic[actorId] = now;
                        return ActorMap.GetOrAdd(actorId, k => new Actor(k, IdGenerator.GetActorType(k)));
                    });
                }
            }
            else
            {
                return ActorMap.GetOrAdd(actorId, k => new Actor(k, IdGenerator.GetActorType(k)));
            }
        }

        /// <summary>
        /// 全部完成
        /// </summary>
        /// <returns></returns>
        public static Task AllFinish()
        {
            var tasks = new List<Task>();
            foreach (var actor in ActorMap.Values)
            {
                tasks.Add(actor.SendAsync(() => true));
            }

            return Task.WhenAll(tasks);
        }

        private static readonly ConcurrentDictionary<long, DateTime> activeTimeDic = new();

        private static readonly List<WorkerActor> workerActors = new();
        private const int workerCount = 10;

        static ActorManager()
        {
            for (int i = 0; i < workerCount; i++)
            {
                workerActors.Add(new WorkerActor());
            }
        }

        /// <summary>
        /// 根据ActorId 获取玩家
        /// </summary>
        /// <param name="actorId"></param>
        /// <returns></returns>
        private static WorkerActor GetLifeActor(long actorId)
        {
            return workerActors[(int)(actorId % workerCount)];
        }

        /// <summary>
        /// 目前只回收玩家
        /// </summary>
        public static Task CheckIdle()
        {
            foreach (var actor in ActorMap.Values)
            {
                if (actor.AutoRecycle)
                {
                    actor.Tell(async () =>
                    {
                        if (actor.AutoRecycle
                            && (DateTime.Now - activeTimeDic[actor.Id]).TotalMinutes > 15)
                        {
                            await GetLifeActor(actor.Id).SendAsync(async () =>
                            {
                                if (activeTimeDic.TryGetValue(actor.Id, out var activeTime)
                                    && (DateTime.Now - activeTimeDic[actor.Id]).TotalMinutes > 15)
                                {
                                    // 防止定时回存失败时State被直接移除
                                    if (actor.ReadyToDeActive)
                                    {
                                        await actor.DeActive();
                                        ActorMap.TryRemove(actor.Id, out var _);
                                        LogHelper.Debug($"actor回收 id:{actor.Id} type:{actor.Type}");
                                    }
                                    else
                                    {
                                        // 不能存就久一点再判断
                                        activeTimeDic[actor.Id] = DateTime.Now;
                                    }
                                }

                                return true;
                            });
                        }
                    });
                }
            }

            return Task.CompletedTask;
        }


        /// <summary>
        /// 保存所有数据
        /// </summary>
        public static async Task SaveAll()
        {
            try
            {
                var begin = DateTime.Now;
                var taskList = new List<Task>();
                foreach (var actor in ActorMap.Values)
                {
                    taskList.Add(actor.SendAsync(async () => await actor.SaveAllState()));
                }

                await Task.WhenAll(taskList);
                LogHelper.Info($"save all state, use: {(DateTime.Now - begin).TotalMilliseconds}ms");
            }
            catch (Exception e)
            {
                LogHelper.Error($"save all state error \n{e}");
                throw;
            }
        }

        //public static readonly StatisticsTool statisticsTool = new();
        const int ONCE_SAVE_COUNT = 1000;

        /// <summary>
        ///  定时回存所有数据
        /// </summary>
        /// <returns></returns>
        public static async Task TimerSave()
        {
            try
            {
                int count = 0;
                var taskList = new List<Task>();
                foreach (var actor in ActorMap.Values)
                {
                    //如果定时回存的过程中关服了，直接终止定时回存，因为关服时会调用SaveAll以保证数据回存
                    if (!GlobalTimer.working)
                        return;
                    if (count < ONCE_SAVE_COUNT)
                    {
                        taskList.Add(actor.SendAsync(async () => await actor.SaveAllState()));
                        count++;
                    }
                    else
                    {
                        await Task.WhenAll(taskList);
                        await Task.Delay(1000);
                        taskList = new List<Task>();
                        count = 0;
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Info("timer save state error");
                LogHelper.Error(e.ToString());
            }
        }


        /// <summary>
        /// 角色跨天
        /// </summary>
        /// <param name="openServerDay">开服天数</param>
        /// <returns></returns>
        public static Task RoleCrossDay(int openServerDay)
        {
            foreach (var actor in ActorMap.Values)
            {
                if (actor.Type == ActorType.Player)
                {
                    actor.Tell(() => actor.CrossDay(openServerDay));
                }
            }

            return Task.CompletedTask;
        }

        const int CROSS_DAY_GLOBAL_WAIT_SECONDS = 60;
        const int CROSS_DAY_NOT_ROLE_WAIT_SECONDS = 120;

        /// <summary>
        /// 跨天
        /// </summary>
        /// <param name="openServerDay">开服天数</param>
        /// <param name="driverActorType">driverActorType</param>
        public static async Task CrossDay(int openServerDay, ActorType driverActorType)
        {
            // 驱动actor优先执行跨天
            var id = IdGenerator.GetActorID(driverActorType);
            var driverActor = ActorMap[id];
            await driverActor.CrossDay(openServerDay);

            var begin = DateTime.Now;
            int a = 0;
            int b = 0;
            foreach (var actor in ActorMap.Values)
            {
                if (actor.Type > ActorType.Separator && actor.Type != driverActorType)
                {
                    b++;
                    actor.Tell(async () =>
                    {
                        LogHelper.Info($"全局Actor：{actor.Type}执行跨天");
                        await actor.CrossDay(openServerDay);
                        Interlocked.Increment(ref a);
                    });
                }
            }

            while (a < b)
            {
                if ((DateTime.Now - begin).TotalSeconds > CROSS_DAY_GLOBAL_WAIT_SECONDS)
                {
                    LogHelper.Warn($"全局comp跨天耗时过久，不阻止其他comp跨天，当前已过{CROSS_DAY_GLOBAL_WAIT_SECONDS}秒");
                    break;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }

            var globalCost = (DateTime.Now - begin).TotalMilliseconds;
            LogHelper.Info($"全局comp跨天完成 耗时：{globalCost:f4}ms");
            a = 0;
            b = 0;
            foreach (var actor in ActorMap.Values)
            {
                if (actor.Type < ActorType.Separator && actor.Type != ActorType.Player)
                {
                    b++;
                    actor.Tell(async () =>
                    {
                        await actor.CrossDay(openServerDay);
                        Interlocked.Increment(ref a);
                    });
                }
            }

            while (a < b)
            {
                if ((DateTime.Now - begin).TotalSeconds > CROSS_DAY_NOT_ROLE_WAIT_SECONDS)
                {
                    LogHelper.Warn($"非玩家comp跨天耗时过久，不阻止玩家comp跨天，当前已过{CROSS_DAY_NOT_ROLE_WAIT_SECONDS}秒");
                    break;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }

            var otherCost = (DateTime.Now - begin).TotalMilliseconds - globalCost;
            LogHelper.Info($"非玩家comp跨天完成 耗时：{otherCost:f4}ms");
        }

        /// <summary>
        /// 删除所有actor
        /// </summary>
        public static async Task RemoveAll()
        {
            var tasks = new List<Task>();
            foreach (var actor in ActorMap.Values)
            {
                tasks.Add(actor.DeActive());
            }

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 删除actor
        /// </summary>
        /// <param name="actorId">actorId</param>
        /// <returns></returns>
        public static Task Remove(long actorId)
        {
            if (ActorMap.Remove(actorId, out var actor))
            {
                actor.Tell(actor.DeActive);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// 遍历所有actor
        /// </summary>
        /// <param name="func">遍历actor回调</param>
        /// <typeparam name="T"></typeparam>
        public static void ActorForEach<T>(Func<T, Task> func) where T : IComponentAgent
        {
            var agentType = typeof(T);
            var compType = HotfixMgr.GetCompType(agentType);
            var actorType = ComponentRegister.GetActorType(compType);
            foreach (var actor in ActorMap.Values)
            {
                if (actor.Type == actorType)
                {
                    actor.Tell(async () =>
                    {
                        var comp = await actor.GetComponentAgent<T>();
                        await func(comp);
                    });
                }
            }
        }

        /// <summary>
        /// 遍历所有actor
        /// </summary>
        /// <param name="action">遍历actor回调</param>
        /// <typeparam name="T"></typeparam>
        public static void ActorForEach<T>(Action<T> action) where T : IComponentAgent
        {
            var agentType = typeof(T);
            var compType = HotfixMgr.GetCompType(agentType);
            var actorType = ComponentRegister.GetActorType(compType);
            foreach (var actor in ActorMap.Values)
            {
                if (actor.Type == actorType)
                {
                    actor.Tell(async () =>
                    {
                        var comp = await actor.GetComponentAgent<T>();
                        action(comp);
                    });
                }
            }
        }

        /// <summary>
        /// 清除所有agent
        /// </summary>
        public static void ClearAgent()
        {
            foreach (var actor in ActorMap.Values)
            {
                actor.Tell(actor.ClearAgent);
            }
        }
    }
}