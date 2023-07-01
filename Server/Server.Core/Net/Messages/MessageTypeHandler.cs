namespace Server.Core.Net.Messages;

[AttributeUsage(AttributeTargets.Class)]
public class MessageTypeHandler : Attribute
{
    public int MessageId { get; }

    public MessageTypeHandler(int messageId)
    {
        MessageId = messageId;
    }
}