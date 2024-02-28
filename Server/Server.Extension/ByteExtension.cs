using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace Server.Extension
{
    public static class ByteExtension
    {
        public static string ToHex(this byte b)
        {
            return b.ToString("X2");
        }

        public static string ToHex(this byte[] bytes)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte b in bytes)
            {
                stringBuilder.Append(b.ToString("X2"));
            }
            return stringBuilder.ToString();
        }

        public static string ToHex(this byte[] bytes, string format)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte b in bytes)
            {
                stringBuilder.Append(b.ToString(format));
            }
            return stringBuilder.ToString();
        }

        public static string ToHex(this byte[] bytes, int offset, int count)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = offset; i < offset + count; ++i)
            {
                stringBuilder.Append(bytes[i].ToString("X2"));
            }
            return stringBuilder.ToString();
        }

        public static string ToStr(this byte[] bytes)
        {
            return Encoding.Default.GetString(bytes);
        }

        public static string ToStr(this byte[] bytes, int index, int count)
        {
            return Encoding.Default.GetString(bytes, index, count);
        }

        public static string Utf8ToStr(this byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        public static string Utf8ToStr(this byte[] bytes, int index, int count)
        {
            return Encoding.UTF8.GetString(bytes, index, count);
        }

        public static void Write(this byte[] bytes, uint num, int offset)
        {
            BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan()[offset..], num);
        }

        public static void Write(this byte[] bytes, int num, int offset)
        {
            BinaryPrimitives.WriteInt32BigEndian(bytes.AsSpan()[offset..], num);
        }

        public static void Write(this byte[] bytes, byte num, int offset)
        {
            bytes[offset] = num;
        }

        public static void Write(this byte[] bytes, short num, int offset)
        {
            BinaryPrimitives.WriteInt16BigEndian(bytes.AsSpan().Slice(offset), num);
        }

        public static void Write(this byte[] bytes, ushort num, int offset)
        {
            BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan()[offset..], num);
        }

        public static void Write(this byte[] bytes, long num, int offset)
        {
            BinaryPrimitives.WriteInt64BigEndian(bytes.AsSpan()[offset..], num);
        }

        public static int ReadInt(this byte[] bytes, int offset)
        {
            return BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan()[offset..]);
        }

        public static long ReadLong(this byte[] bytes, int offset)
        {
            return BinaryPrimitives.ReadInt64BigEndian(bytes.AsSpan()[offset..]);
        }

        public static int ReadInt(this Span<byte> bytes, int offset)
        {
            return BinaryPrimitives.ReadInt32BigEndian(bytes[offset..]);
        }

        public static long ReadLong(this Span<byte> bytes, int offset)
        {
            return BinaryPrimitives.ReadInt64BigEndian(bytes[offset..]);
        }

        public static int ReadInt(this ReadOnlySpan<byte> bytes, int offset)
        {
            return BinaryPrimitives.ReadInt32BigEndian(bytes[offset..]);
        }

        public static long ReadLong(this ReadOnlySpan<byte> bytes, int offset)
        {
            return BinaryPrimitives.ReadInt64BigEndian(bytes[offset..]);
        }

        public const int IntSize = sizeof(int);
        public const int LongSize = sizeof(long);

        #region WriteSpan
        public static void Write(this Span<byte> buffer, int value, ref int offset)
        {
            if (offset + IntSize > buffer.Length)
            {
                throw new ArgumentException($"Write out of index {offset + IntSize}, {buffer.Length}");
            }

            BinaryPrimitives.WriteInt32BigEndian(buffer[offset..], value);
            offset += IntSize;
        }


        public static void Write(this Span<byte> buffer, long value, ref int offset)
        {
            if (offset + LongSize > buffer.Length)
            {
                throw new ArgumentException($"Write out of index {offset + LongSize}, {buffer.Length}");
            }

            BinaryPrimitives.WriteInt64BigEndian(buffer[offset..], value);
            offset += LongSize;
        }


       
        public static ArraySegment<byte> GetArray(this ReadOnlyMemory<byte> memory)
        {
            if (!MemoryMarshal.TryGetArray(memory, out var result))
            {
                throw new InvalidOperationException("Buffer backed by array was expected");
            }
            return result;
        }

        /*
        public static void WritePipe(this PipeWriter writer, TempNetPackage package, CancellationToken token = default)
        {
            Span<byte> target = stackalloc byte[package.Length + 4];
            int offset = 0;
            target.Write(package.Length, ref offset);
            target.Write(package, ref offset);
            writer.Write(target);
            //_ = writer.FlushAsync(token); 
        }

        public static void Write(this MemoryStream stream, TempNetPackage package)
        {
            Span<byte> target = stackalloc byte[package.Length + 4];
            int offset = 0;
            target.Write(package.Length, ref offset);
            target.Write(package, ref offset);
            stream.Write(target);
            //_ = writer.FlushAsync(token); 
        }*/
        #endregion
    }
}