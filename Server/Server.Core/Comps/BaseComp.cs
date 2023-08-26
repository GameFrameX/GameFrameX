using Server.Core.Actors;
using Server.Core.Hotfix;
using Server.Core.Hotfix.Agent;

namespace Server.Core.Comps
{
    public abstract class BaseComp
    {
        private IComponentAgent _cacheAgent = null;
        private readonly object _cacheAgentLock = new();

        public IComponentAgent GetAgent(Type refAssemblyType = null)
        {
            lock (_cacheAgentLock)
            {
                if (_cacheAgent != null && !HotfixMgr.DoingHotfix)
                {
                    return _cacheAgent;
                }

                var agent = HotfixMgr.GetAgent<IComponentAgent>(this, refAssemblyType);
                _cacheAgent = agent;
                return agent;
            }
        }

        public void ClearCacheAgent()
        {
            _cacheAgent = null;
        }

        internal Actor Actor { get; set; }

        internal long ActorId => Actor.Id;

        public bool IsActive { get; private set; } = false;

        public virtual Task Active()
        {
            IsActive = true;
            return Task.CompletedTask;
        }

        public virtual async Task Inactive()
        {
            var agent = GetAgent();
            if (agent != null)
                await agent.Inactive();
        }

        internal virtual Task SaveState()
        {
            return Task.CompletedTask;
        }

        internal virtual bool ReadyToInactive => true;
    }
}