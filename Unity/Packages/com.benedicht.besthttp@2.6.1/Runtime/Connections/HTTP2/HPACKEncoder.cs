#if (!UNITY_WEBGL || UNITY_EDITOR) && !BESTHTTP_DISABLE_ALTERNATE_SSL && !BESTHTTP_DISABLE_HTTP2

using BestHTTP.Extensions;
using BestHTTP.PlatformSupport.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BestHTTP.Connections.HTTP2
{
    public sealed class HPACKEncoder
    {
        private HTTP2SettingsManager settingsRegistry;

        // https://http2.github.io/http2-spec/compression.html#encoding.context
        // When used for bidirectional communication, such as in HTTP, the encoding and decoding dynamic tables
        //  maintained by an endpoint are completely independent, i.e., the request and response dynamic tables are separate.
        private HeaderTable requestTable;
        private HeaderTable responseTable;

        private HTTP2Handler parent;

        public HPACKEncoder(HTTP2Handler parentHandler, HTTP2SettingsManager registry)
        {
            this.parent = parentHandler;
            this.settingsRegistry = registry;

            // I'm unsure what settings (local or remote) we should use for these two tables!
            this.requestTable = new HeaderTable(this.settingsRegistry.MySettings);
            this.responseTable = new HeaderTable(this.settingsRegistry.RemoteSettings);
        }

        public void Encode(HTTP2Stream context, HTTPRequest request, Queue<HTTP2FrameHeaderAndPayload> to, UInt32 streamId)
        {
            // Add usage of SETTINGS_MAX_HEADER_LIST_SIZE to be able to create a header and one or more continuation fragments
            // (https://httpwg.org/specs/rfc7540.html#SettingValues)

            using (BufferPoolMemoryStream bufferStream = new BufferPoolMemoryStream())
            {
                WriteHeader(bufferStream, ":method", HTTPRequest.MethodNames[(int)request.MethodType]);
                // add path
                WriteHeader(bufferStream, ":path", request.CurrentUri.PathAndQuery);
                // add authority
                WriteHeader(bufferStream, ":authority", request.CurrentUri.Authority);
                // add scheme
                WriteHeader(bufferStream, ":scheme", "https");

                //bool hasBody = false;

                // add other, regular headers
                request.EnumerateHeaders((header, values) =>
                {
                    if (header.Equals("connection", StringComparison.OrdinalIgnoreCase) ||
                        header.Equals("te", StringComparison.OrdinalIgnoreCase) ||
                        header.Equals("host", StringComparison.OrdinalIgnoreCase) ||
                        header.Equals("keep-alive", StringComparison.OrdinalIgnoreCase) ||
                        header.StartsWith("proxy-", StringComparison.OrdinalIgnoreCase))
                        return;

                    //if (!hasBody)
                    //    hasBody = header.Equals("content-length", StringComparison.OrdinalIgnoreCase) && int.Parse(values[0]) > 0;

                    // https://httpwg.org/specs/rfc7540.html#HttpSequence
                    // The chunked transfer encoding defined in Section 4.1 of [RFC7230] MUST NOT be used in HTTP/2.
                    if (header.Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase))
                    {
                        // error!
                        return;
                    }

                    // https://httpwg.org/specs/rfc7540.html#HttpHeaders
                    // Just as in HTTP/1.x, header field names are strings of ASCII characters that are compared in a case-insensitive fashion.
                    // However, header field names MUST be converted to lowercase prior to their encoding in HTTP/2. 
                    // A request or response containing uppercase header field names MUST be treated as malformed
                    if (header.Any(Char.IsUpper))
                        header = header.ToLower();

                    for (int i = 0; i < values.Count; ++i)
                    {
                        WriteHeader(bufferStream, header, values[i]);

                        if (HTTPManager.Logger.Level <= Logger.Loglevels.Information)
                            HTTPManager.Logger.Information("HPACKEncoder", string.Format("[{0}] - Encode - Header({1}/{2}): '{3}': '{4}'", context.Id, i + 1, values.Count, header, values[i]), this.parent.Context, context.Context, request.Context);
                    }
                }, true);

                var upStreamInfo = request.GetUpStream();
                CreateHeaderFrames(to,
                                   streamId,
                                   bufferStream.ToArray(true),
                                   (UInt32)bufferStream.Length,
                                   upStreamInfo.Stream != null);
            }
        }

        public void Decode(HTTP2Stream context, Stream stream, List<KeyValuePair<string, string>> to)
        {
            int headerType = stream.ReadByte();
            while (headerType != -1)
            {
                byte firstDataByte = (byte)headerType;

                // https://http2.github.io/http2-spec/compression.html#indexed.header.representation
                if (BufferHelper.ReadBit(firstDataByte, 0) == 1)
                {
                    var header = ReadIndexedHeader(firstDataByte, stream);

                    if (HTTPManager.Logger.Level <= Logger.Loglevels.Information)
                        HTTPManager.Logger.Information("HPACKEncoder", string.Format("[{0}] Decode - IndexedHeader: {1}", context.Id, header.ToString()), this.parent.Context, context.Context, context.AssignedRequest.Context);

                    to.Add(header);
                }
                else if (BufferHelper.ReadValue(firstDataByte, 0, 1) == 1)
                {
                    // https://http2.github.io/http2-spec/compression.html#literal.header.with.incremental.indexing

                    if (BufferHelper.ReadValue(firstDataByte, 2, 7) == 0)
                    {
                        // Literal Header Field with Incremental Indexing — New Name
                        var header = ReadLiteralHeaderFieldWithIncrementalIndexing_NewName(firstDataByte, stream);

                        if (HTTPManager.Logger.Level <= Logger.Loglevels.Information)
                            HTTPManager.Logger.Information("HPACKEncoder", string.Format("[{0}] Decode - LiteralHeaderFieldWithIncrementalIndexing_NewName: {1}", context.Id, header.ToString()), this.parent.Context, context.Context, context.AssignedRequest.Context);

                        this.responseTable.Add(header);
                        to.Add(header);
                    }
                    else
                    {
                        // Literal Header Field with Incremental Indexing — Indexed Name
                        var header = ReadLiteralHeaderFieldWithIncrementalIndexing_IndexedName(firstDataByte, stream);

                        if (HTTPManager.Logger.Level <= Logger.Loglevels.Information)
                            HTTPManager.Logger.Information("HPACKEncoder", string.Format("[{0}] Decode - LiteralHeaderFieldWithIncrementalIndexing_IndexedName: {1}", context.Id, header.ToString()), this.parent.Context, context.Context, context.AssignedRequest.Context);

                        this.responseTable.Add(header);
                        to.Add(header);
                    }
                } else if (BufferHelper.ReadValue(firstDataByte, 0, 3) == 0)
                {
                    // https://http2.github.io/http2-spec/compression.html#literal.header.without.indexing

                    if (BufferHelper.ReadValue(firstDataByte, 4, 7) == 0)
                    {
                        // Literal Header Field without Indexing — New Name
                        var header = ReadLiteralHeaderFieldwithoutIndexing_NewName(firstDataByte, stream);

                        if (HTTPManager.Logger.Level <= Logger.Loglevels.Information)
                            HTTPManager.Logger.Information("HPACKEncoder", string.Format("[{0}] Decode - LiteralHeaderFieldwithoutIndexing_NewName: {1}", context.Id, header.ToString()), this.parent.Context, context.Context, context.AssignedRequest.Context);

                        to.Add(header);
                    }
                    else
                    {
                        // Literal Header Field without Indexing — Indexed Name
                        var header = ReadLiteralHeaderFieldwithoutIndexing_IndexedName(firstDataByte, stream);

                        if (HTTPManager.Logger.Level <= Logger.Loglevels.Information)
                            HTTPManager.Logger.Information("HPACKEncoder", string.Format("[{0}] Decode - LiteralHeaderFieldwithoutIndexing_IndexedName: {1}", context.Id, header.ToString()), this.parent.Context, context.Context, context.AssignedRequest.Context);

                        to.Add(header);
                    }
                }
                else if (BufferHelper.ReadValue(firstDataByte, 0, 3) == 1)
                {
                    // https://http2.github.io/http2-spec/compression.html#literal.header.never.indexed

                    if (BufferHelper.ReadValue(firstDataByte, 4, 7) == 0)
                    {
                        // Literal Header Field Never Indexed — New Name
                        var header = ReadLiteralHeaderFieldNeverIndexed_NewName(firstDataByte, stream);

                        if (HTTPManager.Logger.Level <= Logger.Loglevels.Information)
                            HTTPManager.Logger.Information("HPACKEncoder", string.Format("[{0}] Decode - LiteralHeaderFieldNeverIndexed_NewName: {1}", context.Id, header.ToString()), this.parent.Context, context.Context, context.AssignedRequest.Context);

                        to.Add(header);
                    }
                    else
                    {
                        // Literal Header Field Never Indexed — Indexed Name
                        var header = ReadLiteralHeaderFieldNeverIndexed_IndexedName(firstDataByte, stream);

                        if (HTTPManager.Logger.Level <= Logger.Loglevels.Information)
                            HTTPManager.Logger.Information("HPACKEncoder", string.Format("[{0}] Decode - LiteralHeaderFieldNeverIndexed_IndexedName: {1}", context.Id, header.ToString()), this.parent.Context, context.Context, context.AssignedRequest.Context);

                        to.Add(header);
                    }
                }
                else if (BufferHelper.ReadValue(firstDataByte, 0, 2) == 1)
                {
                    // https://http2.github.io/http2-spec/compression.html#encoding.context.update

                    UInt32 newMaxSize = DecodeInteger(5, firstDataByte, stream);

                    if (HTTPManager.Logger.Level <= Logger.Loglevels.Information)
                        HTTPManager.Logger.Information("HPACKEncoder", string.Format("[{0}] Decode - Dynamic Table Size Update: {1}", context.Id, newMaxSize), this.parent.Context, context.Context, context.AssignedRequest.Context);

                    //this.settingsRegistry[HTTP2Settings.HEADER_TABLE_SIZE] = (UInt16)newMaxSize;
                    this.responseTable.MaxDynamicTableSize = (UInt16)newMaxSize;
                }
                else
                {
                    // ERROR
                }

                headerType = stream.ReadByte();
            }
        }

        private KeyValuePair<string, string> ReadIndexedHeader(byte firstByte, Stream stream)
        {
            // https://http2.github.io/http2-spec/compression.html#indexed.header.representation

            UInt32 index = DecodeInteger(7, firstByte, stream);
            return this.responseTable.GetHeader(index);
        }

        private KeyValuePair<string, string> ReadLiteralHeaderFieldWithIncrementalIndexing_IndexedName(byte firstByte, Stream stream)
        {
            // https://http2.github.io/http2-spec/compression.html#literal.header.with.incremental.indexing

            UInt32 keyIndex = DecodeInteger(6, firstByte, stream);

            string header = this.responseTable.GetKey(keyIndex);
            string value = DecodeString(stream);

            return new KeyValuePair<string, string>(header, value);
        }

        private KeyValuePair<string, string> ReadLiteralHeaderFieldWithIncrementalIndexing_NewName(byte firstByte, Stream stream)
        {
            // https://http2.github.io/http2-spec/compression.html#literal.header.with.incremental.indexing

            string header = DecodeString(stream);
            string value = DecodeString(stream);

            return new KeyValuePair<string, string>(header, value);
        }

        private KeyValuePair<string, string> ReadLiteralHeaderFieldwithoutIndexing_IndexedName(byte firstByte, Stream stream)
        {
            // https://http2.github.io/http2-spec/compression.html#literal.header.without.indexing

            UInt32 index = DecodeInteger(4, firstByte, stream);
            string header = this.responseTable.GetKey(index);
            string value = DecodeString(stream);

            return new KeyValuePair<string, string>(header, value);
        }

        private KeyValuePair<string, string> ReadLiteralHeaderFieldwithoutIndexing_NewName(byte firstByte, Stream stream)
        {
            // https://http2.github.io/http2-spec/compression.html#literal.header.without.indexing

            string header = DecodeString(stream);
            string value = DecodeString(stream);

            return new KeyValuePair<string, string>(header, value);
        }

        private KeyValuePair<string, string> ReadLiteralHeaderFieldNeverIndexed_IndexedName(byte firstByte, Stream stream)
        {
            // https://http2.github.io/http2-spec/compression.html#literal.header.never.indexed

            UInt32 index = DecodeInteger(4, firstByte, stream);
            string header = this.responseTable.GetKey(index);
            string value = DecodeString(stream);

            return new KeyValuePair<string, string>(header, value);
        }

        private KeyValuePair<string, string> ReadLiteralHeaderFieldNeverIndexed_NewName(byte firstByte, Stream stream)
        {
            // https://http2.github.io/http2-spec/compression.html#literal.header.never.indexed

            string header = DecodeString(stream);
            string value = DecodeString(stream);

            return new KeyValuePair<string, string>(header, value);
        }

        private string DecodeString(Stream stream)
        {
            byte start = (byte)stream.ReadByte();
            bool rawString = BufferHelper.ReadBit(start, 0) == 0;
            UInt32 stringLength = DecodeInteger(7, start, stream);

            if (stringLength == 0)
                return string.Empty;

            if (rawString)
            {
                byte[] buffer = BufferPool.Get(stringLength, true);

                stream.Read(buffer, 0, (int)stringLength);

                BufferPool.Release(buffer);

                return System.Text.Encoding.UTF8.GetString(buffer, 0, (int)stringLength);
            }
            else
            {
                var node = HuffmanEncoder.GetRoot();
                byte currentByte = (byte)stream.ReadByte();
                byte bitIdx = 0; // 0..7

                using (BufferPoolMemoryStream bufferStream = new BufferPoolMemoryStream())
                {
                    do
                    {
                        byte bitValue = BufferHelper.ReadBit(currentByte, bitIdx);

                        if (++bitIdx > 7)
                        {
                            stringLength--;

                            if (stringLength > 0)
                            {
                                bitIdx = 0;
                                currentByte = (byte)stream.ReadByte();
                            }
                        }

                        node = HuffmanEncoder.GetNext(node, bitValue);

                        if (node.Value != 0)
                        {
                            if (node.Value != HuffmanEncoder.EOS)
                                bufferStream.WriteByte((byte)node.Value);

                            node = HuffmanEncoder.GetRoot();
                        }
                    } while (stringLength > 0);

                    byte[] buffer = bufferStream.ToArray(true);

                    string result = System.Text.Encoding.UTF8.GetString(buffer, 0, (int)bufferStream.Length);

                    BufferPool.Release(buffer);

                    return result;
                }
            }
        }

        private void CreateHeaderFrames(Queue<HTTP2FrameHeaderAndPayload> to, UInt32 streamId, byte[] dataToSend, UInt32 payloadLength, bool hasBody)
        {
            UInt32 maxFrameSize = this.settingsRegistry.RemoteSettings[HTTP2Settings.MAX_FRAME_SIZE];

            // Only one headers frame
            if (payloadLength <= maxFrameSize)
            {
                HTTP2FrameHeaderAndPayload frameHeader = new HTTP2FrameHeaderAndPayload();
                frameHeader.Type = HTTP2FrameTypes.HEADERS;
                frameHeader.StreamId = streamId;
                frameHeader.Flags = (byte)(HTTP2HeadersFlags.END_HEADERS);

                if (!hasBody)
                    frameHeader.Flags |= (byte)(HTTP2HeadersFlags.END_STREAM);

                frameHeader.PayloadLength = payloadLength;
                frameHeader.Payload = dataToSend;

                to.Enqueue(frameHeader);
            }
            else
            {
                HTTP2FrameHeaderAndPayload frameHeader = new HTTP2FrameHeaderAndPayload();
                frameHeader.Type = HTTP2FrameTypes.HEADERS;
                frameHeader.StreamId = streamId;
                frameHeader.PayloadLength = maxFrameSize;
                frameHeader.Payload = dataToSend;
                frameHeader.DontUseMemPool = true;
                frameHeader.PayloadOffset = 0;

                if (!hasBody)
                    frameHeader.Flags = (byte)(HTTP2HeadersFlags.END_STREAM);

                to.Enqueue(frameHeader);

                UInt32 offset = maxFrameSize;
                while (offset < payloadLength)
                {
                    frameHeader = new HTTP2FrameHeaderAndPayload();
                    frameHeader.Type = HTTP2FrameTypes.CONTINUATION;
                    frameHeader.StreamId = streamId;
                    frameHeader.PayloadLength = maxFrameSize;
                    frameHeader.Payload = dataToSend;
                    frameHeader.PayloadOffset = offset;

                    offset += maxFrameSize;

                    if (offset >= payloadLength)
                    {
                        frameHeader.Flags = (byte)(HTTP2ContinuationFlags.END_HEADERS);
                        // last sent continuation fragment will release back the payload buffer
                        frameHeader.DontUseMemPool = false;
                    }
                    else
                        frameHeader.DontUseMemPool = true;

                    to.Enqueue(frameHeader);
                }
            }
        }

        private void WriteHeader(Stream stream, string header, string value)
        {
            // https://http2.github.io/http2-spec/compression.html#header.representation

            KeyValuePair<UInt32, UInt32> index = this.requestTable.GetIndex(header, value);

            if (index.Key == 0 && index.Value == 0)
            {
                WriteLiteralHeaderFieldWithIncrementalIndexing_NewName(stream, header, value);
                this.requestTable.Add(new KeyValuePair<string, string>(header, value));
            }
            else if (index.Key != 0 && index.Value == 0)
            {
                WriteLiteralHeaderFieldWithIncrementalIndexing_IndexedName(stream, index.Key, value);
                this.requestTable.Add(new KeyValuePair<string, string>(header, value));
            }
            else
            {
                WriteIndexedHeaderField(stream, index.Key);
            }
        }

        private static void WriteIndexedHeaderField(Stream stream, UInt32 index)
        {
            byte requiredBytes = RequiredBytesToEncodeInteger(index, 7);
            byte[] buffer = BufferPool.Get(requiredBytes, true);
            UInt32 offset = 0;

            buffer[0] = 0x80;
            EncodeInteger(index, 7, buffer, ref offset);

            stream.Write(buffer, 0, (int)offset);

            BufferPool.Release(buffer);
        }

        private static void WriteLiteralHeaderFieldWithIncrementalIndexing_IndexedName(Stream stream, UInt32 index, string value)
        {
            // https://http2.github.io/http2-spec/compression.html#literal.header.with.incremental.indexing

            UInt32 requiredBytes = RequiredBytesToEncodeInteger(index, 6) +
                                   RequiredBytesToEncodeString(value);

            byte[] buffer = BufferPool.Get(requiredBytes, true);
            UInt32 offset = 0;

            buffer[0] = 0x40;
            EncodeInteger(index, 6, buffer, ref offset);
            EncodeString(value, buffer, ref offset);

            stream.Write(buffer, 0, (int)offset);

            BufferPool.Release(buffer);
        }

        private static void WriteLiteralHeaderFieldWithIncrementalIndexing_NewName(Stream stream, string header, string value)
        {
            // https://http2.github.io/http2-spec/compression.html#literal.header.with.incremental.indexing

            UInt32 requiredBytes = 1 + RequiredBytesToEncodeString(header) + RequiredBytesToEncodeString(value);

            byte[] buffer = BufferPool.Get(requiredBytes, true);
            UInt32 offset = 0;

            buffer[offset++] = 0x40;
            EncodeString(header, buffer, ref offset);
            EncodeString(value, buffer, ref offset);

            stream.Write(buffer, 0, (int)offset);

            BufferPool.Release(buffer);
        }

        private static void WriteLiteralHeaderFieldWithoutIndexing_IndexedName(Stream stream, UInt32 index, string value)
        {
            // https://http2.github.io/http2-spec/compression.html#literal.header.without.indexing

            UInt32 requiredBytes = RequiredBytesToEncodeInteger(index, 4) + RequiredBytesToEncodeString(value);

            byte[] buffer = BufferPool.Get(requiredBytes, true);
            UInt32 offset = 0;

            buffer[0] = 0;
            EncodeInteger(index, 4, buffer, ref offset);
            EncodeString(value, buffer, ref offset);

            stream.Write(buffer, 0, (int)offset);

            BufferPool.Release(buffer);
        }

        private static void WriteLiteralHeaderFieldWithoutIndexing_NewName(Stream stream, string header, string value)
        {
            // https://http2.github.io/http2-spec/compression.html#literal.header.without.indexing

            UInt32 requiredBytes = 1 + RequiredBytesToEncodeString(header) + RequiredBytesToEncodeString(value);

            byte[] buffer = BufferPool.Get(requiredBytes, true);
            UInt32 offset = 0;

            buffer[offset++] = 0;
            EncodeString(header, buffer, ref offset);
            EncodeString(value, buffer, ref offset);

            stream.Write(buffer, 0, (int)offset);

            BufferPool.Release(buffer);
        }

        private static void WriteLiteralHeaderFieldNeverIndexed_IndexedName(Stream stream, UInt32 index, string value)
        {
            // https://http2.github.io/http2-spec/compression.html#literal.header.never.indexed

            UInt32 requiredBytes = RequiredBytesToEncodeInteger(index, 4) + RequiredBytesToEncodeString(value);

            byte[] buffer = BufferPool.Get(requiredBytes, true);
            UInt32 offset = 0;

            buffer[0] = 0x10;
            EncodeInteger(index, 4, buffer, ref offset);
            EncodeString(value, buffer, ref offset);

            stream.Write(buffer, 0, (int)offset);

            BufferPool.Release(buffer);
        }

        private static void WriteLiteralHeaderFieldNeverIndexed_NewName(Stream stream, string header, string value)
        {
            // https://http2.github.io/http2-spec/compression.html#literal.header.never.indexed

            UInt32 requiredBytes = 1 + RequiredBytesToEncodeString(header) + RequiredBytesToEncodeString(value);

            byte[] buffer = BufferPool.Get(requiredBytes, true);
            UInt32 offset = 0;

            buffer[offset++] = 0x10;
            EncodeString(header, buffer, ref offset);
            EncodeString(value, buffer, ref offset);

            stream.Write(buffer, 0, (int)offset);

            BufferPool.Release(buffer);
        }

        private static void WriteDynamicTableSizeUpdate(Stream stream, UInt16 maxSize)
        {
            // https://http2.github.io/http2-spec/compression.html#encoding.context.update

            UInt32 requiredBytes = RequiredBytesToEncodeInteger(maxSize, 5);

            byte[] buffer = BufferPool.Get(requiredBytes, true);
            UInt32 offset = 0;

            buffer[offset] = 0x20;
            EncodeInteger(maxSize, 5, buffer, ref offset);

            stream.Write(buffer, 0, (int)offset);

            BufferPool.Release(buffer);
        }

        private static UInt32 RequiredBytesToEncodeString(string str)
        {
            uint requiredBytesForRawStr = RequiredBytesToEncodeRawString(str);
            uint requiredBytesForHuffman = RequiredBytesToEncodeStringWithHuffman(str);
            requiredBytesForHuffman += RequiredBytesToEncodeInteger(requiredBytesForHuffman, 7);

            return Math.Min(requiredBytesForRawStr, requiredBytesForHuffman);
        }

        private static void EncodeString(string str, byte[] buffer, ref UInt32 offset)
        {
            uint requiredBytesForRawStr = RequiredBytesToEncodeRawString(str);
            uint requiredBytesForHuffman = RequiredBytesToEncodeStringWithHuffman(str);

            // if using huffman encoding would produce the same length, we choose raw encoding instead as it requires
            //  less CPU cicles
            if (requiredBytesForRawStr <= requiredBytesForHuffman + RequiredBytesToEncodeInteger(requiredBytesForHuffman, 7))
                EncodeRawStringTo(str, buffer, ref offset);
            else
                EncodeStringWithHuffman(str, requiredBytesForHuffman, buffer, ref offset);
        }

        // This calculates only the length of the compressed string,
        // additional header length must be calculated using the value returned by this function
        private static UInt32 RequiredBytesToEncodeStringWithHuffman(string str)
        {
            int requiredBytesForStr = System.Text.Encoding.UTF8.GetByteCount(str);
            byte[] strBytes = BufferPool.Get(requiredBytesForStr, true);

            System.Text.Encoding.UTF8.GetBytes(str, 0, str.Length, strBytes, 0);

            UInt32 requiredBits = 0;

            for (int i = 0; i < requiredBytesForStr; ++i)
                requiredBits += HuffmanEncoder.GetEntryForCodePoint(strBytes[i]).Bits;

            BufferPool.Release(strBytes);

            return (UInt32)((requiredBits / 8) + ((requiredBits % 8) == 0 ? 0 : 1));
        }

        private static void EncodeStringWithHuffman(string str, UInt32 encodedLength, byte[] buffer, ref UInt32 offset)
        {
            int requiredBytesForStr = System.Text.Encoding.UTF8.GetByteCount(str);
            byte[] strBytes = BufferPool.Get(requiredBytesForStr, true);

            System.Text.Encoding.UTF8.GetBytes(str, 0, str.Length, strBytes, 0);

            // 0. bit: huffman flag
            buffer[offset] = 0x80;

            // 1..7+ bit: length
            EncodeInteger(encodedLength, 7, buffer, ref offset);

            byte bufferBitIdx = 0;

            for (int i = 0; i < requiredBytesForStr; ++i)
                AddCodePointToBuffer(HuffmanEncoder.GetEntryForCodePoint(strBytes[i]), buffer, ref offset, ref bufferBitIdx);

            // https://http2.github.io/http2-spec/compression.html#string.literal.representation
            // As the Huffman-encoded data doesn't always end at an octet boundary, some padding is inserted after it,
            // up to the next octet boundary. To prevent this padding from being misinterpreted as part of the string literal,
            // the most significant bits of the code corresponding to the EOS (end-of-string) symbol are used.
            if (bufferBitIdx != 0)
                AddCodePointToBuffer(HuffmanEncoder.GetEntryForCodePoint(256), buffer, ref offset, ref bufferBitIdx, true);

            BufferPool.Release(strBytes);
        }

        private static void AddCodePointToBuffer(HuffmanEncoder.TableEntry code, byte[] buffer, ref UInt32 offset, ref byte bufferBitIdx, bool finishOnBoundary = false)
        {
            for (byte codeBitIdx = 1; codeBitIdx <= code.Bits; ++codeBitIdx)
            {
                byte bit = code.GetBitAtIdx(codeBitIdx);
                buffer[offset] = BufferHelper.SetBit(buffer[offset], bufferBitIdx, bit);

                // octet boundary reached, proceed to the next octet
                if (++bufferBitIdx == 8)
                {
                    if (++offset < buffer.Length)
                        buffer[offset] = 0;

                    if (finishOnBoundary)
                        return;

                    bufferBitIdx = 0;
                }
            }
        }

        private static UInt32 RequiredBytesToEncodeRawString(string str)
        {
            int requiredBytesForStr = System.Text.Encoding.UTF8.GetByteCount(str);
            int requiredBytesForLengthPrefix = RequiredBytesToEncodeInteger((UInt32)requiredBytesForStr, 7);

            return (UInt32)(requiredBytesForStr + requiredBytesForLengthPrefix);
        }

        // This method encodes a string without huffman encoding
        private static void EncodeRawStringTo(string str, byte[] buffer, ref UInt32 offset)
        {
            uint requiredBytesForStr = (uint)System.Text.Encoding.UTF8.GetByteCount(str);
            int requiredBytesForLengthPrefix = RequiredBytesToEncodeInteger((UInt32)requiredBytesForStr, 7);

            UInt32 originalOffset = offset;
            buffer[offset] = 0;
            EncodeInteger(requiredBytesForStr, 7, buffer, ref offset);

            // Zero out the huffman flag
            buffer[originalOffset] = BufferHelper.SetBit(buffer[originalOffset], 0, false);

            if (offset != originalOffset + requiredBytesForLengthPrefix)
                throw new Exception(string.Format("offset({0}) != originalOffset({1}) + requiredBytesForLengthPrefix({1})", offset, originalOffset, requiredBytesForLengthPrefix));

            System.Text.Encoding.UTF8.GetBytes(str, 0, str.Length, buffer, (int)offset);
            offset += requiredBytesForStr;
        }

        private static byte RequiredBytesToEncodeInteger(UInt32 value, byte N)
        {
            UInt32 maxValue = (1u << N) - 1;
            byte count = 0;

            // If the integer value is small enough, i.e., strictly less than 2^N-1, it is encoded within the N-bit prefix.
            if (value < maxValue)
            {
                count++;
            }
            else
            {
                // Otherwise, all the bits of the prefix are set to 1, and the value, decreased by 2^N-1
                count++;
                value -= maxValue;

                while (value >= 0x80)
                {
                    // The most significant bit of each octet is used as a continuation flag: its value is set to 1 except for the last octet in the list.
                    count++;
                    value = value / 0x80;
                }

                count++;
            }

            return count;
        }

        // https://http2.github.io/http2-spec/compression.html#integer.representation
        private static void EncodeInteger(UInt32 value, byte N, byte[] buffer, ref UInt32 offset)
        {
            // 2^N - 1
            UInt32 maxValue = (1u << N) - 1;

            // If the integer value is small enough, i.e., strictly less than 2^N-1, it is encoded within the N-bit prefix.
            if (value < maxValue)
            {
                buffer[offset++] |= (byte)value;
            }
            else
            {
                // Otherwise, all the bits of the prefix are set to 1, and the value, decreased by 2^N-1
                buffer[offset++] |= (byte)(0xFF >> (8 - N));
                value -= maxValue;

                while (value >= 0x80)
                {
                    // The most significant bit of each octet is used as a continuation flag: its value is set to 1 except for the last octet in the list.
                    buffer[offset++] = (byte)(0x80 | (0x7F & value));
                    value = value / 0x80;
                }

                buffer[offset++] = (byte)value;
            }
        }

        // https://http2.github.io/http2-spec/compression.html#integer.representation
        private static UInt32 DecodeInteger(byte N, byte[] buffer, ref UInt32 offset)
        {
            // The starting value is the value behind the mask of the N bits
            UInt32 value = (UInt32)(buffer[offset++] & (byte)(0xFF >> (8 - N)));

            // All N bits are 1s ? If so, we have at least one another byte to decode
            if (value == (1u << N) - 1)
            {
                byte shift = 0;

                do
                {
                    // The most significant bit is a continuation flag, so we have to mask it out
                    value += (UInt32)((buffer[offset] & 0x7F) << shift);
                    shift += 7;
                } while ((buffer[offset++] & 0x80) == 0x80);
            }

            return value;
        }

        // https://http2.github.io/http2-spec/compression.html#integer.representation
        private static UInt32 DecodeInteger(byte N, byte data, Stream stream)
        {
            // The starting value is the value behind the mask of the N bits
            UInt32 value = (UInt32)(data & (byte)(0xFF >> (8 - N)));

            // All N bits are 1s ? If so, we have at least one another byte to decode
            if (value == (1u << N) - 1)
            {
                byte shift = 0;

                do
                {
                    data = (byte)stream.ReadByte();

                    // The most significant bit is a continuation flag, so we have to mask it out
                    value += (UInt32)((data & 0x7F) << shift);
                    shift += 7;
                } while ((data & 0x80) == 0x80);
            }

            return value;
        }

        public override string ToString()
        {
            return this.requestTable.ToString() + this.responseTable.ToString();
        }
    }
}

#endif
