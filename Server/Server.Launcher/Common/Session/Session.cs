using Server.Core.Net;
using Server.Core.Net.Messages;
using Server.Core.Net.Tcp.Codecs;

namespace Server.Launcher.Common.Session
{
    public class Session
    {
        /// <summary>
        /// 全局标识符
        /// </summary>
        public long Id { get; }

        public Session(long id)
        {
            Id = id;
            CreateTime = DateTime.Now;
        }

        /// <summary>
        /// 连接时间
        /// </summary>
        public DateTime CreateTime { get; }

        /// <summary>
        /// 连接上下文
        /// </summary>
        public NetChannel Channel { get; set; }

        /// <summary>
        /// 连接标示，避免自己顶自己的号,客户端每次启动游戏生成一次/或者每个设备一个
        /// </summary>
        public string Sign { get; set; }

        public void WriteAsync(MessageObject msg)
        {
            Channel?.Write(msg);
        }
    }
}