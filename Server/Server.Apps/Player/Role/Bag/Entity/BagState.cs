using Server.DBServer.State;

namespace Server.Apps.Player.Role.Bag.Entity;

public class BagState : CacheState
{
    public Dictionary<int, long> ItemMap = new Dictionary<int, long>();
}