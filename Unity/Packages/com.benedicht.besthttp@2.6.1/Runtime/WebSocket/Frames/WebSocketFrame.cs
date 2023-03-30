#if !BESTHTTP_DISABLE_WEBSOCKET && (!UNITY_WEBGL || UNITY_EDITOR)

using BestHTTP.Extensions;
using BestHTTP.PlatformSupport.Memory;
using System;
using System.IO;

namespace BestHTTP.WebSocket.Frames
{
    public struct RawFrameData : IDisposable
    {
        public byte[] Data;
        public int Length;

        public RawFrameData(byte[] data, int length)
        {
            Data = data;
            Length = length;
        }

        public void Dispose()
        {
            BufferPool.Release(Data);
            Data = null;
        }
    }
    /// <summary>
    /// Denotes a binary frame. The "Payload data" is arbitrary binary data whose interpretation is solely up to the application layer.
    /// This is the base class of all other frame writers, as all frame can be represented as a byte array.
    /// </summary>
    [BestHTTP.PlatformSupport.IL2CPP.Il2CppEagerStaticClassConstructionAttribute]
    public sealed class WebSocketFrame
    {
        public WebSocketFrameTypes Type { get; private set; }
        public bool IsFinal { get; private set; }
        public byte Header { get; private set; }

        public byte[] Data { get; private set; }
        public int DataLength { get; private set; }
        public bool UseExtensions { get; private set; }

        public override string ToString()
        {
            return string.Format("[WebSocketFrame Type: {0}, IsFinal: {1}, Header: {2:X2}, DataLength: {3}, UseExtensions: {4}]",
                this.Type, this.IsFinal, this.Header, this.DataLength, this.UseExtensions);
        }

        #region Constructors

        public WebSocketFrame(WebSocket webSocket, WebSocketFrameTypes type, byte[] data)
            :this(webSocket, type, data, true)
        { }

        public WebSocketFrame(WebSocket webSocket, WebSocketFrameTypes type, byte[] data, bool useExtensions)
            : this(webSocket, type, data, 0, data != null ? (UInt64)data.Length : 0, true, useExtensions)
        {
        }

        public WebSocketFrame(WebSocket webSocket, WebSocketFrameTypes type, byte[] data, bool isFinal, bool useExtensions)
            : this(webSocket, type, data, 0, data != null ? (UInt64)data.Length : 0, isFinal, useExtensions)
        {
        }

        public WebSocketFrame(WebSocket webSocket, WebSocketFrameTypes type, byte[] data, UInt64 pos, UInt64 length, bool isFinal, bool useExtensions)
        {
            this.Type = type;
            this.IsFinal = isFinal;
            this.UseExtensions = useExtensions;

            this.DataLength = (int)length;
            if (data != null)
            {
                this.Data = BufferPool.Get(this.DataLength, true);
                Array.Copy(data, (int)pos, this.Data, 0, this.DataLength);
            }
            else
                data = BufferPool.NoData;

            // First byte: Final Bit + Rsv flags + OpCode
            byte finalBit = (byte)(IsFinal ? 0x80 : 0x0);
            this.Header = (byte)(finalBit | (byte)Type);

            if (this.UseExtensions && webSocket != null && webSocket.Extensions != null)
            {
                for (int i = 0; i < webSocket.Extensions.Length; ++i)
                {
                    var ext = webSocket.Extensions[i];
                    if (ext != null)
                    {
                        this.Header |= ext.GetFrameHeader(this, this.Header);
                        byte[] newData = ext.Encode(this);

                        if (newData != this.Data)
                        {
                            BufferPool.Release(this.Data);

                            this.Data = newData;
                            this.DataLength = newData.Length;
                        }
                    }
                }
            }
        }

        #endregion

        #region Public Functions

        public unsafe RawFrameData Get()
        {
            if (Data == null)
                Data = BufferPool.NoData;

            using (var ms = new BufferPoolMemoryStream(this.DataLength + 9))
            {
                // For the complete documentation for this section see:
                // http://tools.ietf.org/html/rfc6455#section-5.2

                // Write the header
                ms.WriteByte(this.Header);

                // The length of the "Payload data", in bytes: if 0-125, that is the payload length.  If 126, the following 2 bytes interpreted as a
                // 16-bit unsigned integer are the payload length.  If 127, the following 8 bytes interpreted as a 64-bit unsigned integer (the
                // most significant bit MUST be 0) are the payload length.  Multibyte length quantities are expressed in network byte order.
                if (this.DataLength < 126)
                    ms.WriteByte((byte)(0x80 | (byte)this.DataLength));
                else if (this.DataLength < UInt16.MaxValue)
                {
                    ms.WriteByte((byte)(0x80 | 126));
                    byte[] len = BitConverter.GetBytes((UInt16)this.DataLength);
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(len, 0, len.Length);

                    ms.Write(len, 0, len.Length);
                }
                else
                {
                    ms.WriteByte((byte)(0x80 | 127));
                    byte[] len = BitConverter.GetBytes((UInt64)this.DataLength);
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(len, 0, len.Length);

                    ms.Write(len, 0, len.Length);
                }

                // All frames sent from the client to the server are masked by a 32-bit value that is contained within the frame.  This field is
                // present if the mask bit is set to 1 and is absent if the mask bit is set to 0.
                // If the data is being sent by the client, the frame(s) MUST be masked.
                byte[] mask = BufferPool.Get(4, true);

                int hash = this.GetHashCode();

                mask[0] = (byte)((hash >> 24) & 0xFF);
                mask[1] = (byte)((hash >> 16) & 0xFF);
                mask[2] = (byte)((hash >> 8) & 0xFF);
                mask[3] = (byte)(hash & 0xFF);

                ms.Write(mask, 0, 4);

                // Do the masking.
                fixed (byte* pData = Data, pmask = mask)
                {
                    // Here, instead of byte by byte, we reinterpret cast the data as uints and apply the masking so.
                    // This way, we can mask 4 bytes in one cycle, instead of just 1
                    int localLength = this.DataLength / 4;
                    if (localLength > 0)
                    {
                        uint* upData = (uint*)pData;
                        uint umask = *(uint*)pmask;

                        unchecked
                        {
                            for (int i = 0; i < localLength; ++i)
                                upData[i] = upData[i] ^ umask;
                        }
                    }

                    // Because data might not be exactly dividable by 4, we have to mask the remaining 0..3 too.
                    int from = localLength * 4;
                    localLength = from + this.DataLength % 4;
                    for (int i = from; i < localLength; ++i)
                        pData[i] = (byte)(pData[i] ^ pmask[i % 4]);
                }

                BufferPool.Release(mask);

                ms.Write(Data, 0, DataLength);

                return new RawFrameData(ms.ToArray(true), (int)ms.Length);
            }
        }


        public WebSocketFrame[] Fragment(uint maxFragmentSize)
        {
            if (this.Data == null)
                return null;

            // All control frames MUST have a payload length of 125 bytes or less and MUST NOT be fragmented.
            if (this.Type != WebSocketFrameTypes.Binary && this.Type != WebSocketFrameTypes.Text)
                return null;

            if (this.DataLength <= maxFragmentSize)
                return null;

            this.IsFinal = false;

            // Clear final bit from the header flags
            this.Header &= 0x7F;

            // One chunk will remain in this fragment, so we have to allocate one less
            int count = (int)((this.DataLength / maxFragmentSize) + (this.DataLength % maxFragmentSize == 0 ? -1 : 0));

            WebSocketFrame[] fragments = new WebSocketFrame[count];

            // Skip one chunk, for the current one
            UInt64 pos = maxFragmentSize;
            while (pos < (UInt64)this.DataLength)
            {
                UInt64 chunkLength = Math.Min(maxFragmentSize, (UInt64)this.DataLength - pos);

                fragments[fragments.Length - count--] = new WebSocketFrame(null, WebSocketFrameTypes.Continuation, this.Data, pos, chunkLength, pos + chunkLength >= (UInt64)this.DataLength, false);

                pos += chunkLength;
            }

            //byte[] newData = VariableSizedBufferPool.Get(maxFragmentSize, true);
            //Array.Copy(this.Data, 0, newData, 0, maxFragmentSize);
            //VariableSizedBufferPool.Release(this.Data);

            //this.Data = newData;
            this.DataLength = (int)maxFragmentSize;

            return fragments;
        }

        #endregion
    }
}

#endif
