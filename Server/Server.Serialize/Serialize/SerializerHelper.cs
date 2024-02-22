using System.Buffers;
using ProtoBuf.Meta;

namespace Server.Serialize.Serialize
{
    public static class SerializerHelper
    {
        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static byte[] Serialize<T>(T value)
        {
            return ProtoBuf.SerializerHelper.Serialize<T>(value);
        }

        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="bufferWriter"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        public static void Serialize<T>(IBufferWriter<byte> bufferWriter, T value)
        {
            ProtoBuf.SerializerHelper.Serialize<T>(ref bufferWriter, value);
        }

        public static void Register<T>()
        {
            ProtoBuf.SerializerHelper.Register<T>();
        }

        public static void Register(Type type)
        {
            ProtoBuf.SerializerHelper.Register(type);
        }

        /// <summary>
        /// 反序列化数据对象
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="type">对象类型</param>
        /// <returns></returns>
        public static object Deserialize(byte[] data, Type type)
        {
            return ProtoBuf.SerializerHelper.Deserialize(data, type);
        }

        /// <summary>
        /// 反序列化数据对象
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="type">对象类型</param>
        /// <returns></returns>
        public static object Deserialize(ReadOnlySequence<byte> data, Type type)
        {
            return ProtoBuf.SerializerHelper.Deserialize(ref data, type);
        }
    }
}