using Server.Core.Events;

namespace Server.App.Common.Event
{
    public class EventAttribute : EventInfoAttribute
    {
        public EventAttribute(EventId eventId) : base((int) eventId)
        {
        }
    }
}