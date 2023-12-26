//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System.Buffers;

namespace GameFrameX.Network
{
    /// <summary>
    /// 网络消息包头接口。
    /// </summary>
    public interface IPacketReceiveHeaderHandler
    {
        /// <summary>
        /// 消息包头长度
        /// </summary>
        int PacketHeaderLength { get; }

        /// <summary>
        /// 获取网络消息包长度。
        /// </summary>
        int PacketLength { get; }

        /// <summary>
        /// 获取网络消息包协议编号。
        /// </summary>
        int Id { get; }

        bool Handler(object source);
    }
}