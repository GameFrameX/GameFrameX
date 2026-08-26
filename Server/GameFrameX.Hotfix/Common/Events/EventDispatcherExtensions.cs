using GameFrameX.Apps.Common.Event;
using GameFrameX.Core.Abstractions.Agent;
using GameFrameX.Core.Abstractions.Events;
using GameFrameX.Foundation.Extensions;
using GameFrameX.Hotfix.Logic.Server;

namespace GameFrameX.Hotfix.Common.Events;

public static class EventDispatcherExtensions
{
    /// <summary>
    /// 分发事件
    /// </summary>
    /// <param name="agent">代理对象</param>
    /// <param name="eventId">事件ID</param>
    /// <param name="gameEventArgs">事件参数,可以为null</param>
    public static void Dispatch(this IComponentAgent agent, int eventId, GameEventArgs gameEventArgs = null)
    {
        // 自己处理
        SelfHandle(agent, eventId, gameEventArgs);

        if ((EventId)eventId > EventId.RoleSeparator && agent.OwnerType > GlobalConst.ActorTypeSeparator)
        {
            // 全局非玩家事件，抛给所有玩家
            agent.Tell(()
                           =>
                       {
                           return ServerComponentAgent.OnlineRoleForeach(role
                                                                             =>
                                                                         {
                                                                             role.Dispatch(eventId, gameEventArgs);
                                                                         });
                       });
        }
    }

    private static void SelfHandle(IComponentAgent agent, int evtId, GameEventArgs evt)
    {
        agent.Tell(async () =>
        {
            // 事件需要在本actor内执行，不可多线程执行，所以不能使用Task.WhenAll来处理
            var listeners = HotfixManager.FindListeners(agent.OwnerType, evtId);
            if (listeners.IsNullOrEmpty())
            {
                // Log.Warn($"事件：{(EventID)evtId} 没有找到任何监听者");
                return;
            }

            foreach (var listener in listeners)
            {
                var comp = await agent.GetComponentAgent(listener.AgentType);
                try
                {
                    await listener.HandleEvent(comp, evt);
                }
                catch (Exception exception)
                {
                    LogHelper.Error(exception);
                }
            }
        });
    }

    /// <summary>
    /// 分发事件
    /// </summary>
    /// <param name="agent">代理对象</param>
    /// <param name="eventId">事件ID</param>
    /// <param name="args">事件参数</param>
    public static void Dispatch(this IComponentAgent agent, EventId eventId, GameEventArgs args = null)
    {
        Dispatch(agent, (int)eventId, args);
    }
}