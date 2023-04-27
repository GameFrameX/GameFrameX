
using Server.Core.Events;

namespace Server.App.Common.Event
{
    public class EventAttribute : EventInfoAttribute
    {
        public EventAttribute(EventID eventId) : base((int)eventId)
        {
        }
    }
}
