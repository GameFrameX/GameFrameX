using Server.Core.Hotfix.Agent;

namespace Server.Core.Net.Tcp.Handler;

public abstract class RoleComponentHandler : BaseComponentHandler
{
    protected override Task InitActor()
    {
        if (ActorId <= 0)
            ActorId = Channel.GetSessionId();
        return Task.CompletedTask;
    }
}

public abstract class RoleComponentHandler<T> : RoleComponentHandler where T : IComponentAgent
{
    protected override Type ComponentAgentType => typeof(T);
    protected T Comp => (T)CacheComponent;
}