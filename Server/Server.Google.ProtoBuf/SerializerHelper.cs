using System;
using System.Buffers;
using System.IO;
using ProtoBuf.Meta;

namespace ProtoBuf
{
    /// <summary>
    /// 序列化帮助类
    /// </summary>
    public static class SerializerHelper
    {
        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="bufferWriter"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        public static void Serialize<T>(ref IBufferWriter<byte> bufferWriter, T value)
        {
            RuntimeTypeModel.Default.Serialize(bufferWriter, value);
        }

        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static byte[] Serialize<T>(T value)
        {
            using (var memoryStream = new MemoryStream())
            {
                RuntimeTypeModel.Default.Serialize(memoryStream, value);
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="value"></param>
        /// <param name="prefixStyle"></param>
        /// <param name="fieldNumber"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static byte[] SerializeWithLengthPrefix<T>(T value, PrefixStyle prefixStyle, int fieldNumber)
        {
            using (var memoryStream = new MemoryStream())
            {
                RuntimeTypeModel.Default.SerializeWithLengthPrefix(memoryStream, value, typeof(T), prefixStyle, fieldNumber);
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// 注册类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Register<T>()
        {
            RuntimeTypeModel.Default.Add<T>();
        }

        /// <summary>
        /// 注册类型
        /// </summary>
        /// <param name="type"></param>
        public static void Register(Type type)
        {
            RuntimeTypeModel.Default.Add(type);
        }

        /// <summary>
        /// 反序列化数据对象
        /// </summary>
        /// <param name="data">数据</param>
        /// <typeparam name="T">类型</typeparam>
        /// <returns></returns>
        public static T Deserialize<T>(byte[] data)
        {
            using (var memoryStream = new MemoryStream(data))
            {
                return (T)RuntimeTypeModel.Default.Deserialize(typeof(T), memoryStream);
            }
        }

        /// <summary>
        /// 反序列化数据对象
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static object Deserialize(byte[] data, Type type)
        {
            using (var memoryStream = new MemoryStream(data))
            {
                return RuntimeTypeModel.Default.Deserialize(type, memoryStream);
            }
        }

        /// <summary>
        /// 反序列化数据对象
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static object Deserialize(ref ReadOnlySequence<byte> data, Type type)
        {
            return RuntimeTypeModel.Default.Deserialize(type, data);
        }

        /// <summary>
        /// 反序列化数据对象
        /// </summary>
        /// <param name="data">数据</param>
        /// <typeparam name="T">类型</typeparam>
        /// <returns></returns>
        public static T Deserialize<T>(ref ReadOnlySequence<byte> data)
        {
            return RuntimeTypeModel.Default.Deserialize<T>(data);
        }

        
        /// <summary>
        /// 反序列化数据对象
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static object Deserialize(ref ReadOnlyMemory<byte> data, Type type)
        {
            return RuntimeTypeModel.Default.Deserialize(type, data);
        }

        /// <summary>
        /// 反序列化数据对象
        /// </summary>
        /// <param name="data">数据</param>
        /// <typeparam name="T">类型</typeparam>
        /// <returns></returns>
        public static T Deserialize<T>(ref ReadOnlyMemory<byte> data)
        {
            return RuntimeTypeModel.Default.Deserialize<T>(data);
        }
        
        /// <summary>
        /// 反序列化数据对象
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static object Deserialize(ref ReadOnlySpan<byte> data, Type type)
        {
            return RuntimeTypeModel.Default.Deserialize(type, data);
        }

        /// <summary>
        /// 反序列化数据对象
        /// </summary>
        /// <param name="data">数据</param>
        /// <typeparam name="T">类型</typeparam>
        /// <returns></returns>
        public static T Deserialize<T>(ref ReadOnlySpan<byte> data)
        {
            return RuntimeTypeModel.Default.Deserialize<T>(data);
        }
        
        /// <summary>
        /// 反序列化数据对象
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="instance">实例对象</param>
        /// <param name="prefixStyle"></param>
        /// <param name="fieldNumber"></param>
        /// <typeparam name="T">类型</typeparam>
        /// <returns></returns>
        public static T DeserializeWithLengthPrefix<T>(byte[] data, T instance, PrefixStyle prefixStyle, int fieldNumber)
        {
            using (var memoryStream = new MemoryStream(data))
            {
                return (T)RuntimeTypeModel.Default.DeserializeWithLengthPrefix(memoryStream, instance, typeof(T), prefixStyle, fieldNumber);
            }
        }
    }
}