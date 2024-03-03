namespace Server.NetWork.Messages;

/// <summary>
/// 消息基类接口
/// </summary>
public interface IMessage
{
    string UniqueId { get; set; }
}