using System;

namespace UnityGameFramework.Runtime
{
    public class Java_CSPacketHeader : PacketHeaderBase
    {
        // /// <summary>
        // /// 加密位
        // /// </summary>
        // public byte Flag;

        public override PacketType PacketType
        {
            get { return PacketType.ClientToServer; }
        }
    }
}