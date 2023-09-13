using Server.Hotfix.Logic.Heart.Entity;

namespace Server.Hotfix.Logic.Heart.Component;

[ComponentType(ActorType.Server)]
public class HeartBeatComponent : StateComp<HeartBeatState>
{
}