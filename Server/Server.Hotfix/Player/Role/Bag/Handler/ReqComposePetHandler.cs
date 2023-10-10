using Server.Core.Net.BaseHandler;
using Server.Hotfix.Player.Role.Bag.Agent;

namespace Server.Hotfix.Player.Role.Bag.Handler
{
    [MsgMapping(typeof(ReqComposePet))]
    public class ReqComposePetHandler : RoleComponentHandler<BagComponentAgent>
    {
        protected override async Task ActionAsync()
        {
            await Comp.ComposePet(Message as ReqComposePet);
        }
    }
}