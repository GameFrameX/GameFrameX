using Server.Launcher.Common.Session;
using Server.NetWork.HTTP;

namespace Server.Hotfix.Http
{
    /// <summary>
    /// 获取在线人数
    /// </summary>
    [HttpMsgMapping(typeof(HttpGetOnlinePlayerHandler))]
    public class HttpGetOnlinePlayerHandler : BaseHttpHandler
    {
        public override bool IsCheckSign => false;

        /// <summary>
        /// http://192.168.0.163:20000/game/api?command=online_num_query
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="url"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public override Task<string> Action(string ip, string url, Dictionary<string, string> parameters)
        {
            var res = HttpResult.CreateOk($"当前在线人数:{SessionManager.Count()}").ToString();
            return Task.FromResult(res);
        }
    }
}