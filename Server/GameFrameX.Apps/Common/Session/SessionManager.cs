// ==========================================================================================
//  GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  GameFrameX organization and its derivative projects' copyrights, trademarks, patents, and related rights
//  均受中华人民共和国及相关国际法律法规保护。
//  are protected by the laws of the People's Republic of China and relevant international regulations.
//  
//  使用本项目须严格遵守相应法律法规及开源许可证之规定。
//  Usage of this project must strictly comply with applicable laws, regulations, and open-source licenses.
//  
//  本项目采用 MIT 许可证与 Apache License 2.0 双许可证分发，
//  This project is dual-licensed under the MIT License and Apache License 2.0,
//  完整许可证文本请参见源代码根目录下的 LICENSE 文件。
//  please refer to the LICENSE file in the root directory of the source code for the full license text.
//  
//  禁止利用本项目实施任何危害国家安全、破坏社会秩序、
//  It is prohibited to use this project to engage in any activities that endanger national security, disrupt social order,
//  侵犯他人合法权益等法律法规所禁止的行为！
//  or infringe upon the legitimate rights and interests of others, as prohibited by laws and regulations!
//  因基于本项目二次开发所产生的一切法律纠纷与责任，
//  Any legal disputes and liabilities arising from secondary development based on this project
//  本项目组织与贡献者概不承担。
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//  
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository: https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository:  https://gitee.com/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using GameFrameX.Apps.Common.Event;
using GameFrameX.Core.Actors;
using GameFrameX.Core.Events;
using GameFrameX.NetWork.Abstractions;
using GameFrameX.Utility.Setting;

namespace GameFrameX.Apps.Common.Session;

/// <summary>
/// 管理玩家session，一个玩家一个，下线之后移除，顶号之后释放之前的channel，替换channel
/// </summary>
public static class SessionManager
{
    private static readonly ConcurrentDictionary<string, Session> SessionMap = new();

    /// <summary>
    /// 获取当前在线玩家的数量。
    /// </summary>
    /// <returns>当前在线玩家的数量。</returns>
    public static int Count()
    {
        return SessionMap.Count;
    }

    /// <summary>
    /// 获取分页的玩家列表。
    /// </summary>
    /// <param name="pageSize">每页的玩家数量。</param>
    /// <param name="pageIndex">当前页的索引，从0开始。</param>
    /// <returns>指定页的玩家会话列表。</returns>
    public static List<Session> GetPageList(int pageSize, int pageIndex)
    {
        var result = SessionMap.Values.OrderBy(m => m.CreateTime)
            .Where(m => ActorManager.HasActor(m.PlayerId))
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToList();
        return result;
    }

    /// <summary>
    /// 踢掉指定角色ID的玩家，移除其会话。
    /// </summary>
    /// <param name="roleId">要踢掉的玩家的角色ID。</param>
    public static void KickOffLineByUserId(long roleId)
    {
        var roleSession = Get(m => m.PlayerId == roleId);
        if (roleSession != null)
        {
            if (SessionMap.TryRemove(roleSession.SessionId, out var value) && ActorManager.HasActor(roleSession.PlayerId))
            {
                EventDispatcher.Dispatch(roleSession.PlayerId, (int)EventId.SessionRemove);
            }
        }
    }

    /// <summary>
    /// 根据角色ID获取对应的会话对象。
    /// 会话对象必须已经存在才会返回。
    /// </summary>
    /// <param name="roleId">角色ID。</param>
    /// <returns>对应的会话对象，如果不存在则返回null。</returns>
    public static Session GetByRoleId(long roleId)
    {
        var roleSession = Get(m => m.PlayerId == roleId);
        if (roleSession != null && ActorManager.HasActor(roleSession.PlayerId))
        {
            return roleSession;
        }

        return roleSession;
    }

    /// <summary>
    /// 根据会话ID获取连接的会话对象。
    /// </summary>
    /// <param name="sessionId">会话ID。</param>
    /// <returns>对应的会话对象，如果不存在则返回null。</returns>
    public static Session Get(string sessionId)
    {
        SessionMap.TryGetValue(sessionId, out var value);
        return value;
    }

    /// <summary>
    /// 根据指定的查询条件获取会话对象。
    /// </summary>
    /// <param name="predicate">查询条件的委托。</param>
    /// <returns>符合条件的会话对象，如果不存在则返回null。</returns>
    public static Session Get(Func<Session, bool> predicate)
    {
        return SessionMap.Values.FirstOrDefault(predicate);
    }

    /// <summary>
    /// 根据指定的查询条件获取会话对象列表。
    /// </summary>
    /// <param name="predicate">查询条件的委托。</param>
    /// <returns>符合条件的会话对象列表。</returns>
    public static List<Session> GetList(Func<Session, bool> predicate)
    {
        return SessionMap.Values.Where(predicate).ToList();
    }

    /// <summary>
    /// 移除指定会话ID的玩家。
    /// </summary>
    /// <param name="sessionId">要移除的会话ID。</param>
    /// <returns>被移除的会话对象，如果不存在则返回null。</returns>
    public static Session Remove(string sessionId)
    {
        if (SessionMap.TryRemove(sessionId, out var value) && ActorManager.HasActor(value.PlayerId))
        {
            EventDispatcher.Dispatch(value.PlayerId, (int)EventId.SessionRemove);
        }

        return value;
    }

    /// <summary>
    /// 移除所有在线玩家的会话。
    /// </summary>
    /// <returns>一个表示异步操作的任务。</returns>
    public static Task RemoveAll()
    {
        foreach (var session in SessionMap.Values)
        {
            if (ActorManager.HasActor(session.PlayerId))
            {
                EventDispatcher.Dispatch(session.PlayerId, (int)EventId.SessionRemove);
            }
        }

        SessionMap.Clear();
        return Task.CompletedTask;
    }

    /// <summary>
    /// 获取指定会话ID的网络连接通道。
    /// </summary>
    /// <param name="sessionId">会话ID。</param>
    /// <returns>对应的网络连接通道，如果不存在则返回null。</returns>
    public static INetWorkChannel GetChannel(string sessionId)
    {
        SessionMap.TryGetValue(sessionId, out var session);
        return session?.WorkChannel;
    }

    /// <summary>
    /// 添加新的连接会话。
    /// </summary>
    /// <param name="session">要添加的会话对象。</param>
    public static void Add(Session session)
    {
        session.WorkChannel.SetData(GlobalConst.SessionIdKey, session.SessionId);
        SessionMap[session.SessionId] = session;
    }

    /// <summary>
    /// 更新会话，处理角色ID和签名的更新。
    /// 如果角色ID已在其他设备上登录，则会通知旧会话并关闭其连接。
    /// </summary>
    /// <param name="sessionId">会话ID，用于标识当前会话</param>
    /// <param name="roleId">角色ID，表示当前会话所关联的角色</param>
    /// <param name="sign">签名，用于验证会话的唯一性</param>
    public static async void UpdateSession(string sessionId, long roleId, string sign)
    {
        // 获取与角色ID关联的旧会话
        var oldSession = GetByRoleId(roleId);
        if (oldSession != null)
        {
            // 创建提示消息，通知用户其账号已在其他设备上登录
            var msg = new RespPrompt
            {
                Type = 5,
                Content = "你的账号已在其他设备上登陆",
            };
            // 发送消息给旧会话
            await oldSession.WriteAsync(msg);
            // 清除旧会话的连接数据并关闭连接
            oldSession.WorkChannel.ClearData();
            oldSession.WorkChannel.Close();
            // 这里先移除，等待Disconnected回调断开在移除的话有延迟
            Remove(oldSession.SessionId);
        }

        // 获取当前会话并更新角色ID和签名
        var session = Get(sessionId);
        if (session.IsNull())
        {
            return;
        }
        session.SetPlayerId(roleId);
        session.SetSign(sign);
    }
}