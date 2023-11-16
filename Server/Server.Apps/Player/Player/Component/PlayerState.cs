using Server.DBServer.State;

namespace Server.Apps.Player.Player.Component;

public class PlayerState : CacheState
{
    public long RoleId => Id;
    public string RoleName;
    public int Level = 1;
    public int VipLevel = 1;
    public DateTime LoginTime;
    public DateTime OfflineTime;
}