using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using ProtoBuf.Meta;
using UnityEngine;

namespace ProtoBuf.Runtime
{
    public static class ProtobufHelper
    {
        public static object FromBytes(Type type, byte[] bytes)
        {
            if (bytes == null)
            {
                Debug.Assert(false, "数据为空？？？");
                return null;
            }

            using (MemoryStream stream = new MemoryStream(bytes, 0, bytes.Length))
            {
                object o = RuntimeTypeModel.Default.Deserialize(stream, null, type);
                if (o is ISupportInitialize supportInitialize)
                {
                    supportInitialize.EndInit();
                }

                return o;
            }
        }

        public static object FromBytes(Type type, byte[] bytes, int index, int count)
        {
            using (MemoryStream stream = new MemoryStream(bytes, index, count))
            {
                object o = RuntimeTypeModel.Default.Deserialize(stream, null, type);
                if (o is ISupportInitialize supportInitialize)
                {
                    supportInitialize.EndInit();
                }

                return o;
            }
        }

        public static string ToString(object message)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                RuntimeTypeModel.Default.Serialize(stream, message);
                return Encoding.Unicode.GetString(stream.ToArray());
            }
        }

        public static byte[] ToBytes(object message)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                RuntimeTypeModel.Default.Serialize(stream, message);
                return stream.ToArray();
            }
        }

        public static void ToStream(object message, MemoryStream stream)
        {
            RuntimeTypeModel.Default.Serialize(stream, message);
        }

        public static object FromStream(Type type, MemoryStream stream)
        {
            object o = RuntimeTypeModel.Default.Deserialize(stream, null, type);
            if (o is ISupportInitialize supportInitialize)
            {
                supportInitialize.EndInit();
            }

            return o;
        }
    }
}