using Server.Core.Net.BaseHandler;
using Server.Hotfix.Player.Role.Bag.Agent;
using Server.NetWork.Messages;

namespace Server.Hotfix.Player.Role.Bag.Handler
{
    [MessageMapping(typeof(ReqBagInfo))]
    public class ReqBagInfoHandler : RoleComponentHandler<BagComponentAgent>
    {
        protected override async Task ActionAsync()
        {
            await Comp.GetBagInfo(Message as ReqBagInfo);
        }
    }
}