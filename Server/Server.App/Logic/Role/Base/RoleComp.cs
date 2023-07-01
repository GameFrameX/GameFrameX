using Server.Core.Actors;
using Server.Core.Comps;
using Server.DBServer.Storage;

namespace Server.App.Logic.Role.Base
{
    [ComponentType(ActorType.Role)]
    public class RoleComp : StateComp<RoleState>
    {
    }

    public class RoleState : CacheState
    {
        public long RoleId => Id;
        public string RoleName;
        public int Level = 1;
        public int VipLevel = 1;
        public DateTime CreateTime;
        public DateTime LoginTime;
        public DateTime OfflineTime;
    }
}