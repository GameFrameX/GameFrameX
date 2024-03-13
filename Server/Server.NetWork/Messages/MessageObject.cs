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
        [JsonIgnore]
        public int UniId { get; set; }

        /// <summary>
        /// 消息ID
        /// </summary>
        [ProtoMember(998)]
        public int MessageId { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// 消息的唯一ID
        /// </summary>
        [ProtoMember(999)]
        public long UniqueId { get; set; }

        public MessageObject()
        {
            UniqueId = Utility.UtilityIdGenerator.GetNextUniqueId();
        }
    }
}