//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using Newtonsoft.Json;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 客户端发到服务器内容
    /// </summary>
    public abstract class CSPacketBase : PacketBase
    {
        /// <summary>
        /// 客户端发到服务器
        /// </summary>
        [JsonIgnore]
        public override PacketType PacketType => PacketType.ClientToServer;
    }
}