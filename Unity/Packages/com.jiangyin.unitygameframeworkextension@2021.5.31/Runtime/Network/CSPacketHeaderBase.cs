//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 客户端发到服务器头
    /// </summary>
    public sealed class CSPacketHeaderBase : PacketHeaderBase
    {
        /// <summary>
        /// 客户端发到服务器
        /// </summary>
        public override PacketType PacketType => PacketType.ClientToServer;

        public override int Id { get; set; }
    }
}