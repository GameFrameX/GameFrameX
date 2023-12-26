namespace Server.NetWork.TCPSocket;

public sealed class TcpSocketMessageHelper : IMessageHelper
{
    /// <summary>
    /// 消息处理器。根据消息Id查找消息的处理器
    /// </summary>
    public Func<int, IMessageHandler> MessageHandler { get; set; }

    /// <summary>
    /// 根据消息Id查找消息类型
    /// </summary>
    public Func<int, Type> MessageTypeByIdGetter { get; set; }

    /// <summary>
    /// 根据消息类型查找消息ID
    /// </summary>
    public Func<Type, int> MessageIdByTypeGetter { get; set; }

    public TcpSocketMessageHelper(Func<int, IMessageHandler> messageHandler, Func<int, Type> typeGetter, Func<Type, int> idGetter)
    {
        MessageHandler = messageHandler;
        MessageTypeByIdGetter = typeGetter;
        MessageIdByTypeGetter = idGetter;
    }
}