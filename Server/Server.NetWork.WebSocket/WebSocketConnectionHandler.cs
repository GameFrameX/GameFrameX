using Server.Log;
using Server.NetWork.Messages;

namespace Server.NetWork.WebSocket
{
    public abstract class WebSocketConnectionHandler
    {
        public virtual async Task OnConnectedAsync(System.Net.WebSockets.WebSocket socket, string clientAddress)
        {
            LogHelper.Info($"new websocket {clientAddress} connect...");
            WebSocketChannel channel = null;
            channel = new WebSocketChannel(socket, clientAddress, WebSocketServer.MessageHelper, (msg) => _ = Dispatcher(channel, msg));
            await channel.StartAsync();
            OnDisconnection(channel);
        }

        public virtual void OnDisconnection(INetChannel channel)
        {
            LogHelper.Debug($"{channel.RemoteAddress} 断开链接");
        }

        protected async Task Dispatcher(INetChannel channel, MessageObject msg)
        {
            if (msg == null)
            {
                return;
            }

            //LOGGER.Debug($"-------------收到消息{msg.MsgId} {msg.GetType()}");
            var handler = WebSocketServer.MessageHelper.MessageHandler(msg.MessageId);
            if (handler == null)
            {
                LogHelper.Error($"找不到[{msg.MessageId}][{msg.GetType()}]对应的handler");
                return;
            }

            handler.Message = msg;
            handler.Channel = channel;
            await handler.Init();
            await handler.InnerAction();
        }
    }
}