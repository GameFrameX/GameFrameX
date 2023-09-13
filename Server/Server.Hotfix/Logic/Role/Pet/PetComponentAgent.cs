using Server.App.Common.Event;
using Server.App.Logic.Role.Pet;
using Server.Core.Actors;
using Server.Core.Events;
using Server.Core.Hotfix.Agent;
using Server.Hotfix.Logic.Server;
using Server.Hotfix.Logic.Server.Agent;


namespace Server.Hotfix.Logic.Role.Pet
{
    public class PetComponentAgent : StateComponentAgent<PetComp, PetState>
    {
        readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

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
            var serverComp = await ActorMgr.GetCompAgent<ServerComponentAgent>();
            //var level = await serverComp.SendAsync(() => serverComp.GetWorldLevel()); //手动入队的写法
            var level = await serverComp.GetWorldLevel();
            LOGGER.Debug($"PetCompAgent.OnGotNewPet监听到了获得宠物的事件,宠物ID:{param.Value}当前世界等级:{level}");
        }
    }
}