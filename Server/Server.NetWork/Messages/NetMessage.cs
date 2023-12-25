using System.Buffers;
using Server.Serialize.Serialize;

namespace Server.NetWork.Messages
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

        public MessageObject Msg { get; }

        public NetMessage(MessageObject msg)
        {
            Msg = msg;
            Payload = default;
        }

        public void Serialize(IBufferWriter<byte> writer)
        {
            SerializerHelper.Serialize(writer, Msg);
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