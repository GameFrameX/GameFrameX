/*using Microsoft.AspNetCore.Connections;
using Newtonsoft.Json;
using Server.NetWork.Messages;
using Server.Setting;

namespace Server.NetWork.UDPSocket
{
    public class UdpConnectionHandler : ConnectionHandler
    {
        static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public override async Task OnConnectedAsync(ConnectionContext connection)
        {
            Logger.Debug($"{connection.RemoteEndPoint?.ToString()} 链成功");
            BaseNetChannel channel = null;
            channel = new UdpChannel(connection, UdpServer.MessageHelper, (msg) => _ = Dispatcher(channel, msg));
            await channel.StartAsync();
            Logger.Debug($"{channel.RemoteAddress} 断开链接");
            OnDisconnection(channel);
        }

        protected virtual void OnDisconnection(BaseNetChannel channel)
        {
        }

        private async Task Dispatcher(BaseNetChannel channel, MessageObject messageObject)
        {
            if (messageObject == null)
            {
                return;
            }

            var messageType = messageObject.GetType();

            if (GlobalSettings.IsDebug)
            {
                Logger.Info($"---收到消息ID:[{messageObject.MsgId}] ==>消息类型:{messageType} 消息内容:{JsonConvert.SerializeObject(messageObject)}");
            }

            var handler = UdpServer.MessageHelper.MessageHandler(messageObject.MsgId);
            if (handler == null)
            {
                Logger.Error($"找不到[{messageObject.MsgId}][{messageObject.GetType()}]对应的handler");
                return;
            }

            handler.Message = messageObject;
            handler.Channel = channel;
            await handler.Init();
            await handler.InnerAction();
        }
    }
}*/