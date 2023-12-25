namespace Server.NetWork.WebSocket;

public class WebSocketChannelHandler : WebSocketConnectionHandler
{
    public WebSocketChannelHandler(Func<int, IMessageHandler> messageHandler) : base(messageHandler)
    {
    }
}