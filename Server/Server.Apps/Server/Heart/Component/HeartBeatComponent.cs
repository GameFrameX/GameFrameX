using Server.Apps.Server.Heart.Entity;

namespace Server.Apps.Server.Heart.Component;

[ComponentType(ActorType.Server)]
public class HeartBeatComponent : StateComponent<HeartBeatState>
{
}