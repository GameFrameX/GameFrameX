using Server.Apps.Heart.Entity;

namespace Server.Apps.Heart.Component;

[ComponentType(ActorType.Server)]
public class HeartBeatComponent : StateComponent<HeartBeatState>
{
}