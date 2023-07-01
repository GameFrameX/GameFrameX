using Server.Core.Actors;
using Server.Core.Comps;
using Server.Core.Hotfix.Agent;
using Server.Core.Utility;

namespace Server.Core.Net.Tcp.Handler
{
    public abstract class BaseCompHandler : BaseTcpHandler
    {
        protected long ActorId { get; set; }

        protected abstract Type CompAgentType { get; }

        public ICompAgent CacheComp { get; set; }

        protected abstract Task InitActor();

        public override async Task Init()
        {
            await InitActor();
            if (CacheComp == null)
            {
                CacheComp = await ActorMgr.GetCompAgent(ActorId, CompAgentType);
                Console.WriteLine(CacheComp);
            }
        }

        public override Task InnerAction()
        {
            CacheComp.Tell(ActionAsync);
            return Task.CompletedTask;
        }

        protected Task<OtherAgent> GetCompAgent<OtherAgent>() where OtherAgent : ICompAgent
        {
            return CacheComp.GetCompAgent<OtherAgent>();
        }
    }

    public abstract class RoleCompHandler : BaseCompHandler
    {
        protected override Task InitActor()
        {
            if (ActorId <= 0)
                ActorId = Channel.GetSessionId();
            return Task.CompletedTask;
        }
    }


    public abstract class GlobalCompHandler : BaseCompHandler
    {
        protected override Task InitActor()
        {
            if (ActorId <= 0)
            {
                var compType = CompAgentType.BaseType.GetGenericArguments()[0];
                ActorType actorType = ComponentRegister.GetActorType(compType);
                ActorId = IdGenerator.GetActorID(actorType);
            }

            return Task.CompletedTask;
        }
    }

    public abstract class RoleCompHandler<T> : RoleCompHandler where T : ICompAgent
    {
        protected override Type CompAgentType => typeof(T);
        protected T Comp => (T) CacheComp;
    }

    public abstract class GlobalCompHandler<T> : GlobalCompHandler where T : ICompAgent
    {
        protected override Type CompAgentType => typeof(T);
        protected T Comp => (T) CacheComp;
    }
}