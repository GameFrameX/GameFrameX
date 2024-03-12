namespace Server.NetWork.Messages;

/// <summary>
/// 消息基类接口
/// </summary>
public interface IMessage
{
    /// <summary>
    /// 消息唯一ID
    /// </summary>
    string UniqueId { get; set; }
}