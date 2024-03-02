using System.Diagnostics;
using System.Text;

namespace Server.NetWork
{
    /// <summary>
    /// Dynamic byte buffer
    /// </summary>
    public sealed class SocketBuffer
    {
        private byte[] _data;
        private long size;
        private long offset;

        /// <summary>
        /// Is the buffer empty?
        /// </summary>
        public bool IsEmpty => (_data == null) || (size == 0);

        /// <summary>
        /// Bytes memory buffer
        /// </summary>
        public byte[] Data => _data;

        /// <summary>
        /// Bytes memory buffer capacity
        /// </summary>
        public long Capacity => _data.Length;

        /// <summary>
        /// Bytes memory buffer size
        /// </summary>
        public long Size => size;

        /// <summary>
        /// Bytes memory buffer offset
        /// </summary>
        public long Offset => offset;

        /// <summary>
        /// Buffer indexer operator
        /// </summary>
        public byte this[long index] => _data[index];

        /// <summary>
        /// Initialize a new expandable buffer with zero capacity
        /// </summary>
        public SocketBuffer()
        {
            _data = new byte[0];
            size = 0;
            offset = 0;
        }

        /// <summary>
        /// Initialize a new expandable buffer with the given capacity
        /// </summary>
        public SocketBuffer(long capacity)
        {
            _data = new byte[capacity];
            size = 0;
            offset = 0;
        }

        /// <summary>
        /// Initialize a new expandable buffer with the given data
        /// </summary>
        public SocketBuffer(byte[] data)
        {
            _data = data;
            size = data.Length;
            offset = 0;
        }

        #region Memory buffer methods

        /// <summary>
        /// Get a span of bytes from the current buffer
        /// </summary>
        public Span<byte> AsSpan()
        {
            return new Span<byte>(_data, (int)offset, (int)size);
        }

        /// <summary>
        /// Get a string from the current buffer
        /// </summary>
        public override string ToString()
        {
            return ExtractString(0, size);
        }

        /// <summary>
        /// Clear the current buffer and its offset
        /// </summary>
        public void Clear()
        {
            size = 0;
            offset = 0;
        }

        /// <summary>
        /// Extract the string from buffer of the given offset and size
        /// </summary>
        public string ExtractString(long offset, long size)
        {
            Debug.Assert(((offset + size) <= Size), "Invalid offset & size!");
            if ((offset + size) > Size)
                throw new ArgumentException("Invalid offset & size!", nameof(offset));

            return Encoding.UTF8.GetString(_data, (int)offset, (int)size);
        }

        /// <summary>
        /// Remove the buffer of the given offset and size
        /// </summary>
        public void Remove(long offset, long size)
        {
            Debug.Assert(((offset + size) <= Size), "Invalid offset & size!");
            if ((offset + size) > Size)
                throw new ArgumentException("Invalid offset & size!", nameof(offset));

            Array.Copy(_data, offset + size, _data, offset, this.size - size - offset);
            this.size -= size;
            if (this.offset >= (offset + size))
                this.offset -= size;
            else if (this.offset >= offset)
            {
                this.offset -= this.offset - offset;
                if (this.offset > Size)
                    this.offset = Size;
            }
        }

        /// <summary>
        /// Reserve the buffer of the given capacity
        /// </summary>
        public void Reserve(long capacity)
        {
            Debug.Assert((capacity >= 0), "Invalid reserve capacity!");
            if (capacity < 0)
                throw new ArgumentException("Invalid reserve capacity!", nameof(capacity));

            if (capacity > Capacity)
            {
                byte[] data = new byte[Math.Max(capacity, 2 * Capacity)];
                Array.Copy(_data, 0, data, 0, size);
                _data = data;
            }
        }

        /// <summary>
        /// Resize the current buffer
        /// </summary>
        public void Resize(long size)
        {
            Reserve(size);
            this.size = size;
            if (offset > this.size)
                offset = this.size;
        }

        /// <summary>
        /// Shift the current buffer offset
        /// </summary>
        public void Shift(long offset)
        {
            this.offset += offset;
        }

        /// <summary>
        /// Unshift the current buffer offset
        /// </summary>
        public void Unshift(long offset)
        {
            this.offset -= offset;
        }

        #endregion

        #region Buffer I/O methods

        /// <summary>
        /// Append the single byte
        /// </summary>
        /// <param name="value">Byte value to append</param>
        /// <returns>Count of append bytes</returns>
        public long Append(byte value)
        {
            Reserve(size + 1);
            _data[size] = value;
            size += 1;
            return 1;
        }

        /// <summary>
        /// Append the given buffer
        /// </summary>
        /// <param name="buffer">Buffer to append</param>
        /// <returns>Count of append bytes</returns>
        public long Append(byte[] buffer)
        {
            Reserve(size + buffer.Length);
            Array.Copy(buffer, 0, _data, size, buffer.Length);
            size += buffer.Length;
            return buffer.Length;
        }

        /// <summary>
        /// Append the given buffer fragment
        /// </summary>
        /// <param name="buffer">Buffer to append</param>
        /// <param name="offset">Buffer offset</param>
        /// <param name="size">Buffer size</param>
        /// <returns>Count of append bytes</returns>
        public long Append(byte[] buffer, long offset, long size)
        {
            Reserve(this.size + size);
            Array.Copy(buffer, offset, _data, this.size, size);
            this.size += size;
            return size;
        }

        /// <summary>
        /// Append the given span of bytes
        /// </summary>
        /// <param name="buffer">Buffer to append as a span of bytes</param>
        /// <returns>Count of append bytes</returns>
        public long Append(ReadOnlySpan<byte> buffer)
        {
            Reserve(size + buffer.Length);
            buffer.CopyTo(new Span<byte>(_data, (int)size, buffer.Length));
            size += buffer.Length;
            return buffer.Length;
        }

        /// <summary>
        /// Append the given buffer
        /// </summary>
        /// <param name="socketBuffer">Buffer to append</param>
        /// <returns>Count of append bytes</returns>
        public long Append(SocketBuffer socketBuffer) => Append(socketBuffer.AsSpan());

        /// <summary>
        /// Append the given text in UTF-8 encoding
        /// </summary>
        /// <param name="text">Text to append</param>
        /// <returns>Count of append bytes</returns>
        public long Append(string text)
        {
            int length = Encoding.UTF8.GetMaxByteCount(text.Length);
            Reserve(size + length);
            long result = Encoding.UTF8.GetBytes(text, 0, text.Length, _data, (int)size);
            size += result;
            return result;
        }

        /// <summary>
        /// Append the given text in UTF-8 encoding
        /// </summary>
        /// <param name="text">Text to append as a span of characters</param>
        /// <returns>Count of append bytes</returns>
        public long Append(ReadOnlySpan<char> text)
        {
            int length = Encoding.UTF8.GetMaxByteCount(text.Length);
            Reserve(size + length);
            long result = Encoding.UTF8.GetBytes(text, new Span<byte>(_data, (int)size, length));
            size += result;
            return result;
        }

        #endregion
    }
}