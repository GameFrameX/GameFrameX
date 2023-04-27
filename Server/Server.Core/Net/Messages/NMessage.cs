using System.Buffers;
using ProtoBuf.Meta;

namespace Server.Core.Net.Messages
{
    /// <summary>
    /// net message
    /// </summary>
    public struct NMessage
    {
        public ReadOnlySequence<byte> Payload { get; } = default;

        public NMessage(ReadOnlySequence<byte> payload)
        {
            Payload = payload;
        }

        public Message Msg { get; } = null;

        public NMessage(Message msg)
        {
            Msg = msg;
        }

        public void Serialize(IBufferWriter<byte> writer)
        {
            // RuntimeTypeModel.Default.Serialize(writer.GetMemory(), Msg);
        }

        public byte[] Serialize()
        {
            try
            {
                using MemoryStream memoryStream = new MemoryStream();
                RuntimeTypeModel.Default.Serialize(memoryStream, Msg);
                return memoryStream.ToArray();
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}