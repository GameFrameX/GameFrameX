using System.Collections.Concurrent;
using Server.Core.Actors;
using Server.Core.Events;
using Server.Launcher.Common.Event;
using Server.NetWork;
using Server.Proto.Proto;
using Server.Setting;

namespace Server.Launcher.Common.Session
{
    /// <summary>
    /// 管理玩家session，一个玩家一个，下线之后移除，顶号之后释放之前的channel，替换channel
    /// </summary>
    public sealed class SessionManager
    {
        private static readonly ConcurrentDictionary<long, Session> sessionMap = new ConcurrentDictionary<long, Session>();

        /// <summary>
        /// 玩家数量
        /// </summary>
        /// <returns></returns>
        public static int Count()
        {
            return sessionMap.Count;
        }

        /// <summary>
        /// 获取分页玩家列表
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        public static List<Session> GetPageList(int pageSize, int pageIndex)
        {
            var result = sessionMap.Values.OrderBy(m => m.CreateTime).Where(m => ActorManager.HasActor(m.Id)).Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return result;
        }

        /// <summary>
        /// 踢掉玩家
        /// </summary>
        /// <param name="roleId">链接ID</param>
        public static void KickOffLineByUserId(long roleId)
        {
            var roleSession = sessionMap.Values.FirstOrDefault(m => m.RoleId == roleId);
            if (roleSession != null)
            {
                if (sessionMap.TryRemove(roleSession.Id, out var value) && ActorManager.HasActor(roleSession.Id))
                {
                    EventDispatcher.Dispatch(roleSession.Id, (int)EventId.SessionRemove);
                }
            }
        }

        /// <summary>
        /// 移除玩家
        /// </summary>
        /// <param name="sessionId">链接ID</param>
        public static void Remove(long sessionId)
        {
            if (sessionMap.TryRemove(sessionId, out var _) && ActorManager.HasActor(sessionId))
            {
                EventDispatcher.Dispatch(sessionId, (int)EventId.SessionRemove);
            }
        }

        /// <summary>
        /// 移除全部
        /// </summary>
        /// <returns></returns>
        public static Task RemoveAll()
        {
            foreach (var session in sessionMap.Values)
            {
                if (ActorManager.HasActor(session.Id))
                {
                    EventDispatcher.Dispatch(session.Id, (int)EventId.SessionRemove);
                }
            }

            sessionMap.Clear();
            return Task.CompletedTask;
        }

        /// <summary>
        /// 获取链接
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public static BaseNetChannel GetChannel(long sessionId)
        {
            sessionMap.TryGetValue(sessionId, out var session);
            return session?.Channel;
        }

        /// <summary>
        /// 添加新连接
        /// </summary>
        /// <param name="session"></param>
        public static void Add(Session session)
        {
            if (sessionMap.TryGetValue(session.Id, out var oldSession) && oldSession.Channel != session.Channel)
            {
                if (oldSession.Sign != session.Sign)
                {
                    var msg = new RespPrompt
                    {
                        Type = 5,
                        Content = "你的账号已在其他设备上登陆"
                    };
                    oldSession.WriteAsync(msg);
                }

                // 新连接 or 顶号
                oldSession.Channel.RemoveData(GlobalConst.SessionIdKey);
                oldSession.Channel.Close();
            }

            session.Channel.SetData(GlobalConst.SessionIdKey, session.Id);
            sessionMap[session.Id] = session;
        }
    }
}