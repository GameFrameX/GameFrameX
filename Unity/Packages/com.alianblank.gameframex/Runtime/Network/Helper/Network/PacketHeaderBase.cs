//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFrameX;
using GameFrameX.Network;

namespace GameFrameX.Runtime
{
    public abstract class PacketHeaderBase : IPacketHeader, IReference
    {
        /// <summary>
        /// 数据包类型
        /// </summary>
        public abstract PacketType PacketType { get; }

        /// <summary>
        /// 消息ID
        /// </summary>
        public abstract int Id { get; set; }

        /// <summary>
        /// 数据长度
        /// </summary>
        public virtual int PacketLength { get; set; }

        /// <summary>
        /// 是否是有效数据包
        /// </summary>
        public bool IsValid => PacketType != PacketType.Undefined && PacketLength >= 0;

        public void Clear()
        {
            PacketLength = 0;
        }
    }
}