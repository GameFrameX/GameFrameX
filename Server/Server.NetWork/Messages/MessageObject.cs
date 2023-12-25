using MessagePack;

namespace Server.NetWork.Messages
{
    public abstract class MessageObject : IMessage
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