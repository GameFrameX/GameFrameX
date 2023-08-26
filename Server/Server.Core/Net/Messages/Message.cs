using MessagePack;

namespace Server.Core.Net.Messages
{
    public abstract class Message : IMessage
    {
        /// <summary>
        /// 消息唯一id
        /// </summary>
        [IgnoreMember]
        public int UniId { get; set; }

        /// <summary>
        /// 消息ID
        /// </summary>
        [IgnoreMember]
        public int MsgId { get; set; }
    }
}