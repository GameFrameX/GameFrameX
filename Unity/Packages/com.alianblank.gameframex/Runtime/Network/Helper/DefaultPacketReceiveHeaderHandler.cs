using System;
using System.Buffers;
using MessagePack;

namespace GameFrameX.Network
{
    public sealed class DefaultPacketReceiveHeaderHandler : IPacketReceiveHeaderHandler, IPacketHandler
    {
        public int PacketLength { get; private set; }
        public int Id { get; private set; }


        public bool Handler(object source)
        {
            ReadOnlySequence<byte> sequence = (ReadOnlySequence<byte>)source;
            var reader = new SequenceReader<byte>(sequence);

            // packetLength
            reader.TryReadBigEndian(out int packetLength); //4
            PacketLength = packetLength;
            // timestamp
            reader.TryReadBigEndian(out long timestamp); //8
            // MsgId
            reader.TryReadBigEndian(out int msgId); //4
            Id = msgId;
            return true;
        }

        /// <summary>
        /// 网络包长度
        /// </summary>
        private const int NetPacketLength = 4;

        // 消息码
        private const int NetCmdIdLength = 4;

        // 消息时间戳
        private const int NetTicketLength = 8;

        public DefaultPacketReceiveHeaderHandler()
        {
            PacketHeaderLength = NetPacketLength + NetTicketLength + NetCmdIdLength;
        }

        public int PacketHeaderLength { get; }
    }
}