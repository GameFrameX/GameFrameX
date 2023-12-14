using System;
using System.Text;

public static class BufferExtension
{
    /// <summary>
    /// 整型的大小
    /// </summary>
    public const int IntSize = sizeof(int);

    /// <summary>
    /// 短整型的大小
    /// </summary>
    public const int ShortSize = sizeof(short);

    /// <summary>
    /// 长整型的大小
    /// </summary>
    public const int LongSize = sizeof(long);

    /// <summary>
    /// 单精度浮点数的大小
    /// </summary>
    public const int FloatSize = sizeof(float);

    /// <summary>
    /// 双精度浮点数的大小
    /// </summary>
    public const int DoubleSize = sizeof(double);

    /// <summary>
    /// 字节的大小
    /// </summary>
    public const int ByteSize = sizeof(byte);

    /// <summary>
    /// 有符号字节的大小
    /// </summary>
    public const int SbyteSize = sizeof(sbyte);

    /// <summary>
    /// 布尔值的大小
    /// </summary>
    public const int BoolSize = sizeof(bool);


    #region Write

    /// <summary>
    /// 将整数写入字节数组中的指定偏移量处。
    /// </summary>
    /// <param name="buffer">要写入的字节数组。</param>
    /// <param name="value">要写入的整数值。</param>
    /// <param name="offset">写入操作的偏移量。</param>
    public static unsafe void WriteInt(this byte[] buffer, int value, ref int offset)
    {
        if (offset + IntSize > buffer.Length)
        {
            offset += IntSize;
            return;
        }

        fixed (byte* ptr = buffer)
        {
            *(int*)(ptr + offset) = System.Net.IPAddress.HostToNetworkOrder(value);
            offset += IntSize;
        }
    }

    /// <summary>
    /// 将短整数写入字节数组中的指定偏移量处。
    /// </summary>
    /// <param name="buffer">要写入的字节数组。</param>
    /// <param name="value">要写入的短整数值。</param>
    /// <param name="offset">写入操作的偏移量。</param>
    public static unsafe void WriteShort(this byte[] buffer, short value, ref int offset)
    {
        if (offset + ShortSize > buffer.Length)
        {
            offset += ShortSize;
            return;
        }

        fixed (byte* ptr = buffer)
        {
            *(short*)(ptr + offset) = System.Net.IPAddress.HostToNetworkOrder(value);
            offset += ShortSize;
        }
    }

    /// <summary>
    /// 将长整数写入字节数组中的指定偏移量处。
    /// </summary>
    /// <param name="buffer">要写入的字节数组。</param>
    /// <param name="value">要写入的长整数值。</param>
    /// <param name="offset">写入操作的偏移量。</param>
    public static unsafe void WriteLong(this byte[] buffer, long value, ref int offset)
    {
        if (offset + LongSize > buffer.Length)
        {
            offset += LongSize;
            return;
        }

        fixed (byte* ptr = buffer)
        {
            *(long*)(ptr + offset) = System.Net.IPAddress.HostToNetworkOrder(value);
            offset += LongSize;
        }
    }

    /// <summary>
    /// 将单精度浮点数写入字节数组中的指定偏移量处。
    /// </summary>
    /// <param name="buffer">要写入的字节数组。</param>
    /// <param name="value">要写入的单精度浮点数值。</param>
    /// <param name="offset">字节数组中的偏移量，传递引用以便更新偏移量。</param>
    public static unsafe void WriteFloat(this byte[] buffer, float value, ref int offset)
    {
        if (offset + FloatSize > buffer.Length)
        {
            offset += FloatSize;
            return;
        }

        fixed (byte* ptr = buffer)
        {
            *(float*)(ptr + offset) = value;
            *(int*)(ptr + offset) = System.Net.IPAddress.HostToNetworkOrder(*(int*)(ptr + offset));
            offset += FloatSize;
        }
    }

    /// <summary>
    /// 将双精度浮点数写入字节数组中的指定偏移量处。
    /// </summary>
    /// <param name="buffer">要写入的字节数组。</param>
    /// <param name="value">要写入的双精度浮点数值。</param>
    /// <param name="offset">字节数组中的偏移量，传递引用以便更新偏移量。</param>
    public static unsafe void WriteDouble(this byte[] buffer, double value, ref int offset)
    {
        if (offset + DoubleSize > buffer.Length)
        {
            offset += DoubleSize;
            return;
        }

        fixed (byte* ptr = buffer)
        {
            *(double*)(ptr + offset) = value;
            *(long*)(ptr + offset) = System.Net.IPAddress.HostToNetworkOrder(*(long*)(ptr + offset));
            offset += DoubleSize;
        }
    }

    /// <summary>
    /// 将字节写入字节数组中的指定偏移量处。
    /// </summary>
    /// <param name="buffer">要写入的字节数组。</param>
    /// <param name="value">要写入的字节值。</param>
    /// <param name="offset">字节数组中的偏移量，传递引用以便更新偏移量。</param>
    public static unsafe void WriteByte(this byte[] buffer, byte value, ref int offset)
    {
        if (offset + ByteSize > buffer.Length)
        {
            offset += ByteSize;
            return;
        }

        fixed (byte* ptr = buffer)
        {
            *(ptr + offset) = value;
            offset += ByteSize;
        }
    }

    /// <summary>
    /// 将字节数组写入到缓冲区中，同时更新偏移量。
    /// </summary>
    /// <param name="buffer">目标缓冲区。</param>
    /// <param name="value">要写入的字节数组。</param>
    /// <param name="offset">偏移量。</param>
    public static unsafe void WriteBytes(this byte[] buffer, byte[] value, ref int offset)
    {
        if (value == null)
        {
            buffer.WriteInt(0, ref offset);
            return;
        }

        if (offset + value.Length + IntSize > buffer.Length)
        {
            offset += value.Length + IntSize;
            return;
        }

        buffer.WriteInt(value.Length, ref offset);
        System.Array.Copy(value, 0, buffer, offset, value.Length);
        offset += value.Length;
    }

    /// <summary>
    /// 将有符号字节写入到缓冲区中，同时更新偏移量。
    /// </summary>
    /// <param name="buffer">目标缓冲区。</param>
    /// <param name="value">要写入的有符号字节。</param>
    /// <param name="offset">偏移量。</param>
    public static unsafe void WriteSByte(this byte[] buffer, sbyte value, ref int offset)
    {
        if (offset + SbyteSize > buffer.Length)
        {
            offset += SbyteSize;
            return;
        }

        fixed (byte* ptr = buffer)
        {
            *(sbyte*)(ptr + offset) = value;
            offset += SbyteSize;
        }
    }

    /// <summary>
    /// 将字符串写入到缓冲区中，同时更新偏移量。
    /// </summary>
    /// <param name="buffer">目标缓冲区。</param>
    /// <param name="value">要写入的字符串。</param>
    /// <param name="offset">偏移量。</param>
    public static unsafe void WriteString(this byte[] buffer, string value, ref int offset)
    {
        if (value == null)
        {
            value = string.Empty;
        }

        int len = System.Text.Encoding.UTF8.GetByteCount(value);

        if (len > short.MaxValue)
        {
            throw new ArgumentException($"字符串长度超过了 short.MaxValue {len}, {short.MaxValue}");
        }

        // 预判已经超出长度了，直接计算长度就行了
        if (offset + len + ShortSize > buffer.Length)
        {
            offset += len + ShortSize;
            return;
        }

        fixed (byte* ptr = buffer)
        {
            System.Text.Encoding.UTF8.GetBytes(value, 0, value.Length, buffer, offset + ShortSize);
            buffer.WriteShort((short)len, ref offset);
            offset += len;
        }
    }

    /// <summary>
    /// 将布尔值写入到缓冲区中，同时更新偏移量。
    /// </summary>
    /// <param name="buffer">目标缓冲区。</param>
    /// <param name="value">要写入的布尔值。</param>
    /// <param name="offset">偏移量。</param>
    public static unsafe void WriteBool(this byte[] buffer, bool value, ref int offset)
    {
        if (offset + BoolSize > buffer.Length)
        {
            offset += BoolSize;
            return;
        }

        fixed (byte* ptr = buffer)
        {
            *(bool*)(ptr + offset) = value;
            offset += BoolSize;
        }
    }

    #endregion

    #region Read

    /// <summary>
    /// 从字节数组中读取一个整数值。
    /// </summary>
    /// <param name="buffer">包含整数值的字节数组。</param>
    /// <param name="offset">从字节数组中读取整数值的偏移量。</param>
    /// <returns>从字节数组中读取的整数值。</returns>
    public static unsafe int ReadInt(this byte[] buffer, ref int offset)
    {
        if (offset > buffer.Length + IntSize)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "buffer read out of index");
        }

        fixed (byte* ptr = buffer)
        {
            var value = *(int*)(ptr + offset);
            offset += IntSize;
            return System.Net.IPAddress.NetworkToHostOrder(value);
        }
    }

    /// <summary>
    /// 从字节数组中读取一个短整数值。
    /// </summary>
    /// <param name="buffer">包含短整数值的字节数组。</param>
    /// <param name="offset">从字节数组中读取短整数值的偏移量。</param>
    /// <returns>从字节数组中读取的短整数值。</returns>
    public static unsafe short ReadShort(this byte[] buffer, ref int offset)
    {
        if (offset > buffer.Length + ShortSize)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "buffer read out of index");
        }

        fixed (byte* ptr = buffer)
        {
            var value = *(short*)(ptr + offset);
            offset += ShortSize;
            return System.Net.IPAddress.NetworkToHostOrder(value);
        }
    }

    /// <summary>
    /// 从字节数组中读取一个长整型数值。
    /// </summary>
    /// <param name="buffer">字节数组。</param>
    /// <param name="offset">偏移量。</param>
    /// <returns>长整型数值。</returns>
    public static unsafe long ReadLong(this byte[] buffer, ref int offset)
    {
        if (offset > buffer.Length + LongSize)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "buffer read out of index");
        }

        fixed (byte* ptr = buffer)
        {
            var value = *(long*)(ptr + offset);
            offset += LongSize;
            return System.Net.IPAddress.NetworkToHostOrder(value);
        }
    }

    /// <summary>
    /// 从字节数组中读取一个单精度浮点数值。
    /// </summary>
    /// <param name="buffer">字节数组。</param>
    /// <param name="offset">偏移量。</param>
    /// <returns>单精度浮点数值。</returns>
    public static unsafe float ReadFloat(this byte[] buffer, ref int offset)
    {
        if (offset > buffer.Length + FloatSize)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "buffer read out of index");
        }

        fixed (byte* ptr = buffer)
        {
            *(int*)(ptr + offset) = System.Net.IPAddress.NetworkToHostOrder(*(int*)(ptr + offset));
            var value = *(float*)(ptr + offset);
            offset += FloatSize;
            return value;
        }
    }

    /// <summary>
    /// 从字节数组中读取一个双精度浮点数值。
    /// </summary>
    /// <param name="buffer">字节数组。</param>
    /// <param name="offset">偏移量。</param>
    /// <returns>双精度浮点数值。</returns>
    public static unsafe double ReadDouble(this byte[] buffer, ref int offset)
    {
        if (offset > buffer.Length + DoubleSize)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "buffer read out of index");
        }

        fixed (byte* ptr = buffer)
        {
            *(long*)(ptr + offset) = System.Net.IPAddress.NetworkToHostOrder(*(long*)(ptr + offset));
            var value = *(double*)(ptr + offset);
            offset += DoubleSize;
            return value;
        }
    }

    /// <summary>
    /// 从字节数组中读取一个字节。
    /// </summary>
    /// <param name="buffer">字节数组。</param>
    /// <param name="offset">偏移量。</param>
    /// <returns>字节值。</returns>
    public static unsafe byte ReadByte(this byte[] buffer, ref int offset)
    {
        if (offset > buffer.Length + ByteSize)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "buffer read out of index");
        }

        fixed (byte* ptr = buffer)
        {
            var value = *(ptr + offset);
            offset += ByteSize;
            return value;
        }
    }

    /// <summary>
    /// 从字节数组中读取一定长度的字节。
    /// </summary>
    /// <param name="buffer">字节数组。</param>
    /// <param name="offset">偏移量。</param>
    /// <returns>读取的字节数组。</returns>
    public static unsafe byte[] ReadBytes(this byte[] buffer, ref int offset)
    {
        var len = ReadInt(buffer, ref offset);
        //数据不可信
        if (len <= 0 || offset > buffer.Length + len * ByteSize)
        {
            return Array.Empty<byte>();
        }

        var data = new byte[len];
        System.Array.Copy(buffer, offset, data, 0, len);
        offset += len;
        return data;
    }

    /// <summary>
    /// 从字节数组中读取有符号字节。
    /// </summary>
    /// <param name="buffer">字节数组。</param>
    /// <param name="offset">偏移量。</param>
    /// <returns>读取的有符号字节。</returns>
    public static unsafe sbyte ReadSByte(this byte[] buffer, ref int offset)
    {
        if (offset > buffer.Length + ByteSize)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "buffer read out of index");
        }

        fixed (byte* ptr = buffer)
        {
            var value = *(sbyte*)(ptr + offset);
            offset += ByteSize;
            return value;
        }
    }

    /// <summary>
    /// 从字节数组中读取字符串。
    /// </summary>
    /// <param name="buffer">字节数组。</param>
    /// <param name="offset">偏移量。</param>
    /// <returns>读取的字符串。</returns>
    public static unsafe string ReadString(this byte[] buffer, ref int offset)
    {
        fixed (byte* ptr = buffer)
        {
            var len = ReadShort(buffer, ref offset);
            //数据不可信
            if (len <= 0 || offset > buffer.Length + len * ByteSize)
                return "";

            var value = System.Text.Encoding.UTF8.GetString(buffer, offset, len);
            offset += len;
            return value;
        }
    }

    /// <summary>
    /// 从字节数组中读取布尔值。
    /// </summary>
    /// <param name="buffer">字节数组。</param>
    /// <param name="offset">偏移量。</param>
    /// <returns>读取的布尔值。</returns>
    public static unsafe bool ReadBool(this byte[] buffer, ref int offset)
    {
        if (offset > buffer.Length + BoolSize)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "buffer read out of index");
        }

        fixed (byte* ptr = buffer)
        {
            var value = *(bool*)(ptr + offset);
            offset += BoolSize;
            return value;
        }
    }

    #endregion

    private static readonly StringBuilder StringBuilder = new StringBuilder();

    /// <summary>
    /// 将字节转换为十六进制字符串。
    /// </summary>
    /// <param name="b">要转换的字节。</param>
    /// <returns>表示字节的十六进制字符串。</returns>
    public static string ToHex(this byte b)
    {
        return b.ToString("X2");
    }

    /// <summary>
    /// 将字节数组转换为十六进制字符串。
    /// </summary>
    /// <param name="bytes">要转换的字节数组。</param>
    /// <returns>表示字节数组的十六进制字符串。</returns>
    public static string ToHex(this byte[] bytes)
    {
        StringBuilder.Clear();
        foreach (byte b in bytes)
        {
            StringBuilder.Append(b.ToString("X2"));
        }

        return StringBuilder.ToString();
    }

    /// <summary>
    /// 使用指定格式将字节数组转换为十六进制字符串。
    /// </summary>
    /// <param name="bytes">要转换的字节数组。</param>
    /// <param name="format">十六进制格式。</param>
    /// <returns>表示字节数组的十六进制字符串。</returns>
    public static string ToHex(this byte[] bytes, string format)
    {
        StringBuilder.Clear();
        foreach (byte b in bytes)
        {
            StringBuilder.Append(b.ToString(format));
        }

        return StringBuilder.ToString();
    }

    /// <summary>
    /// 将字节数组中指定范围的字节转换为十六进制字符串。
    /// </summary>
    /// <param name="bytes">要转换的字节数组。</param>
    /// <param name="offset">起始偏移量。</param>
    /// <param name="count">要转换的字节数。</param>
    /// <returns>表示指定范围内字节的十六进制字符串。</returns>
    public static string ToHex(this byte[] bytes, int offset, int count)
    {
        StringBuilder.Clear();
        for (int i = offset; i < offset + count; ++i)
        {
            StringBuilder.Append(bytes[i].ToString("X2"));
        }

        return StringBuilder.ToString();
    }

    /// <summary>
    /// 将字节数组转换为字符串，使用默认编码。
    /// </summary>
    /// <param name="bytes">要转换的字节数组。</param>
    /// <returns>转换后的字符串。</returns>
    public static string ToDefaultString(this byte[] bytes)
    {
        return Encoding.Default.GetString(bytes);
    }

    /// <summary>
    /// 将字节数组的一部分转换为字符串，使用默认编码。
    /// </summary>
    /// <param name="bytes">要转换的字节数组。</param>
    /// <param name="index">起始位置。</param>
    /// <param name="count">要转换的字节数。</param>
    /// <returns>转换后的字符串。</returns>
    public static string ToDefaultString(this byte[] bytes, int index, int count)
    {
        return Encoding.Default.GetString(bytes, index, count);
    }

    /// <summary>
    /// 将字节数组转换为字符串，使用UTF-8编码。
    /// </summary>
    /// <param name="bytes">要转换的字节数组。</param>
    /// <returns>转换后的字符串。</returns>
    public static string ToUtf8String(this byte[] bytes)
    {
        return Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    /// 将字节数组的一部分转换为字符串，使用UTF-8编码。
    /// </summary>
    /// <param name="bytes">要转换的字节数组。</param>
    /// <param name="index">起始位置。</param>
    /// <param name="count">要转换的字节数。</param>
    /// <returns>转换后的字符串。</returns>
    public static string ToUtf8String(this byte[] bytes, int index, int count)
    {
        return Encoding.UTF8.GetString(bytes, index, count);
    }
}