using MessagePack;

namespace Server.Core.Net.Messages
{
    public abstract class Message
    {
        /// <summary>
        /// 消息唯一id
        /// </summary>
        [IgnoreMember]
        public int UniId { get; set; }

        [IgnoreMember] 
        public int MsgId { get; set; }
    }
}