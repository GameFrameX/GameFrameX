using System;
using System.IO;
using ProtoBuf;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 客户端->服务器
    /// </summary>
    [ProtoContract, Serializable]
    public class C2SMessage : CSPacketBase
    {
        /// <summary>
        /// 
        /// </summary>
        [ProtoMember(1)]
        public uint cmd { get; set; }

        /// <summary>
        /// 序列号
        /// </summary>
        [ProtoMember(2)]
        public int index { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [ProtoMember(3)]
        public byte[] data { get; set; }

        public override void Clear()
        {
        }

        [ProtoIgnore] public override int Id { get; }

        // public override int PacketSize()
        // {
        //     using (MemoryStream stream = new MemoryStream())
        //     {
        //         ProtoBuf.Serializer.Serialize(stream, this);
        //         return stream.ToArray().Length;
        //     }
        // }
    }
}