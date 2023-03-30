using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 服务器->客户端
    /// </summary>
    [ProtoContract, Serializable]
    public class S2CMessage : SCPacketBase
    {
        /// <summary>
        /// 
        /// </summary>
        [ProtoMember(1)]
        public uint cmd { get; set; }

        /// <summary>
        /// 序列号回传。如果是0表示是服务器主动推送的消息
        /// </summary>
        [ProtoMember(2)]
        public int index { get; set; }

        /// <summary>
        /// 如果为0，表示成功返回。其他值表示错误码
        /// </summary>
        [ProtoMember(3)]
        public int error { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [ProtoMember(4)] public List<string> errorParams = new List<string>();

        /// <summary>
        /// 
        /// </summary>
        [ProtoMember(5)]
        public byte[] data { get; set; }

        public override void Clear()
        {
        }

        [ProtoIgnore] public override int Id => (int) cmd;

        public override string ToString()
        {
            return ProtobufHelper.ToString(this);
        }
    }
}