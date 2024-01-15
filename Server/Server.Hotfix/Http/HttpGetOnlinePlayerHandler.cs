using Server.Launcher.Common.Session;
using Server.NetWork.HTTP;

namespace Server.Hotfix.Http
{
    class GetOnlinePlayerResponse
    {
        public int Count { get; set; }
    }

    /// <summary>
    /// 获取在线人数
    /// http://localhost:20001/game/api?command=GetOnlinePlayer
    /// </summary>
    [HttpMsgMapping(typeof(HttpGetOnlinePlayerHandler))]
    public class HttpGetOnlinePlayerHandler : BaseHttpHandler
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
            GetOnlinePlayerResponse response = new GetOnlinePlayerResponse();
            response.Count = SessionManager.Count();
            var res = HttpResult.CreateOk($"当前在线人数:{response.Count}", response);
            return Task.FromResult(res);
        }
    }
}