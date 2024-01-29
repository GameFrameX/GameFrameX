using System.Buffers;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using MessagePack;
using Server.Extension;
using Server.NetWork.Messages;
using Server.Utility;

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
            // var array = new object[2];
            while (!IsClose())
            {
                await newSendMsgSemaphore.WaitAsync();

                if (!_sendQueue.TryDequeue(out var message))
                {
                    continue;
                }

                var bytes = MessagePackSerializer.Serialize(message);


                // len +timestamp + msgId + bytes.length
                int len = 4 + 4 + 8 + bytes.Length;
                var sendData = ArrayPool<byte>.Shared.Rent(len);
                int offset = 0;
                sendData.WriteInt(len, ref offset);
                sendData.WriteLong(TimeHelper.UnixTimeSeconds(), ref offset);
                var messageType = message.GetType();
                var msgId = MessageHelper.MessageIdByTypeGetter(messageType);
                sendData.WriteInt(msgId, ref offset);
                sendData.WriteBytesWithoutLength(bytes, ref offset);
#if DEBUG

                StringBuilder stringBuilder = new StringBuilder();
                foreach (var b in sendData)
                {
                    stringBuilder.Append(b + " ");
                }

                Logger.Debug($"---发送消息ID:[{message.MsgId}] ==>消息类型:{messageType} 消息内容:{MessagePackSerializer.ConvertToJson(sendData)} \n {stringBuilder}");
#endif
                await webSocket.SendAsync(sendData, WebSocketMessageType.Binary, true, CloseSrc.Token);
                ArrayPool<byte>.Shared.Return(sendData);
            }
        }

        MessageObject DeserializeMsg(MemoryStream stream)
        {
            var data = stream.GetBuffer();

            // StringBuilder stringBuilder = new StringBuilder();
            // for (var index = 0; index < stream.Length; index++)
            // {
            //     var b = data[index];
            //     stringBuilder.Append(b + " ");
            // }
            //
            // Console.WriteLine(stringBuilder);

            var input = new ReadOnlySequence<byte>(data, 0, (int)stream.Length);
            var reader = new SequenceReader<byte>(input);
            reader.TryReadBigEndian(out int msgLength); //reader.ReadInt32()
            reader.TryReadBigEndian(out long time); //reader.ReadInt32()
            reader.TryReadBigEndian(out int magic); //reader.ReadInt32()
            reader.TryReadBigEndian(out int messageId); //reader.ReadInt32()

            var message = MessagePackSerializer.Deserialize<MessageObject>(reader.UnreadSequence);
            message.MsgId = messageId;
            if (message.MsgId != messageId)
            {
                throw new Exception($"解析消息错误，注册消息id和消息无法对应.real:{message.MsgId}, register:{messageId}");
            }

            return message;
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
                var messageObject = DeserializeMsg(stream); // Serializer.Deserialize<Message>(stream);

#if DEBUG
                var messageType = messageObject.GetType();
                Logger.Debug($"---收到消息ID:[{messageObject.MsgId}] ==>消息类型:{messageType} 消息内容:{MessagePackSerializer.SerializeToJson(messageObject)}");
#endif
                onMessage(messageObject);
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