using Server.Core.Actors;

namespace Server.Core.Comps
{
    /// <summary>
    /// 组件类型标记
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ComponentTypeAttribute : Attribute
    {
        public ComponentTypeAttribute(ActorType type)
        {
            ActorType = type;
        }

        public ActorType ActorType { get; }
    }
}