using Server.Core.Utility;
using Server.Setting;
using Server.Utility;

namespace Server.Launcher.Common
{
    public class AppSetting : BaseSetting
    {
        public readonly List<int> Servers = new();

        /// <summary>
        /// 是否是本地
        /// </summary>
        /// <param name="serverId"> 服务ID</param>
        /// <returns> 返回是否是本地</returns>
        public override bool IsLocal(int serverId)
        {
            return base.IsLocal(serverId) || Servers.Contains(serverId);
        }
    }
}