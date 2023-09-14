using Server.Core.Comps;
using Server.DBServer.State;
using Server.DBServer.Storage;

namespace Server.Core.Hotfix.Agent
{
    public abstract class StateComponentAgent<TComp, TState> : BaseComponentAgent<TComp> where TComp : StateComponent<TState> where TState : CacheState, new()
    {
        public TState State => Comp.State;
    }
}