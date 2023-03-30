using System;

namespace UnityGameFramework.Runtime
{
    public class Java_SCPacketHeader : PacketHeaderBase
    {
        public override PacketType PacketType
        {
            get { return PacketType.ServerToClient; }
        }
    }
}