using Server.Launcher.Common.Session;
using Server.NetWork.HTTP;

namespace Server.Hotfix.Http;

/// <summary>
/// 将指定角色的玩家从当前服务断开
/// http://localhost:20001/game/api?command=KickOffLineByUserIdPlayer
/// </summary>
[HttpMsgMapping(typeof(HttpKickOffLineByUserIdPlayerHandler))]
public class HttpKickOffLineByUserIdPlayerHandler : BaseHttpHandler
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="url"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public override Task<string> Action(string ip, string url, Dictionary<string, string> parameters)
    {
        if (parameters.TryGetValue("roleId", out var roleId) && !string.IsNullOrEmpty(roleId))
        {
            SessionManager.KickOffLineByUserId(Convert.ToInt64(roleId));

            return Task.FromResult(HttpResult.CreateOk());
        }

        var res = HttpResult.CreateErrorParam("角色ID异常");
        return Task.FromResult(res);
    }
}