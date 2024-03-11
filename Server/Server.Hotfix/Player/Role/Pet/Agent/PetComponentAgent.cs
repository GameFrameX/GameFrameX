using Server.Apps.Player.Role.Pet.Component;
using Server.Apps.Player.Role.Pet.Entity;
using Server.Hotfix.Server.Server.Agent;

namespace Server.Hotfix.Player.Role.Pet.Agent
{
    public class PetComponentAgent : StateComponentAgent<PetComponent, PetState>
    {
        [Event(EventId.GotNewPet)]
        class EL : EventListener<PetComponentAgent>
        {
            protected override async Task HandleEvent(PetComponentAgent agent, Event evt)
            {
                switch ((EventId) evt.EventId)
                {
                    case EventId.GotNewPet:
                        await agent.OnGotNewPet((OneParam<int>) evt.Data);
                        break;
                    default:
                        break;
                }
            }
        }

        private async Task OnGotNewPet(OneParam<int> param)
        {
            var serverComp = await ActorManager.GetComponentAgent<ServerComponentAgent>();
            //var level = await serverComp.SendAsync(() => serverComp.GetWorldLevel()); //手动入队的写法
            var level = await serverComp.GetWorldLevel();
            LogHelper.Debug($"PetCompAgent.OnGotNewPet监听到了获得宠物的事件,宠物ID:{param.Value}当前世界等级:{level}");
        }
    }
}