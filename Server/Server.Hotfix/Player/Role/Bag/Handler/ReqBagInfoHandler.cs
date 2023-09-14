using Server.Hotfix.Player.Role.Bag.Agent;

namespace Server.Hotfix.Player.Role.Bag.Handler
{
    [MsgMapping(typeof(ReqBagInfo))]
    public class ReqBagInfoHandler : RoleComponentHandler<BagComponentAgent>
    {
        public override async Task ActionAsync()
        {
            await Comp.GetBagInfo(Msg as ReqBagInfo);
        }
    }
}