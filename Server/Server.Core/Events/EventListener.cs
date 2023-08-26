using Server.Core.Hotfix.Agent;

namespace Server.Core.Events
{
    public abstract class EventListener<T> : IEventListener where T : IComponentAgent
    {
        public Task HandleEvent(IComponentAgent agent, Event evt)
        {
            return HandleEvent((T) agent, evt);
        }

        protected abstract Task HandleEvent(T agent, Event evt);


        public Type AgentType { get; } = typeof(T);
    }
}