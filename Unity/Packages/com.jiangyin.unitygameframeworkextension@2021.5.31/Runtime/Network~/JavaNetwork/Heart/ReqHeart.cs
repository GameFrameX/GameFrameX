using ProtoBuf;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 心跳请求
    /// </summary>
    [ProtoContract]
    public partial class ReqHeart
    {
        /// <summary>
        /// 
        /// </summary>
        [ProtoMember(1)]
        public long timestamp { get; set; }
    }
}