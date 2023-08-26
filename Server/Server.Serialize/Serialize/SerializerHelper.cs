using System.Buffers;
using MessagePack;

namespace Server.Serialize.Serialize
{
    public static class SerializerHelper
    {
        public static byte[] Serialize<T>(T value)
        {
            return MessagePackSerializer.Serialize(value);
        }

        public static void Serialize<T>(IBufferWriter<byte> writer,
            T value,
            MessagePackSerializerOptions? options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            MessagePackSerializer.Serialize(writer, value, options, cancellationToken);
        }

        public static T Deserialize<T>(byte[] data)
        {
            return MessagePackSerializer.Deserialize<T>(data);
        }

        public static T Deserialize<T>(
            in ReadOnlySequence<byte> byteSequence,
            MessagePackSerializerOptions? options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return MessagePackSerializer.Deserialize<T>(byteSequence, options, cancellationToken);
        }
    }
}