using System.Collections.Concurrent;
using System.Net.WebSockets;
using MessagePack;
using PolymorphicMessagePack;
using Server.NetWork.Messages;

namespace Server.NetWork.WebSocket
{
    public sealed class WebSocketChannel : BaseNetChannel
    {
        static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        System.Net.WebSockets.WebSocket webSocket;
        readonly Action<MessageObject> onMessage;
        private readonly ConcurrentQueue<MessageObject> _sendQueue = new();
        private readonly SemaphoreSlim newSendMsgSemaphore = new(0);

        public WebSocketChannel(System.Net.WebSockets.WebSocket webSocket, string remoteAddress, IMessageHelper messageHelper, Action<MessageObject> onMessage = null) : base(messageHelper)
        {
            this.RemoteAddress = remoteAddress;
            this.webSocket = webSocket;
            this.onMessage = onMessage;
        }

        public override async void Close()
        {
            try
            {
                base.Close();
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "socketclose", CancellationToken.None);
            }
            catch
            {
            }
            finally
            {
                webSocket = null;
            }
        }

        public override async Task StartAsync()
        {
            try
            {
                _ = DoSend();
                await DoRevice();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }
        }

        public override void WriteAsync(IMessage msg, int uniId, int code, string desc = "")
        {
        }

        async Task DoSend()
        {
            var array = new object[2];
            while (!IsClose())
            {
                await newSendMsgSemaphore.WaitAsync();

                if (!_sendQueue.TryDequeue(out var message))
                {
                    continue;
                }

                array[0] = message.MsgId;
                array[1] = message;
                //这里为了应对前端是js等不方便处理多态的情况 
                var data = MessagePackSerializer.Serialize(array, MessagePackSerializerOptions.Standard);
#if DEBUG
                Logger.Info("发送消息:" + MessagePackSerializer.ConvertToJson(data));
#endif
                await webSocket.SendAsync(data, WebSocketMessageType.Binary, true, CloseSrc.Token);
            }
        }

        MessageObject DeserializeMsg(MemoryStream stream)
        {
            var data = stream.GetBuffer();
            var reader = new MessagePackReader(new ReadOnlyMemory<byte>(data, 0, (int)stream.Length));
            Type type = null;
            if (reader.NextMessagePackType == MessagePackType.Array)
            {
                var count = reader.ReadArrayHeader();
                if (count != 2)
                    throw new MessagePackSerializationException("Invalid polymorphic array count");
                if (reader.NextMessagePackType == MessagePackType.Integer)
                {
                    var typeId = reader.ReadInt32();
                    if (!PolymorphicTypeMapper.TryGet(typeId, out type))
                        throw new MessagePackSerializationException($"Cannot find Type Id: {typeId} registered in {nameof(PolymorphicTypeMapper)}");
                }
            }
            else
            {
                throw new MessagePackSerializationException("不是正确的序列化格式...");
            }

            return MessagePackSerializer.Deserialize(type, ref reader, MessagePackSerializerOptions.Standard) as MessageObject;
        }

        async Task DoRevice()
        {
            var stream = new MemoryStream();
            var buffer = new ArraySegment<byte>(new byte[2048]);

            while (!IsClose())
            {
                int len = 0;
                WebSocketReceiveResult result;
                stream.SetLength(0);
                stream.Seek(0, SeekOrigin.Begin);
                do
                {
                    result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                    len += result.Count;
                    stream.Write(buffer.Array, buffer.Offset, result.Count);
                } while (!result.EndOfMessage);

                if (result.MessageType == WebSocketMessageType.Close)
                    break;

                stream.Seek(0, SeekOrigin.Begin);
                //这里默认用多态类型的反序列方式，里面做了兼容处理 
                var message = DeserializeMsg(stream); // Serializer.Deserialize<Message>(stream);

#if DEBUG
                Logger.Info("收到消息:" + message.GetType().Name + "  " + MessagePackSerializer.SerializeToJson(message));
#endif
                onMessage(message);
            }

            stream.Close();
        }

        public override void Write(IMessage msg)
        {
            MessageObject messageObject = (MessageObject)msg;
            _sendQueue.Enqueue(messageObject);
            newSendMsgSemaphore.Release();
        }
    }
}