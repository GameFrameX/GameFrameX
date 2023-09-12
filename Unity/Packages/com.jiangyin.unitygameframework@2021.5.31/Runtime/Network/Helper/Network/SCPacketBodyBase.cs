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
    /// 消息内容基类
    /// </summary>
    public abstract class SCPacketBodyBase : PacketBase
    {
        [JsonIgnore] public override PacketType PacketType => PacketType.ServerToClient;
    }
}