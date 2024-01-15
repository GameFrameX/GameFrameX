using Server.NetWork.HTTP;

namespace Server.Hotfix.Http;

/// <summary>
/// 热更新
/// http://localhost:20001/game/api?command=Reload&amp;version=1.0.0
/// </summary>
[HttpMsgMapping(typeof(HttpReloadHandler))]
public class HttpReloadHandler : BaseHttpHandler
{
    /// <summary>
    /// ***正式的HttpHandler请一定设置CheckSign为True***
    /// </summary>
    public override bool IsCheckSign => false;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="url"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public override async Task<string> Action(string ip, string url, Dictionary<string, string> parameters)
    {
        if (parameters.TryGetValue("version", out var version))
        {
            await HotfixMgr.LoadHotfixModule(version);
            return Task.FromResult(HttpResult.CreateOk()).Result;
        }

        var result = HttpResult.CreateErrorParam($"参数错误 {nameof(version)}");

        return Task.FromResult(result).Result;
    }
}