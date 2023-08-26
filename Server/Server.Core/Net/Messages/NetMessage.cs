using System.Buffers;

namespace Server.Core.Net.Messages
{
    /// <summary>
    /// net message
    /// </summary>
    public readonly struct NetMessage
    {
        public ReadOnlySequence<byte> Payload { get; }

        public NetMessage(ReadOnlySequence<byte> payload)
        {
            Msg = null;
            Payload = payload;
        }

        public Message Msg { get; }

        public NetMessage(Message msg)
        {
            Msg = msg;
            Payload = default;
        }

        public void Serialize(IBufferWriter<byte> writer)
        {
            Server.Serialize.Serialize.SerializerHelper.Serialize(writer, Msg);
        }

        public byte[] Serialize()
        {
            try
            {
                return Server.Serialize.Serialize.SerializerHelper.Serialize(Msg);
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}