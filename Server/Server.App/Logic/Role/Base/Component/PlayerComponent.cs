using Server.Core.Actors;
using Server.Core.Comps;

namespace Server.App.Logic.Role.Base.Component
{
    [ComponentType(ActorType.Player)]
    public sealed class PlayerComponent : StateComp<PlayerState>
    {
    }
}