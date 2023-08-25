using System;
using MessagePack;

namespace Protocol
{
    [MessagePackObject(true)]
    public class Message
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