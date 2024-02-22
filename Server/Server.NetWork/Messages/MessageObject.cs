

namespace Server.NetWork.Messages
{
    public abstract class MessageObject : IMessage
    {
        /// <summary>
        /// 消息唯一id
        /// </summary>
        public int UniId { get; set; }

        /// <summary>
        /// 消息ID
        /// </summary>
        public int MsgId { get; set; }
    }
}