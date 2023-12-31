﻿using System;

namespace UnityGameFramework.Runtime
{
    public sealed class CSPacketBeatHeader : PacketHeaderBase
    {
        public override PacketType PacketType => PacketType.ClientToServer;

        public override int Id { get; set; } = 1;
    }
}