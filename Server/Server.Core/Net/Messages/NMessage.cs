using System.Buffers;
using MessagePack;

namespace Server.Core.Net.Messages
{
    /// <summary>
    /// net message
    /// </summary>
    public struct NMessage
    {
        public ReadOnlySequence<byte> Payload { get; }

        public NMessage(ReadOnlySequence<byte> payload)
        {
            Msg = null;
            Payload = payload;
        }

        public Message Msg { get; }

        public NMessage(Message msg)
        {
            Msg = msg;
            Payload = default;
        }

        public void Serialize(IBufferWriter<byte> writer)
        {
            MessagePackSerializer.Serialize(writer, Msg);
        }

        public byte[] Serialize()
        {
            try
            {
                return MessagePackSerializer.Serialize(Msg);
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}