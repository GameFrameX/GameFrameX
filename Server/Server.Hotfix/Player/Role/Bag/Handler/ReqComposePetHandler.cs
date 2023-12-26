using Server.Core.Net.BaseHandler;
using Server.Hotfix.Player.Role.Bag.Agent;
using Server.NetWork.Messages;

namespace Server.Hotfix.Player.Role.Bag.Handler
{
    [MessageMapping(typeof(ReqComposePet))]
    public class ReqComposePetHandler : RoleComponentHandler<BagComponentAgent>
    {
        protected override async Task ActionAsync()
        {
            await Comp.ComposePet(Message as ReqComposePet);
        }
    }
}