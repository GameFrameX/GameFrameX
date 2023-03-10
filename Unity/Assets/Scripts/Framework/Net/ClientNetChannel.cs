using System;
using Bedrock.Framework;
using Bedrock.Framework.Protocols;
using Geek.Server;
using MessagePack;
using System.Threading.Tasks;

namespace Geek.Client
{
    public class ClientNetChannel : NetChannel
    {
        public ClientNetChannel(ConnectionContext context, IProtocal<NMessage> protocal)
            : base(context, protocal)
        {
            Task.Run(NetLooping);
        }

        public void Execute()
        {
            _ = NetLooping();
        }

        protected override void ConnectionClosed()
        {
            base.ConnectionClosed();
            connectionClosed = true;
            GameClient.Singleton.OnDisConnected();
        }

        private bool connectionClosed = false;

        private async Task NetLooping()
        {
            while (!connectionClosed)
            {
                try
                {
                    var result = await Reader.ReadAsync(Protocol);

                    var message = result.Message;
                    //分发消息
                    Dispatcher(message);

                    if (result.IsCompleted)
                        break;
                }
                catch (ConnectionResetException)
                {
                    UnityEngine.Debug.Log($"{Context.ConnectionId} disconnected");
                    break;
                }
                finally
                {
                    Reader.Advance();
                }
            }
        }

        private static Action<int, NMessage> _action;

        public static void SetMessage(Action<int, NMessage> action)
        {
            _action = action;
        }

        public static void Dispatcher(NMessage message)
        {
            var reader = new SequenceReader<byte>(message.Payload);

            //数据包长度+消息ID=两个int=8位
            if (message.Payload.Length < 4)
                return;

            //消息id
            reader.TryReadBigEndian(out int msgId);
            _action?.Invoke(msgId, message);
        }
    }
}