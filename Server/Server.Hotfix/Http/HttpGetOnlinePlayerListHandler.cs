using Server.Launcher.Common.Session;
using Server.NetWork.HTTP;

namespace Server.Hotfix.Http;

/// <summary>
/// 获取在线玩家列表
/// </summary>
[HttpMsgMapping(typeof(HttpGetOnlinePlayerListHandler))]
public class HttpGetOnlinePlayerListHandler : BaseHttpHandler
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
        parameters.TryGetValue("pageSize", out var pageSizeStr);
        parameters.TryGetValue("pageIndex", out var pageIndexStr);
        var pageSize = string.IsNullOrEmpty(pageSizeStr) ? 0 : Convert.ToInt32(pageSizeStr);
        var pageIndex = string.IsNullOrEmpty(pageIndexStr) ? 0 : Convert.ToInt32(pageIndexStr);

        var response = SessionManager.GetPageList(pageSize, pageIndex);
        var res = HttpResult.CreateOk($"当前在线玩家", response);
        return Task.FromResult(res);
    }
}