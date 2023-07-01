using Server.Core.Actors;
using Server.Core.Comps;
using Server.DBServer.Storage;

namespace Server.App.Logic.Role.Bag
{
    public class BagState : CacheState
    {
        public Dictionary<int, long> ItemMap = new Dictionary<int, long>();
    }

    [ComponentType(ActorType.Role)]
    public class BagComp : StateComp<BagState>
    {
    }
}