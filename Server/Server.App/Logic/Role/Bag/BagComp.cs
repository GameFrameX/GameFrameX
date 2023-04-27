using Geek.Server.Core.Storage;
using Server.Core.Actors;
using Server.Core.Comps;

namespace Server.App.Logic.Role.Bag
{

    public class BagState : CacheState
    {
        public Dictionary<int, long> ItemMap = new Dictionary<int, long>();
    }

    [Comp(ActorType.Role)]
    public class BagComp : StateComp<BagState>
    {

    }
}
