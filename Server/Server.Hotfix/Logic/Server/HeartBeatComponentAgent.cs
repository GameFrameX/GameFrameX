using Server.Core.Actors;
using Server.Core.Comps;
using Server.Core.Hotfix.Agent;
using Server.DBServer.State;
using Server.DBServer.Storage;

namespace Server.Hotfix.Logic.Login;

public class HeartBeatState : CacheState
{
    // public ConcurrentDictionary<string, PlayerInfo> PlayerMap = new();
}

[ComponentType(ActorType.Server)]
public class HeartBeatComp : StateComp<HeartBeatState>
{
}

public class HeartBeatComponentAgent : StateComponentAgent<HeartBeatComp, HeartBeatState>
{
}