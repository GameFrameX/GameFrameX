using Server.Apps.Role.Pet.Entity;

namespace Server.Apps.Role.Pet.Component
{
    [ComponentType(ActorType.Player)]
    public class PetComponent : StateComponent<PetState>
    {
    }
}