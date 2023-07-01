using System.Buffers;
using ProtoBuf.Meta;

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
            MemoryStream memoryStream = new MemoryStream(writer.GetMemory().ToArray());
            RuntimeTypeModel.Default.Serialize(memoryStream, Msg);
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