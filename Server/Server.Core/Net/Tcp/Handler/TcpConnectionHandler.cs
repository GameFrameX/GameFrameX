using Microsoft.AspNetCore.Connections;
using Newtonsoft.Json;
using Server.Core.Hotfix;
using Server.Core.Net.Messages;
using Server.Core.Net.Tcp.Codecs;

namespace Server.Core.Net.Tcp.Handler
{
    public class TcpConnectionHandler : ConnectionHandler
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public TcpConnectionHandler()
        {
        }

        public override async Task OnConnectedAsync(ConnectionContext connection)
        {
            OnConnection(connection);
            var channel = new NetChannel(connection, new NetMessageProtocalHandler());
            var remoteInfo = channel.Context.RemoteEndPoint;
            while (!channel.IsClose())
            {
                try
                {
                    var result = await channel.Reader.ReadAsync(channel.Protocol);
                    var message = result.Message;

                    if (result.IsCompleted)
                    {
                        break;
                    }

                    var decodeMessage = MsgDecoder.Decode(connection, message);
                    _ = Dispatcher(channel, decodeMessage);
                }
                catch (ConnectionResetException)
                {
                    LOGGER.Info($"{remoteInfo} ConnectionReset...");
                    break;
                }
                catch (ConnectionAbortedException)
                {
                    LOGGER.Info($"{remoteInfo} ConnectionAborted...");
                    break;
                }
                catch (Exception e)
                {
                    LOGGER.Error($"{remoteInfo} Exception:{e.Message}");
                }

                try
                {
                    channel.Reader.Advance();
                }
                catch (Exception e)
                {
                    LOGGER.Error($"{remoteInfo} channel.Reader.Advance Exception:{e.Message}");
                    break;
                }
            }

            OnDisconnection(channel);
        }

        protected virtual void OnConnection(ConnectionContext connection)
        {
            LOGGER.Debug($"{connection.RemoteEndPoint?.ToString()} 链接成功");
        }

        protected virtual void OnDisconnection(NetChannel channel)
        {
            LOGGER.Debug($"{channel.Context.RemoteEndPoint?.ToString()} 断开链接");
        }

        private async Task Dispatcher(NetChannel channel, Message msg)
        {
            if (msg == null)
            {
                return;
            }

            var messageType = msg.GetType();
            LOGGER.Debug(
                $"---收到消息ID:[{msg.MsgId}] ==>消息类型:{messageType} 消息内容:{JsonConvert.SerializeObject(msg)}");
            var handler = HotfixMgr.GetTcpHandler(msg.MsgId);
            if (handler == null)
            {
                LOGGER.Error($"找不到[{msg.MsgId}][{messageType}]对应的handler");
                return;
            }

            handler.Msg = msg;
            handler.Channel = channel;
            // 处理器初始化
            await handler.Init();
            // 执行处理器
            await handler.InnerAction();
        }
    }
}