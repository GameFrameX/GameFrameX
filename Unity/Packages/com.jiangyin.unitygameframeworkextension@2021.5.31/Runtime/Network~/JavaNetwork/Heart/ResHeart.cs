using ProtoBuf;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 心跳返回
    /// </summary>
    [ProtoContract]
    public partial class ResHeart
    {
        /// <summary>
        /// 
        /// </summary>
        [ProtoMember(1)]
        public long timestamp { get; set; }
    }
}