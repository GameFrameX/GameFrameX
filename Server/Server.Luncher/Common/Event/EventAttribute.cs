using Server.Core.Events;

namespace Server.Luncher.Common.Event
{
    public class EventAttribute : EventInfoAttribute
    {
        public EventAttribute(EventId eventId) : base((int) eventId)
        {
        }
    }
}