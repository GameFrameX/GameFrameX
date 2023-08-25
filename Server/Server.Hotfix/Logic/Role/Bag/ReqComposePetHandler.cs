namespace Server.Hotfix.Logic.Role.Bag
{
    [MsgMapping(typeof(ReqComposePet))]
    public class ReqComposePetHandler : RoleCompHandler<BagCompAgent>
    {
        public override async Task ActionAsync()
        {
            await Comp.ComposePet(Msg as ReqComposePet);
        }
    }
}