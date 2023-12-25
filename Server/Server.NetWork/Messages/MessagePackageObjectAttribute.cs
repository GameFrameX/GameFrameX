namespace Server.NetWork.Messages;

/// <summary>
/// 消息对象标签
/// </summary>
public class MessagePackageObjectAttribute : MessagePack.MessagePackObjectAttribute
{
    public MessagePackageObjectAttribute() : base(true)
    {
    }
}