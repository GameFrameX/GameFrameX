using ProtoBuf.Meta;

namespace Server.Google.ProtoBuf
{
    public static class SerializerHelper
    {
        public static byte[] Serialize<T>(T value)
        {
            using MemoryStream memoryStream = new MemoryStream();

            RuntimeTypeModel.Default.Serialize(memoryStream, value);
            return memoryStream.ToArray();
        }

        public static T Deserialize<T>(byte[] data)
        {
            using MemoryStream memoryStream = new MemoryStream(data);
            return (T) RuntimeTypeModel.Default.Deserialize(memoryStream, null!, typeof(T));
        }
    }
}