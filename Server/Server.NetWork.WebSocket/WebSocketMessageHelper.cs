using Server.NetWork.TCPSocket;

namespace Server.NetWork.WebSocket;

public sealed class WebSocketMessageHelper : BaseSocketMessageHelper
{
    /// <summary>
    /// 构造消息帮助类
    /// </summary>
    /// <param name="messageHandler">消息处理器</param>
    /// <param name="typeGetter"></param>
    /// <param name="idGetter"></param>
    public WebSocketMessageHelper(Func<int, IMessageHandler> messageHandler, Func<int, Type> typeGetter, Func<Type, int> idGetter) : base(messageHandler, typeGetter, idGetter)
    {
    }
}