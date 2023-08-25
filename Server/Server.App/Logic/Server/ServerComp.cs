using Server.Core.Actors;
using Server.Core.Comps;
using Server.DBServer.State;
using Server.DBServer.Storage;

namespace Server.App.Logic.Server
{
    public class ServerState : CacheState
    {
        /// <summary>
        /// 世界等级
        /// </summary>
        public int WorldLevel { get; set; } = 1;
    }

    [ComponentType(ActorType.Server)]
    public class ServerComp : StateComp<ServerState>
    {
        /// <summary>
        /// 存放在此处的数据不会回存到数据库
        /// </summary>
        public HashSet<long> OnlineSet = new();
    }
}