//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2019 Jiang Yin. All rights reserved.
// Homepage: http://gameframework.cn/
// Feedback: mailto:jiangyin@gameframework.cn
//------------------------------------------------------------

// using ProtoBuf;

namespace GameFrameX.Runtime
{
    // [ProtoContract]
    public partial class CSHeartBeat : CSPacketBase
    {
        /// <summary>
        /// 时间戳
        /// </summary>
        // [ProtoMember(1)]
        public long Timestamp { get; set; }

        public override int Id => 1;
    }
}