namespace Server.NetWork.Messages;

[AttributeUsage(AttributeTargets.Class)]
public class MessageTypeHandlerAttribute : Attribute
{
    public int MessageId { get; }

    public MessageTypeHandlerAttribute(int messageId)
    {
        MessageId = messageId;
    }
}