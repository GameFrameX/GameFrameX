using System;

namespace Protocol
{
    public abstract class Message
    {
    }

    public interface IRequestMessage
    {
    }

    public interface IResponseMessage
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class MessageTypeHandler : Attribute
    {
        public int MessageId { get; }

        public MessageTypeHandler(int messageId)
        {
            MessageId = messageId;
        }
    }
}