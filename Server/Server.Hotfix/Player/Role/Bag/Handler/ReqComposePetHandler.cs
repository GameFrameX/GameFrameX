using Server.Hotfix.Player.Role.Bag.Agent;

namespace Server.Hotfix.Player.Role.Bag.Handler
{
    [MsgMapping(typeof(ReqComposePet))]
    public class ReqComposePetHandler : RoleComponentHandler<BagComponentAgent>
    {
        public override async Task ActionAsync()
        {
            await Comp.ComposePet(Msg as ReqComposePet);
        }
    }
}