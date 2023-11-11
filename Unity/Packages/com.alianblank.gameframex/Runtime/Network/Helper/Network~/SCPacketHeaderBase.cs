//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

namespace GameFrameX.Runtime
{
    /// <summary>
    /// 消息头基类
    /// </summary>
    public sealed class SCPacketHeaderBase : PacketHeaderBase
    {
        /// <summary>
        /// 服务器发到客户端数据
        /// </summary>
        public override PacketType PacketType => PacketType.ServerToClient;

        private static int Number = 0;
        public override int Id { get; set; } = Number++;
    }
}