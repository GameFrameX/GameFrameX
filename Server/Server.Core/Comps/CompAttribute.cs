using Server.Core.Actors;

namespace Server.Core.Comps
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CompAttribute : Attribute
    {
        public CompAttribute(ActorType type)
        {
            ActorType = type;
        }

        public ActorType ActorType { get; }

    }
}
