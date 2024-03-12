using System.Buffers;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using Server.Extension;
using Server.Log;
using Server.NetWork.Messages;
using Server.Serialize.Serialize;
using Server.Setting;
using Server.Utility;

namespace Server.NetWork.WebSocket
{
    public sealed class WebSocketChannel : BaseNetChannel
    {
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
                LogHelper.Error(e.Message);
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

                var bytes = SerializerHelper.Serialize(message);

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
                if (WebSocketServer.AppSetting.IsDebug && WebSocketServer.AppSetting.IsDebugSend)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    for (var index = 0; index < len; index++)
                    {
                        var b = sendData[index];
                        stringBuilder.Append(b + " ");
                    }

                    LogHelper.Info($"---发送消息ID:[{message.MessageId}] ==>消息类型:{messageType} 消息内容长度：{len}=>{bytes.Length} 消息内容:{JsonConvert.SerializeObject(sendData)} 消息字节数组:{stringBuilder}");
                }

                await webSocket.SendAsync(sendData, WebSocketMessageType.Binary, true, CloseSrc.Token);
                ArrayPool<byte>.Shared.Return(sendData);
            }
        }

        MessageObject DeserializeMsg(MemoryStream stream)
        {
            var data = stream.GetBuffer();


            var input = new ReadOnlySequence<byte>(data, 0, (int)stream.Length);
            var reader = new SequenceReader<byte>(input);
            reader.TryReadBigEndian(out int msgLength); //reader.ReadInt32()
            reader.TryReadBigEndian(out long time); //reader.ReadInt32()
            reader.TryReadBigEndian(out int magic); //reader.ReadInt32()
            reader.TryReadBigEndian(out int messageId); //reader.ReadInt32()


            // string json = Encoding.UTF8.GetString(reader.UnreadSequence.First.ToArray());
            var messageType = MessageHelper.MessageTypeByIdGetter(messageId);
            var messageObject = (MessageObject)SerializerHelper.Deserialize(reader.UnreadSequence.First.ToArray(), messageType);

            // var message = MessagePackSerializer.Deserialize<MessageObject>(reader.UnreadSequence);
            messageObject.MessageId = messageId;
            if (WebSocketServer.AppSetting.IsDebug && WebSocketServer.AppSetting.IsDebugReceive)
            {
                StringBuilder stringBuilder = new StringBuilder();
                for (var index = 0; index < stream.Length; index++)
                {
                    var b = data[index];
                    stringBuilder.Append(b + " ");
                }

                LogHelper.Info($"---收到消息ID:[{messageObject.MessageId}] ==>消息类型:{messageType} 消息内容:{JsonConvert.SerializeObject(messageObject)}  消息字节数组:{stringBuilder}");
            }

            return messageObject;
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