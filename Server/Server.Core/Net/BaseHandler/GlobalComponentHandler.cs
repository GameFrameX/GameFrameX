using Server.Core.Actors;
using Server.Core.Comps;
using Server.Core.Hotfix.Agent;
using Server.Core.Utility;

namespace Server.Core.Net.BaseHandler;

public abstract class GlobalComponentHandler : BaseComponentHandler
{
    protected override Task InitActor()
    {
        if (ActorId <= 0)
        {
            var compType = ComponentAgentType.BaseType.GetGenericArguments()[0];
            ActorType actorType = ComponentRegister.GetActorType(compType);
            ActorId = IdGenerator.GetActorID(actorType);
        }

        return Task.CompletedTask;
    }
}

public abstract class GlobalComponentHandler<T> : GlobalComponentHandler where T : IComponentAgent
{
    protected override Type ComponentAgentType => typeof(T);
    protected T Comp => (T)CacheComponent;
}