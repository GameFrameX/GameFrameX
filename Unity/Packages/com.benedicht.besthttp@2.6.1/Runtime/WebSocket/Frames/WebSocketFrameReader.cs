#if !BESTHTTP_DISABLE_WEBSOCKET && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using System.Collections.Generic;
using System.IO;

using BestHTTP.Extensions;
using BestHTTP.PlatformSupport.Memory;

namespace BestHTTP.WebSocket.Frames
{
    /// <summary>
    /// Represents an incoming WebSocket Frame.
    /// </summary>
    public struct WebSocketFrameReader
    {
#region Properties

        public byte Header { get; private set; }

        /// <summary>
        /// True if it's a final Frame in a sequence, or the only one.
        /// </summary>
        public bool IsFinal { get; private set; }

        /// <summary>
        /// The type of the Frame.
        /// </summary>
        public WebSocketFrameTypes Type { get; private set; }

        /// <summary>
        /// Indicates if there are any mask sent to decode the data.
        /// </summary>
        public bool HasMask { get; private set; }

        /// <summary>
        /// The length of the Data.
        /// </summary>
        public UInt64 Length { get; private set; }

        /// <summary>
        /// The decoded array of bytes.
        /// </summary>
        public byte[] Data { get; private set; }

        /// <summary>
        /// Textual representation of the received Data.
        /// </summary>
        public string DataAsText { get; private set; }

        #endregion

        #region Internal & Private Functions

        internal unsafe void Read(Stream stream)
        {
            // For the complete documentation for this section see:
            // http://tools.ietf.org/html/rfc6455#section-5.2

            this.Header = ReadByte(stream);

            // The first byte is the Final Bit and the type of the frame
            IsFinal = (this.Header & 0x80) != 0;
            Type = (WebSocketFrameTypes)(this.Header & 0xF);

            byte maskAndLength = ReadByte(stream);

            // The second byte is the Mask Bit and the length of the payload data
            HasMask = (maskAndLength & 0x80) != 0;

            // if 0-125, that is the payload length.
            Length = (UInt64)(maskAndLength & 127);

            // If 126, the following 2 bytes interpreted as a 16-bit unsigned integer are the payload length.
            if (Length == 126)
            {
                byte[] rawLen = BufferPool.Get(2, true);

                stream.ReadBuffer(rawLen, 2);

                if (BitConverter.IsLittleEndian)
                    Array.Reverse(rawLen, 0, 2);

                Length = (UInt64)BitConverter.ToUInt16(rawLen, 0);

                BufferPool.Release(rawLen);
            }
            else if (Length == 127)
            {
                // If 127, the following 8 bytes interpreted as a 64-bit unsigned integer (the
                // most significant bit MUST be 0) are the payload length.

                byte[] rawLen = BufferPool.Get(8, true);

                stream.ReadBuffer(rawLen, 8);

                if (BitConverter.IsLittleEndian)
                    Array.Reverse(rawLen, 0, 8);

                Length = (UInt64)BitConverter.ToUInt64(rawLen, 0);

                BufferPool.Release(rawLen);
            }

            // The sent byte array as a mask to decode the data.
            byte[] mask = null;

            // Read the Mask, if has any
            if (HasMask)
            {
                mask = BufferPool.Get(4, true);
                if (stream.Read(mask, 0, 4) < mask.Length)
                    throw ExceptionHelper.ServerClosedTCPStream();
            }

            if (Type == WebSocketFrameTypes.Text || Type == WebSocketFrameTypes.Continuation)
                Data = BufferPool.Get((long)Length, true);
            else
                if (Length == 0)
                    Data = BufferPool.NoData;
                else
                    Data = new byte[Length];
            //Data = Type == WebSocketFrameTypes.Text ? VariableSizedBufferPool.Get((long)Length, true) : new byte[Length];

            if (Length == 0L)
                return;

            uint readLength = 0;

            do
            {
                int read = stream.Read(Data, (int)readLength, (int)(Length - readLength));

                if (read <= 0)
                    throw ExceptionHelper.ServerClosedTCPStream();

                readLength += (uint)read;
            } while (readLength < Length);

            if (HasMask)
            {
                fixed (byte* pData = Data, pmask = mask)
                {
                    // Here, instead of byte by byte, we reinterpret cast the data as uints and apply the masking so.
                    // This way, we can mask 4 bytes in one cycle, instead of just 1
                    ulong localLength = this.Length / 4;
                    if (localLength > 0)
                    {
                        uint* upData = (uint*)pData;
                        uint umask = *(uint*)pmask;

                        unchecked
                        {
                            for (ulong i = 0; i < localLength; ++i)
                                upData[i] = upData[i] ^ umask;
                        }
                    }

                    // Because data might not be exactly dividable by 4, we have to mask the remaining 0..3 too.
                    ulong from = localLength * 4;
                    localLength = from + this.Length % 4;
                    for (ulong i = from; i < localLength; ++i)
                        pData[i] = (byte)(pData[i] ^ pmask[i % 4]);
                }

                BufferPool.Release(mask);
            }
        }

        private byte ReadByte(Stream stream)
        {
            int read = stream.ReadByte();

            if (read < 0)
                throw ExceptionHelper.ServerClosedTCPStream();

            return (byte)read;
        }

#endregion

#region Public Functions

        /// <summary>
        /// Assembles all fragments into a final frame. Call this on the last fragment of a frame.
        /// </summary>
        /// <param name="fragments">The list of previously downloaded and parsed fragments of the frame</param>
        public void Assemble(List<WebSocketFrameReader> fragments)
        {
            // this way the following algorithms will handle this fragment's data too
            fragments.Add(this);

            UInt64 finalLength = 0;
            for (int i = 0; i < fragments.Count; ++i)
                finalLength += fragments[i].Length;

            byte[] buffer = fragments[0].Type == WebSocketFrameTypes.Text ? BufferPool.Get((long)finalLength, true) : new byte[finalLength];
            UInt64 pos = 0;
            for (int i = 0; i < fragments.Count; ++i)
            {
                Array.Copy(fragments[i].Data, 0, buffer, (int)pos, (int)fragments[i].Length);
                fragments[i].ReleaseData();

                pos += fragments[i].Length;
            }

            // All fragments of a message are of the same type, as set by the first fragment's opcode.
            this.Type = fragments[0].Type;

            // Reserver flags may be contained only in the first fragment

            this.Header = fragments[0].Header;
            this.Length = finalLength;
            this.Data = buffer;
        }

        /// <summary>
        /// This function will decode the received data incrementally with the associated websocket's extensions.
        /// </summary>
        public void DecodeWithExtensions(WebSocket webSocket)
        {
            if (webSocket.Extensions != null)
                for (int i = 0; i < webSocket.Extensions.Length; ++i)
                {
                    var ext = webSocket.Extensions[i];
                    if (ext != null)
                    {
                        var newData = ext.Decode(this.Header, this.Data, (int)this.Length);
                        if (this.Data != newData)
                        {
                            this.ReleaseData();
                            this.Data = newData;
                            this.Length = (ulong)newData.Length;
                        }
                    }
                }

            if (this.Type == WebSocketFrameTypes.Text && this.Data != null)
            {
                this.DataAsText = System.Text.Encoding.UTF8.GetString(this.Data, 0, (int)this.Length);
                this.ReleaseData();
            }
        }

        public void ReleaseData()
        {
            if (this.Data != null)
            {
                BufferPool.Release(this.Data);
                this.Data = null;
            }
        }

#endregion
    }
}

#endif
