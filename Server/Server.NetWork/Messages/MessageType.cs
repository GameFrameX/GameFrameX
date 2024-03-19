namespace Server.NetWork.Messages;

/// <summary>
/// 消息类型
/// </summary>
public enum MessageType : byte
{
    Unknown = 0,

    /// <summary>
    /// 请求
    /// </summary>
    Request = 1,

    /// <summary>
    /// 返回
    /// </summary>
    Response = 2,

    /// <summary>
    /// Actor请求
    /// </summary>
    ActorRequest = 3,

    /// <summary>
    /// Actor返回
    /// </summary>
    ActorResponse = 4
}