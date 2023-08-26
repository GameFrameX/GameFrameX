namespace Server.Hotfix.Logic.Role.Bag
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