using Server.Core.Events;

namespace Server.Launcher.Common.Event
{
    /// <summary>
    /// 表示事件的特性。
    /// </summary>
    public class EventAttribute : EventInfoAttribute
    {
        /// <summary>
        /// 使用指定的事件ID初始化 <see cref="EventAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="eventId">事件ID。</param>
        public EventAttribute(EventId eventId) : base((int)eventId)
        {
        }
    }
}