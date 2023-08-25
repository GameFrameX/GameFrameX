using MessagePack;

namespace Server.Serialize.Serialize
{
    public static class SerializerHelper
    {
        public static byte[] Serialize<T>(T value)
        {
            return MessagePackSerializer.Serialize(value);
        }

        public static T Deserialize<T>(byte[] data)
        {
            return MessagePackSerializer.Deserialize<T>(data);
        }
    }
}