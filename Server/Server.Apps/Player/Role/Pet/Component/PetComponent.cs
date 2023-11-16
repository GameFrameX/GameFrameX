using Server.Apps.Player.Role.Pet.Entity;

namespace Server.Apps.Player.Role.Pet.Component
{
    [ComponentType(ActorType.Player)]
    public class PetComponent : StateComponent<PetState>
    {
    }
}