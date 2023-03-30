//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using GameFramework.Network;
using ProtoBuf;

namespace UnityGameFramework.Runtime
{
    public abstract class PacketHeaderBase : IPacketHeader, IReference
    {
        public abstract PacketType PacketType { get; }

        // public int Id
        // {
        //     get;
        //     set;
        // }
        public virtual int PacketLength { get; set; }
        public virtual byte Bit { get; set; }

        public bool IsValid
        {
            get { return PacketType != PacketType.Undefined && PacketLength >= 0; }
        }

        public void Clear()
        {
            PacketLength = 0;
        }
    }
}