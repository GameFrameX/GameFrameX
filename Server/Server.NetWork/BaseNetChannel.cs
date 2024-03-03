using System.Collections.Concurrent;
using Server.NetWork.Messages;

namespace Server.NetWork
{
    public abstract class BaseNetChannel : INetChannel
    {
        public long NetId { get; set; } = 0;
        public int TargetServerId { get; set; }

        public BaseNetChannel()
        {
        }

        public BaseNetChannel(IMessageHelper messageHelper)
        {
            MessageHelper = messageHelper;
        }

        protected readonly CancellationTokenSource CloseSrc = new();

        public virtual void Write(IMessage msg)
        {
        }

        public virtual void Close()
        {
            CloseSrc.Cancel();
        }

        public virtual bool IsClose()
        {
            return CloseSrc.IsCancellationRequested;
        }

        public IMessageHelper MessageHelper { get; }
        public virtual string RemoteAddress { get; set; } = "";

        public virtual Task StartAsync()
        {
            return Task.CompletedTask;
        }


        readonly ConcurrentDictionary<string, object> datas = new();

        public T GetData<T>(string key)
        {
            if (datas.TryGetValue(key, out var v))
            {
                return (T)v;
            }

            return default;
        }

        public void RemoveData(string key)
        {
            datas.Remove(key, out _);
        }

        public void SetData(string key, object v)
        {
            datas[key] = v;
        }

        protected long LastReceiveMessageTime;

        /// <summary>
        /// 更新接收消息的时间
        /// </summary>
        /// <param name="offsetTicks"></param>
        public void UpdateReceiveMessageTime(long offsetTicks = 0)
        {
            LastReceiveMessageTime = DateTime.UtcNow.Ticks + offsetTicks;
        }

        /// <summary>
        /// 获取最后接收消息到现在的时间。单位秒
        /// </summary>
        /// <param name="utcTime"></param>
        /// <returns></returns>
        public long GetLastMessageTimeSecond(in DateTime utcTime)
        {
            return (utcTime.Ticks - LastReceiveMessageTime) / 10000_000;
        }

        /// <summary>
        /// 将消息对象异步写入网络通道。
        /// </summary>
        /// <param name="msg">消息对象。</param>
        /// <param name="uniId">唯一ID。</param>
        /// <param name="code">状态码。</param>
        /// <param name="desc">描述。</param>
        public abstract void WriteAsync(IMessage msg, int uniId, int code, string desc = "");
    }
}