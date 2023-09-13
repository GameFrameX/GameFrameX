using Microsoft.AspNetCore.Connections;
using Server.Core.Net.Bedrock.Protocols;
using Server.Core.Net.Messages;

namespace Server.Core.Net.Tcp.Codecs
{
    /// <summary>
    /// 心跳状态
    /// </summary>
    public sealed class HeartBeatStatus
    {
        private float m_HeartBeatElapseSeconds;
        private int m_MissHeartBeatCount;

        public HeartBeatStatus()
        {
            m_HeartBeatElapseSeconds = 0f;
            m_MissHeartBeatCount = 0;
        }

        /// <summary>
        /// 心跳间隔时长
        /// </summary>
        public float HeartBeatElapseSeconds
        {
            get => m_HeartBeatElapseSeconds;
            set => m_HeartBeatElapseSeconds = value;
        }

        /// <summary>
        /// 心跳丢失次数
        /// </summary>
        public int MissHeartBeatCount
        {
            get => m_MissHeartBeatCount;
            set => m_MissHeartBeatCount = value;
        }

        /// <summary>
        /// 重置心跳数据=>保活
        /// </summary>
        /// <param name="resetHeartBeatElapseSeconds">是否重置心跳流逝时长</param>
        public void Reset(bool resetHeartBeatElapseSeconds = true)
        {
            if (resetHeartBeatElapseSeconds)
            {
                m_HeartBeatElapseSeconds = 0f;
            }

            m_MissHeartBeatCount = 0;
        }
    }

    public class NetChannel
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public const string SESSIONID = "SESSIONID";
        public ConnectionContext Context { get; protected set; }
        public ProtocolReader Reader { get; protected set; }
        protected ProtocolWriter Writer { get; set; }

        /// <summary>
        /// 心跳状态
        /// </summary>
        public HeartBeatStatus HeartBeatStatus { get; }

        public IProtocal<NetMessage> Protocol { get; protected set; }

        public NetChannel(ConnectionContext context, IProtocal<NetMessage> protocal)
        {
            Context = context;
            HeartBeatStatus = new HeartBeatStatus();
            Reader = context.CreateReader();
            Writer = context.CreateWriter();
            Protocol = protocal;
            Context.ConnectionClosed.Register(ConnectionClosed);
        }

        protected virtual void ConnectionClosed()
        {
            Reader = null;
            Writer = null;
        }

        public void RemoveSessionId()
        {
            Context.Items.Remove(SESSIONID);
        }

        public bool IsClose()
        {
            return Reader == null || Writer == null;
        }

        public void SetSessionId(long id)
        {
            Context.Items[SESSIONID] = id;
        }

        public long GetSessionId()
        {
            if (Context.Items.TryGetValue(SESSIONID, out var idObj))
                return (long)idObj;
            return 0;
        }

        public void Abort()
        {
            Context.Abort();
            Reader = null;
            Writer = null;
        }

        public async ValueTask WriteAsync(NetMessage msg)
        {
            if (Writer != null)
            {
                await Writer.WriteAsync(Protocol, msg);
                HeartBeatStatus.Reset(true);
            }
        }

        public void WriteAsync(MessageObject msg)
        {
            _ = WriteAsync(new NetMessage(msg));
        }
    }
}