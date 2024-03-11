using Newtonsoft.Json;
using ProtoBuf;

namespace Server.NetWork.Messages
{
    [ProtoContract]
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

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
        [ProtoMember(999)]
        public string UniqueId { get; set; }

        public MessageObject()
        {
            UniqueId = Guid.NewGuid().ToString("N");
        }
    }
}