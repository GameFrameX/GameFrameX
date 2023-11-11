using System;
using GameFrameX.Network;

namespace GameFrameX.Runtime
{
    public sealed class CSMessagePackage : MessagePackage
    {
        public override PacketType PacketType => PacketType.ClientToServer;
    }

    public sealed class SCMessagePackage : MessagePackage
    {
        public override PacketType PacketType => PacketType.ServerToClient;
    }


    public abstract class MessagePackage : PacketBase
    {
        /// <summary>
        /// 消息码
        /// </summary>
        public int Code { get; set; }
// #if UNITY_EDITOR
        public Type MessageType { get; set; }
// #endif
        /// <summary>
        /// 消息内容
        /// </summary>
        public byte[] Data { get; set; }
        

        private static int _index = 0;
        public override int Id => _index++;
    }
}