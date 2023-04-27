using Geek.Server.Core.Storage;
using Server.Core.Actors;
using Server.Core.Comps;

namespace Server.App.Logic.Role.Pet
{

    public class PetState : CacheState
    {

    }

    [Comp(ActorType.Role)]
    public class PetComp : StateComp<PetState>
    {
    }
}
