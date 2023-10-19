//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------


using GameFrameX.Network;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// 数据包基类
    /// </summary>
    public abstract class PacketBase : Packet
    {
        /// <summary>
        /// 数据包类型.
        /// </summary>
        public abstract PacketType PacketType { get; }
    }
}