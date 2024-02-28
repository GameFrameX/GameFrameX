namespace Server.NetWork;

/// <summary>
/// 消息帮助类
/// </summary>
public interface IMessageHelper
{
    /// <summary>
    /// 消息处理器。根据消息Id查找消息的处理器
    /// </summary>
    Func<int, IMessageHandler> MessageHandler { get; }

    /// <summary>
    /// 根据消息Id查找消息类型
    /// </summary>
    Func<int, Type> MessageTypeByIdGetter { get; }

    /// <summary>
    /// 根据消息类型查找消息ID
    /// </summary>
    Func<Type, int> MessageIdByTypeGetter { get; }
}