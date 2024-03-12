using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets.Kcp;
using Newtonsoft.Json;

using ProtoBuf;
using Server.Extension;
using Server.Log;
using Server.NetWork;
using Server.NetWork.Messages;
using Server.Utility;

namespace NetWork
{
    public class KcpChannel : BaseNetChannel, IKcpCallback
    {
        public delegate void KcpOutPutFunc(BaseNetChannel chann, ReadOnlySpan<byte> data);

        public EndPoint routerEndPoint { get; set; }

        private KcpOutPutFunc kcpDataSendFunc;
        private Func<KcpChannel, MessageObject, Task> onMessageAct;
        private Action onClose;
        private SimpleSegManager.Kcp kcp;
        private bool isColose = false;
        private Pipe dataPipe;

        CancellationTokenSource closeToenSrc = new();

        const int MAX_RECV_SIZE = 1024 * 1024 * 20;
        object writeLockObj = new object();

        public KcpChannel(bool isInner, long id, int serverId, EndPoint routerEndPoint, KcpOutPutFunc kcpSocketSendAct, Func<KcpChannel, MessageObject, Task> onMessageAct, Action onClose = null)
            : base(null)
        {
            NetId = id;
            var ipEndPoint = routerEndPoint as IPEndPoint;
            this.routerEndPoint = new IPEndPoint(ipEndPoint.Address, ipEndPoint.Port);
            kcp = new SimpleSegManager.Kcp((uint)id, this);
            kcp.NoDelay(1, 10, 2, 1);
            kcp.WndSize(128, 128);
            kcp.SetMtu(isInner ? 1400 : 520);
            dataPipe = new Pipe(PipeOptions.Default);
            kcpDataSendFunc = kcpSocketSendAct;
            this.onMessageAct = onMessageAct;
            TargetServerId = serverId;
            this.onClose = onClose;
            UpdateReceiveMessageTime(TimeSpan.FromSeconds(5).Ticks);
            _ = StartProcessMsgAsync();
        }


        bool TryParseMessage(ref ReadOnlySequence<byte> input, out MessageObject message)
        {
            message = default;
            var reader = new SequenceReader<byte>(input);

            if (!reader.TryReadBigEndian(out int msgLen))
            {
                return false;
            }

            if (msgLen <= 8) //(消息长度已经被读取)
            {
                throw new Exception($"从客户端接收的包大小异常,{msgLen} {reader.Remaining}:至少大于8个字节");
            }
            else if (msgLen > MAX_RECV_SIZE)
            {
                throw new Exception("从客户端接收的包大小超过限制：" + msgLen + "字节，最大值：" + MAX_RECV_SIZE / 1024 + "字节");
            }

            if (reader.Remaining < msgLen - 4)
            {
                return false;
            }

            reader.TryReadBigEndian(out int msgId); //4   
            var payload = input.Slice(reader.Position, msgLen - 8);
            message = SerializerHelper.Deserialize<MessageObject>(ref payload);
            input = input.Slice(msgLen);
            return true;
        }


        async Task StartProcessMsgAsync()
        {
            var cancelToken = closeToenSrc.Token;
            while (!isColose)
            {
                try
                {
                    var reader = dataPipe.Reader;
                    var result = await reader.ReadAsync(cancelToken);
                    var buffer = result.Buffer;

                    if (buffer.Length > 0)
                    {
                        while (TryParseMessage(ref buffer, out var msg))
                        {
                            if (msg != null)
                            {
#if DEBUG
                                LogHelper.Info($"收到消息:{msg.GetType().Name}:{JsonConvert.SerializeObject(msg)}");
#endif
                                if (onMessageAct != null)
                                    await onMessageAct(this, msg);
                            }
                        }

                        reader.AdvanceTo(buffer.Start, buffer.End);
                    }
                    else if (result.IsCanceled || result.IsCompleted)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e.Message);
                    break;
                }
            }
        }

        public override void Close()
        {
            //LOGGER.Warn($"kcpchannel close:{NetId}"); 
            lock (this)
            {
                isColose = true;
                closeToenSrc?.Cancel();
                onClose?.Invoke();
                onClose = null;
            }
        }

        public override bool IsClose()
        {
            return isColose;
        }

        public override void WriteAsync(IMessage msg, int uniId, int code, string desc = "")
        {
            Guard.NotNull(msg, nameof(msg));
            if (isColose)
            {
                return;
            }

            MessageObject messageObject = msg as MessageObject;
            var bytes = SerializerHelper.Serialize(msg);
            var headSize = 8;
            Span<byte> span = stackalloc byte[headSize + bytes.Length];
            int len = headSize + bytes.Length;
            int offset = 0;
            span.Write(len, ref offset);
            span.Write(messageObject.MessageId, ref offset);
            bytes.CopyTo(span[headSize..]);
            lock (writeLockObj)
            {
                kcp?.Send(span);
            }
        }

        public void Write([NotNull] MessageObject msg)
        {
            if (isColose)
                return;
#if DEBUG
            LogHelper.Info($"发送{msg.GetType()}:{JsonConvert.SerializeObject(msg)}");
#endif
            var bytes = SerializerHelper.Serialize(msg);

            var headSize = 8;
            Span<byte> span = stackalloc byte[headSize + bytes.Length];
            int len = headSize + bytes.Length;
            int offset = 0;
            span.Write(len, ref offset);
            span.Write(msg.MessageId, ref offset);
            bytes.CopyTo(span[headSize..]);
            lock (writeLockObj)
            {
                kcp?.Send(span);
            }
        }

        public void HandleRecv(ReadOnlySpan<byte> data)
        {
            if (isColose)
            {
                return;
            }

            UpdateReceiveMessageTime();
            //LOGGER.Error($"kcp intput...");
            kcp.Input(data);
        }

        public void Output(IMemoryOwner<byte> buffer, int avalidLength)
        {
            //LOGGER.Error($"kcp output:{avalidLength}");
            kcpDataSendFunc(this, buffer.Memory.Span[..avalidLength]);
            buffer.Dispose();
        }

        public void Update(in DateTime time)
        {
            if (isColose)
                return;
            var lastMsgTime = GetLastMessageTimeSecond(time);
            if (lastMsgTime > 60_0) //10分钟没数据则关闭  kcp需要保留一段时间channel，客户端重连后如果老channel还在，会尝试继续通信
            {
                Close();
                return;
            }

            if (lastMsgTime > 10)
            {
                return;
            }

            var (buffer, avalidLength) = kcp.TryRecv();
            if (buffer != null && avalidLength > 0)
            {
                UpdateReceiveMessageTime();
                var writer = dataPipe.Writer;
                writer.Write(buffer.Memory.Span[..avalidLength]);
                writer.FlushAsync();
            }

            kcp.Update(time);
        }
    }
}