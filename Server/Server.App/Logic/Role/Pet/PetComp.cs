using Server.Core.Actors;
using Server.Core.Comps;
using Server.DBServer.Storage;

namespace Server.App.Logic.Role.Pet
{
    public class PetState : CacheState
    {
    }

    [ComponentType(ActorType.Role)]
    public class PetComp : StateComp<PetState>
    {
    }
}