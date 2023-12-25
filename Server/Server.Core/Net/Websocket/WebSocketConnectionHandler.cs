using System.Net.WebSockets;
using Server.Core.Hotfix;
using Server.NetWork.Messages;

namespace Server.Core.Net.Websocket
{
    public abstract class WebSocketConnectionHandler
    {
        static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public virtual async Task OnConnectedAsync(WebSocket socket, string clientAddress)
        {
            Logger.Info($"new websocket {clientAddress} connect...");
            WebSocketChannel channel = null;
            channel = new WebSocketChannel(socket, clientAddress , (msg) => _ = Dispatcher(channel, msg));
            await channel.StartAsync();
            OnDisconnection(channel);
        }

        public virtual void OnDisconnection(NetChannel channel)
        {
            Logger.Debug($"{channel.RemoteAddress} 断开链接");
        }

        protected async Task Dispatcher(NetChannel channel, MessageObject msg)
        {
            if (msg == null)
                return;

            //LOGGER.Debug($"-------------收到消息{msg.MsgId} {msg.GetType()}");
            var handler = HotfixMgr.GetTcpHandler(msg.MsgId);
            if (handler == null)
            {
                Logger.Error($"找不到[{msg.MsgId}][{msg.GetType()}]对应的handler");
                return;
            }
            handler.Message = msg;
            handler.Channel = channel;
            await handler.Init();
            await handler.InnerAction();
        }
    }
}
