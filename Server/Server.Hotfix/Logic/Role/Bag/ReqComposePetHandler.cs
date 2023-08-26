namespace Server.Hotfix.Logic.Role.Bag
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