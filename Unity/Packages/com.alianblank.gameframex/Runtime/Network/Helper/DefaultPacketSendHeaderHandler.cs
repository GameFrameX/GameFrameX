using System;
using System.Buffers;
using System.IO;
using GameFrameX.Runtime;
using MessagePack;

namespace GameFrameX.Network
{
    public sealed class DefaultPacketSendHeaderHandler : IPacketSendHeaderHandler, IPacketHandler
    {
        /// <summary>
        /// 网络包长度
        /// </summary>
        private const int NetPacketLength = 4;

        // 消息码
        private const int NetCmdIdLength = 4;

        // 排序
        private const int NetOrderLength = 4;

        // 消息时间戳
        private const int NetTicketLength = 8;


        public DefaultPacketSendHeaderHandler()
        {
            // 4 + 8 + 4 + 4 
            PacketHeaderLength = NetPacketLength + NetTicketLength + NetOrderLength + NetCmdIdLength;
            _cachedByte = new byte[PacketHeaderLength];
        }

        /// <summary>
        /// 消息包头长度
        /// </summary>
        public int PacketHeaderLength { get; }

        /// <summary>
        /// 获取网络消息包协议编号。
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// 获取网络消息包长度。
        /// </summary>
        public int PacketLength { get; private set; }

        private const int Magic = 0x1234;

        int count = 0;
        private readonly byte[] _cachedByte;

        public bool Handler<T>(T messageObject, MemoryStream cachedStream, out byte[] messageBodyBuffer) where T : MessageObject
        {
            cachedStream.Seek(0, SeekOrigin.Begin);
            cachedStream.SetLength(0);


            messageBodyBuffer = MessagePackSerializer.Serialize(messageObject);
            var messageType = messageObject.GetType();
            Id = ProtoMessageIdHandler.GetReqMessageIdByType(messageType);
            var messageLength = messageBodyBuffer.Length;
            PacketLength = PacketHeaderLength + messageLength;
            int magic = Magic + ++count;
            magic ^= Magic << 8;
            magic ^= PacketLength;

            int offset = 0;

            _cachedByte.WriteInt(PacketLength, ref offset);
            _cachedByte.WriteLong(GameTimeHelper.UnixTimeMilliseconds(), ref offset);
            _cachedByte.WriteInt(magic, ref offset);
            _cachedByte.WriteInt(Id, ref offset);

            cachedStream.Write(_cachedByte, 0, PacketHeaderLength);
            return true;
        }
    }
}