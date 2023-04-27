//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2019 Jiang Yin. All rights reserved.
// Homepage: http://gameframework.cn/
// Feedback: mailto:jiangyin@gameframework.cn
//------------------------------------------------------------


using ProtoBuf;

namespace UnityGameFramework.Runtime
{
    [ProtoContract]
    public partial class SCHeartBeat : SCPacketBodyBase
    {
        public override int Id => 1;

        /// <summary>
        /// 时间戳
        /// </summary>
        [ProtoMember(1)]
        public long Timestamp { get; set; }

        public override void Clear()
        {
        }
    }
}