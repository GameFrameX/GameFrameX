#if (!UNITY_WEBGL || UNITY_EDITOR) && !BESTHTTP_DISABLE_ALTERNATE_SSL && !BESTHTTP_DISABLE_HTTP2

using BestHTTP.Extensions;
using BestHTTP.PlatformSupport.Memory;
using System;
using System.Collections.Generic;
using System.IO;

namespace BestHTTP.Connections.HTTP2
{
    // https://httpwg.org/specs/rfc7540.html#ErrorCodes
    public enum HTTP2ErrorCodes
    {
        NO_ERROR = 0x00,
        PROTOCOL_ERROR = 0x01,
        INTERNAL_ERROR = 0x02,
        FLOW_CONTROL_ERROR = 0x03,
        SETTINGS_TIMEOUT = 0x04,
        STREAM_CLOSED = 0x05,
        FRAME_SIZE_ERROR = 0x06,
        REFUSED_STREAM = 0x07,
        CANCEL = 0x08,
        COMPRESSION_ERROR = 0x09,
        CONNECT_ERROR = 0x0A,
        ENHANCE_YOUR_CALM = 0x0B,
        INADEQUATE_SECURITY = 0x0C,
        HTTP_1_1_REQUIRED = 0x0D
    }

    public static class HTTP2FrameHelper
    {
        public static HTTP2ContinuationFrame ReadContinuationFrame(HTTP2FrameHeaderAndPayload header)
        {
            // https://httpwg.org/specs/rfc7540.html#CONTINUATION

            HTTP2ContinuationFrame frame = new HTTP2ContinuationFrame(header);

            frame.HeaderBlockFragment = header.Payload;
            header.Payload = null;

            return frame;
        }

        public static HTTP2WindowUpdateFrame ReadWindowUpdateFrame(HTTP2FrameHeaderAndPayload header)
        {
            // https://httpwg.org/specs/rfc7540.html#WINDOW_UPDATE

            HTTP2WindowUpdateFrame frame = new HTTP2WindowUpdateFrame(header);

            frame.ReservedBit = BufferHelper.ReadBit(header.Payload[0], 0);
            frame.WindowSizeIncrement = BufferHelper.ReadUInt31(header.Payload, 0);

            return frame;
        }

        public static HTTP2GoAwayFrame ReadGoAwayFrame(HTTP2FrameHeaderAndPayload header)
        {
            // https://httpwg.org/specs/rfc7540.html#GOAWAY
            //      str id      error
            // | 0, 1, 2, 3 | 4, 5, 6, 7 | ...

            HTTP2GoAwayFrame frame = new HTTP2GoAwayFrame(header);

            frame.ReservedBit = BufferHelper.ReadBit(header.Payload[0], 0);
            frame.LastStreamId = BufferHelper.ReadUInt31(header.Payload, 0);
            frame.ErrorCode = BufferHelper.ReadUInt32(header.Payload, 4);

            frame.AdditionalDebugDataLength = header.PayloadLength - 8;
            if (frame.AdditionalDebugDataLength > 0)
            {
                frame.AdditionalDebugData = BufferPool.Get(frame.AdditionalDebugDataLength, true);
                Array.Copy(header.Payload, 8, frame.AdditionalDebugData, 0, frame.AdditionalDebugDataLength);
            }

            return frame;
        }

        public static HTTP2PingFrame ReadPingFrame(HTTP2FrameHeaderAndPayload header)
        {
            // https://httpwg.org/specs/rfc7540.html#PING

            HTTP2PingFrame frame = new HTTP2PingFrame(header);

            Array.Copy(header.Payload, 0, frame.OpaqueData, 0, frame.OpaqueDataLength);

            return frame;
        }

        public static HTTP2PushPromiseFrame ReadPush_PromiseFrame(HTTP2FrameHeaderAndPayload header)
        {
            // https://httpwg.org/specs/rfc7540.html#PUSH_PROMISE

            HTTP2PushPromiseFrame frame = new HTTP2PushPromiseFrame(header);
            frame.HeaderBlockFragmentLength = header.PayloadLength - 4; // PromisedStreamId

            bool isPadded = (frame.Flags & HTTP2PushPromiseFlags.PADDED) != 0;
            if (isPadded)
            {
                frame.PadLength = header.Payload[0];
                frame.HeaderBlockFragmentLength -= (uint)(1 + (frame.PadLength ?? 0));
            }

            frame.ReservedBit = BufferHelper.ReadBit(header.Payload[1], 0);
            frame.PromisedStreamId = BufferHelper.ReadUInt31(header.Payload, 1);

            frame.HeaderBlockFragmentIdx = (UInt32)(isPadded ? 5 : 4);
            frame.HeaderBlockFragment = header.Payload;
            header.Payload = null;

            return frame;
        }

        public static HTTP2RSTStreamFrame ReadRST_StreamFrame(HTTP2FrameHeaderAndPayload header)
        {
            // https://httpwg.org/specs/rfc7540.html#RST_STREAM

            HTTP2RSTStreamFrame frame = new HTTP2RSTStreamFrame(header);
            frame.ErrorCode = BufferHelper.ReadUInt32(header.Payload, 0);

            return frame;
        }

        public static HTTP2PriorityFrame ReadPriorityFrame(HTTP2FrameHeaderAndPayload header)
        {
            // https://httpwg.org/specs/rfc7540.html#PRIORITY

            if (header.PayloadLength != 5)
            {
                //throw FRAME_SIZE_ERROR
            }

            HTTP2PriorityFrame frame = new HTTP2PriorityFrame(header);

            frame.IsExclusive = BufferHelper.ReadBit(header.Payload[0], 0);
            frame.StreamDependency = BufferHelper.ReadUInt31(header.Payload, 0);
            frame.Weight = header.Payload[4];

            return frame;
        }

        public static HTTP2HeadersFrame ReadHeadersFrame(HTTP2FrameHeaderAndPayload header)
        {
            // https://httpwg.org/specs/rfc7540.html#HEADERS

            HTTP2HeadersFrame frame = new HTTP2HeadersFrame(header);
            frame.HeaderBlockFragmentLength = header.PayloadLength;

            bool isPadded = (frame.Flags & HTTP2HeadersFlags.PADDED) != 0;
            bool isPriority = (frame.Flags & HTTP2HeadersFlags.PRIORITY) != 0;

            int payloadIdx = 0;

            if (isPadded)
            {
                frame.PadLength = header.Payload[payloadIdx++];

                uint subLength = (uint)(1 + (frame.PadLength ?? 0));
                if (subLength <= frame.HeaderBlockFragmentLength)
                    frame.HeaderBlockFragmentLength -= subLength;
                //else
                //    throw PROTOCOL_ERROR;
            }

            if (isPriority)
            {
                frame.IsExclusive = BufferHelper.ReadBit(header.Payload[payloadIdx], 0);
                frame.StreamDependency = BufferHelper.ReadUInt31(header.Payload, payloadIdx);
                payloadIdx += 4;
                frame.Weight = header.Payload[payloadIdx++];

                uint subLength = 5;
                if (subLength <= frame.HeaderBlockFragmentLength)
                    frame.HeaderBlockFragmentLength -= subLength;
                //else
                //    throw PROTOCOL_ERROR;
            }

            frame.HeaderBlockFragmentIdx = (UInt32)payloadIdx;
            frame.HeaderBlockFragment = header.Payload;

            return frame;
        }

        public static HTTP2DataFrame ReadDataFrame(HTTP2FrameHeaderAndPayload header)
        {
            // https://httpwg.org/specs/rfc7540.html#DATA

            HTTP2DataFrame frame = new HTTP2DataFrame(header);

            frame.DataLength = header.PayloadLength;

            bool isPadded = (frame.Flags & HTTP2DataFlags.PADDED) != 0;
            if (isPadded)
            {
                frame.PadLength = header.Payload[0];

                uint subLength = (uint)(1 + (frame.PadLength ?? 0));
                if (subLength <= frame.DataLength)
                    frame.DataLength -= subLength;
                //else
                //    throw PROTOCOL_ERROR;
            }

            frame.DataIdx = (UInt32)(isPadded ? 1 : 0);
            frame.Data = header.Payload;
            header.Payload = null;

            return frame;
        }

        public static HTTP2AltSVCFrame ReadAltSvcFrame(HTTP2FrameHeaderAndPayload header)
        {
            HTTP2AltSVCFrame frame = new HTTP2AltSVCFrame(header);
            
            // Implement

            return frame;
        }

        public static void StreamRead(Stream stream, byte[] buffer, int offset, uint count)
        {
            if (count == 0)
                return;

            uint sumRead = 0;
            do
            {
                int readCount = (int)(count - sumRead);
                int streamReadCount = stream.Read(buffer, (int)(offset + sumRead), readCount);
                if (streamReadCount <= 0 && readCount > 0)
                    throw new Exception("TCP Stream closed!");
                sumRead += (uint)streamReadCount;
            } while (sumRead < count);
        }

        public static PooledBuffer HeaderAsBinary(HTTP2FrameHeaderAndPayload header)
        {
            // https://httpwg.org/specs/rfc7540.html#FrameHeader

            var buffer = BufferPool.Get(9, true);

            BufferHelper.SetUInt24(buffer, 0, header.PayloadLength);
            buffer[3] = (byte)header.Type;
            buffer[4] = header.Flags;
            BufferHelper.SetUInt31(buffer, 5, header.StreamId);

            return new PooledBuffer { Data = buffer, Length = 9 };
        }

        public static HTTP2FrameHeaderAndPayload ReadHeader(Stream stream)
        {
            byte[] buffer = BufferPool.Get(9, true);

            StreamRead(stream, buffer, 0, 9);

            HTTP2FrameHeaderAndPayload header = new HTTP2FrameHeaderAndPayload();

            header.PayloadLength = BufferHelper.ReadUInt24(buffer, 0);
            header.Type = (HTTP2FrameTypes)buffer[3];
            header.Flags = buffer[4];
            header.StreamId = BufferHelper.ReadUInt31(buffer, 5);

            BufferPool.Release(buffer);

            header.Payload = BufferPool.Get(header.PayloadLength, true);
            StreamRead(stream, header.Payload, 0, header.PayloadLength);

            return header;
        }

        public static HTTP2SettingsFrame ReadSettings(HTTP2FrameHeaderAndPayload header)
        {
            HTTP2SettingsFrame frame = new HTTP2SettingsFrame(header);

            if (header.PayloadLength > 0)
            {
                int kvpCount = (int)(header.PayloadLength / 6);

                frame.Settings = new List<KeyValuePair<HTTP2Settings, uint>>(kvpCount);
                for (int i = 0; i < kvpCount; ++i)
                {
                    HTTP2Settings key = (HTTP2Settings)BufferHelper.ReadUInt16(header.Payload, i * 6);
                    UInt32 value = BufferHelper.ReadUInt32(header.Payload, (i * 6) + 2);

                    frame.Settings.Add(new KeyValuePair<HTTP2Settings, uint>(key, value));
                }
            }

            return frame;
        }

        public static HTTP2FrameHeaderAndPayload CreateACKSettingsFrame()
        {
            HTTP2FrameHeaderAndPayload frame = new HTTP2FrameHeaderAndPayload();
            frame.Type = HTTP2FrameTypes.SETTINGS;
            frame.Flags = (byte)HTTP2SettingsFlags.ACK;

            return frame;
        }

        public static HTTP2FrameHeaderAndPayload CreateSettingsFrame(List<KeyValuePair<HTTP2Settings, UInt32>> settings)
        {
            HTTP2FrameHeaderAndPayload frame = new HTTP2FrameHeaderAndPayload();
            frame.Type = HTTP2FrameTypes.SETTINGS;
            frame.Flags = 0;
            frame.PayloadLength = (UInt32)settings.Count * 6;

            frame.Payload = BufferPool.Get(frame.PayloadLength, true);

            for (int i = 0; i < settings.Count; ++i)
            {
                BufferHelper.SetUInt16(frame.Payload, i * 6, (UInt16)settings[i].Key);
                BufferHelper.SetUInt32(frame.Payload, (i * 6) + 2, settings[i].Value);
            }

            return frame;
        }

        public static HTTP2FrameHeaderAndPayload CreatePingFrame(HTTP2PingFlags flags = HTTP2PingFlags.None)
        {
            // https://httpwg.org/specs/rfc7540.html#PING

            HTTP2FrameHeaderAndPayload frame = new HTTP2FrameHeaderAndPayload();
            frame.Type = HTTP2FrameTypes.PING;
            frame.Flags = (byte)flags;
            frame.StreamId = 0;
            frame.Payload = BufferPool.Get(8, true);
            frame.PayloadLength = 8;

            return frame;
        }

        public static HTTP2FrameHeaderAndPayload CreateWindowUpdateFrame(UInt32 streamId, UInt32 windowSizeIncrement)
        {
            // https://httpwg.org/specs/rfc7540.html#WINDOW_UPDATE

            HTTP2FrameHeaderAndPayload frame = new HTTP2FrameHeaderAndPayload();
            frame.Type = HTTP2FrameTypes.WINDOW_UPDATE;
            frame.Flags = 0;
            frame.StreamId = streamId;
            frame.Payload = BufferPool.Get(4, true);
            frame.PayloadLength = 4;

            BufferHelper.SetBit(0, 0, 0);
            BufferHelper.SetUInt31(frame.Payload, 0, windowSizeIncrement);

            return frame;
        }

        public static HTTP2FrameHeaderAndPayload CreateGoAwayFrame(UInt32 lastStreamId, HTTP2ErrorCodes error)
        {
            // https://httpwg.org/specs/rfc7540.html#GOAWAY

            HTTP2FrameHeaderAndPayload frame = new HTTP2FrameHeaderAndPayload();
            frame.Type = HTTP2FrameTypes.GOAWAY;
            frame.Flags = 0;
            frame.StreamId = 0;
            frame.Payload = BufferPool.Get(8, true);
            frame.PayloadLength = 8;

            BufferHelper.SetUInt31(frame.Payload, 0, lastStreamId);
            BufferHelper.SetUInt31(frame.Payload, 4, (UInt32)error);

            return frame;
        }

        public static HTTP2FrameHeaderAndPayload CreateRSTFrame(UInt32 streamId, HTTP2ErrorCodes errorCode)
        {
            // https://httpwg.org/specs/rfc7540.html#RST_STREAM

            HTTP2FrameHeaderAndPayload frame = new HTTP2FrameHeaderAndPayload();
            frame.Type = HTTP2FrameTypes.RST_STREAM;
            frame.Flags = 0;
            frame.StreamId = streamId;
            frame.Payload = BufferPool.Get(4, true);
            frame.PayloadLength = 4;

            BufferHelper.SetUInt32(frame.Payload, 0, (UInt32)errorCode);

            return frame;
        }
    }
}

#endif
