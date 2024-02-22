using System.Buffers;
using System.IO.Pipelines;
using Microsoft.AspNetCore.Connections;
using Newtonsoft.Json;
using Server.Extension;
using Server.NetWork.Messages;
using Server.Serialize.Serialize;
using Server.Setting;
using Server.Utility;

namespace Server.NetWork.TCPSocket
{
    public sealed class TcpChannel : BaseNetChannel
    {
        static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public ConnectionContext Context { get; protected set; }
        private PipeReader Reader { get; set; }
        private PipeWriter Writer { get; set; }

        private Action<MessageObject> onMessage;

        private long lastReviceTime = 0;
        private int lastOrder = 0;
        const int MAX_RECV_SIZE = 1024 * 1024 * 5;

        IMessageHelper TypeGetter { get; set; }

        /// 从客户端接收的包大小最大值（单位：字节 5M）
        public TcpChannel(ConnectionContext context, IMessageHelper messageHandler, Action<MessageObject> onMessage = null) : base(messageHandler)
        {
            TypeGetter = messageHandler;
            this.onMessage = onMessage;
            Context = context;
            Reader = context.Transport.Input;
            Writer = context.Transport.Output;
            RemoteAddress = context.RemoteEndPoint?.ToString();
        }

        public override async Task StartAsync()
        {
            try
            {
                var cancelToken = CloseSrc.Token;
                while (!cancelToken.IsCancellationRequested)
                {
                    var result = await Reader.ReadAsync(cancelToken);
                    var buffer = result.Buffer;
                    if (buffer.Length > 0)
                    {
                        while (TryParseMessage(ref buffer))
                        {
                        }

                        Reader.AdvanceTo(buffer.Start, buffer.End);
                    }
                    else if (result.IsCanceled || result.IsCompleted)
                    {
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }
        }

        private bool TryParseMessage(ref ReadOnlySequence<byte> input)
        {
            var bufEnd = input.End;
            var reader = new SequenceReader<byte>(input);

            if (!reader.TryReadBigEndian(out int msgLen))
            {
                return false;
            }

            if (!CheckMsgLen(msgLen))
            {
                throw new Exception("消息长度异常");
            }

            if (reader.Remaining < msgLen - 4)
            {
                return false;
            }

            var payload = input.Slice(reader.Position, msgLen - 4);


            reader.TryReadBigEndian(out long time);
            if (!CheckTime(time))
            {
                throw new Exception("消息接收时间错乱");
            }

            reader.TryReadBigEndian(out int order);
            if (!CheckMagicNumber(order, msgLen))
            {
                throw new Exception("消息order错乱");
            }

            reader.TryReadBigEndian(out int msgId);

            var msgType = TypeGetter.MessageTypeByIdGetter(msgId);
            if (msgType == null)
            {
                Logger.Error("消息ID:{} 找不到对应的Msg.", msgId);
            }
            else
            {
                var message = (MessageObject)SerializerHelper.Deserialize(reader.UnreadSequence, msgType);
                message.MsgId = msgId;
                if (message.MsgId != msgId)
                {
                    throw new Exception($"解析消息错误，注册消息id和消息无法对应.real:{message.MsgId}, register:{msgId}");
                }

                onMessage(message);
            }

            input = input.Slice(input.GetPosition(msgLen));
            return true;
        }

        public bool CheckMagicNumber(int order, int msgLen)
        {
            order ^= 0x1234 << 8;
            order ^= msgLen;

            if (lastOrder != 0 && order != lastOrder + 1)
            {
                Logger.Error("包序列出错, order=" + order + ", lastOrder=" + lastOrder);
                return false;
            }

            lastOrder = order;
            return true;
        }

        /// <summary>
        /// 检查消息长度是否合法
        /// </summary>
        /// <param name="msgLen"></param>
        /// <returns></returns>
        public bool CheckMsgLen(int msgLen)
        {
            //消息长度+时间戳+magic+消息id+数据
            //4 + 8 + 4 + 4 + data
            if (msgLen <= 16) //(消息长度已经被读取)
            {
                Logger.Error("从客户端接收的包大小异常:" + msgLen + ":至少16个字节");
                return false;
            }
            else if (msgLen > MAX_RECV_SIZE)
            {
                Logger.Error("从客户端接收的包大小超过限制：" + msgLen + "字节，最大值：" + MAX_RECV_SIZE / 1024 + "字节");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 时间戳检查(可以防止客户端游戏过程中修改时间)
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool CheckTime(long time)
        {
            if (lastReviceTime > time)
            {
                Logger.Error("时间戳出错，time=" + time + ", lastTime=" + lastReviceTime);
                return false;
            }

            lastReviceTime = time;
            return true;
        }

        public override void Write(IMessage msg)
        {
            if (IsClose())
                return;
            var bytes = SerializerHelper.Serialize(msg);

            // len +timestamp + msgId + bytes.length
            int len = 4 + 4 + 8 + bytes.Length;
            var span = Writer.GetSpan(len);
            int offset = 0;
            span.WriteInt(len, ref offset);
            span.WriteLong(TimeHelper.UnixTimeSeconds(), ref offset);
            var messageType = msg.GetType();
            var msgId = TypeGetter.MessageIdByTypeGetter(messageType);
            span.WriteInt(msgId, ref offset);
            span.WriteBytesWithoutLength(bytes, ref offset);
            Writer.Advance(len);
            _ = Writer.FlushAsync(CloseSrc.Token);
            if (GlobalSettings.IsDebug)
            {
                Logger.Info($"---发送消息ID:[{msgId}] ==>消息类型:{messageType} 消息内容:{JsonConvert.SerializeObject(msg)}");
            }
        }

        public override void WriteAsync(IMessage msg, int uniId, int code, string desc = "")
        {
            if (msg is MessageObject messageObject)
            {
                messageObject.UniId = uniId;
                Write(messageObject);
            }

            if (uniId > 0)
            {
                // TODO 这个要想别的办法实现
                // RespErrorCode res = new RespErrorCode
                // {
                //     // UniId = uniId,
                //     ErrCode = (int)code,
                //     Desc = desc
                // };
                // Write(res);
            }
        }
    }
}