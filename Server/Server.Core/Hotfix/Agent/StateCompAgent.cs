using Geek.Server.Core.Storage;
using Server.Core.Comps;

namespace Server.Core.Hotfix.Agent
{
    public abstract class StateCompAgent<TComp, TState> : BaseCompAgent<TComp> where TComp : StateComp<TState> where TState : CacheState, new()
    {
        public TState State => Comp.State;
    }
}
