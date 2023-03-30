#if (!UNITY_WEBGL || UNITY_EDITOR) && !BESTHTTP_DISABLE_ALTERNATE_SSL && !BESTHTTP_DISABLE_HTTP2

using BestHTTP.Extensions;
using BestHTTP.PlatformSupport.Memory;
using System;
using System.Collections.Generic;

namespace BestHTTP.Connections.HTTP2
{
    // https://httpwg.org/specs/rfc7540.html#iana-frames
    public enum HTTP2FrameTypes : byte
    {
        DATA = 0x00,
        HEADERS = 0x01,
        PRIORITY = 0x02,
        RST_STREAM = 0x03,
        SETTINGS = 0x04,
        PUSH_PROMISE = 0x05,
        PING = 0x06,
        GOAWAY = 0x07,
        WINDOW_UPDATE = 0x08,
        CONTINUATION = 0x09,

        // https://tools.ietf.org/html/rfc7838#section-4
        ALT_SVC = 0x0A
    }

    [Flags]
    public enum HTTP2DataFlags : byte
    {
        None = 0x00,
        END_STREAM = 0x01,
        PADDED = 0x08,
    }

    [Flags]
    public enum HTTP2HeadersFlags : byte
    {
        None = 0x00,
        END_STREAM = 0x01,
        END_HEADERS = 0x04,
        PADDED = 0x08,
        PRIORITY = 0x20,
    }

    [Flags]
    public enum HTTP2SettingsFlags : byte
    {
        None = 0x00,
        ACK = 0x01,
    }

    [Flags]
    public enum HTTP2PushPromiseFlags : byte
    {
        None = 0x00,
        END_HEADERS = 0x04,
        PADDED = 0x08,
    }

    [Flags]
    public enum HTTP2PingFlags : byte
    {
        None = 0x00,
        ACK = 0x01,
    }

    [Flags]
    public enum HTTP2ContinuationFlags : byte
    {
        None = 0x00,
        END_HEADERS = 0x04,
    }

    public struct HTTP2FrameHeaderAndPayload
    {
        public UInt32 PayloadLength;
        public HTTP2FrameTypes Type;
        public byte Flags;
        public UInt32 StreamId;
        public byte[] Payload;

        public UInt32 PayloadOffset;
        public bool DontUseMemPool;

        public override string ToString()
        {
            return string.Format("[HTTP2FrameHeaderAndPayload Length: {0}, Type: {1}, Flags: {2}, StreamId: {3}, PayloadOffset: {4}, DontUseMemPool: {5}]",
                this.PayloadLength, this.Type, this.Flags.ToBinaryStr(), this.StreamId, this.PayloadOffset, this.DontUseMemPool);
        }

        public string PayloadAsHex()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder("[", (int)this.PayloadLength + 1);
            if (this.Payload != null && this.PayloadLength > 0)
            {
                uint idx = this.PayloadOffset;
                sb.Append(this.Payload[idx++]);
                for (int i = 1; i < this.PayloadLength; i++)
                    sb.AppendFormat(", {0:X2}", this.Payload[idx++]);
            }
            sb.Append("]");

            return sb.ToString();
        }
    }

    public struct HTTP2SettingsFrame
    {
        public readonly HTTP2FrameHeaderAndPayload Header;
        public HTTP2SettingsFlags Flags { get { return (HTTP2SettingsFlags)this.Header.Flags; } }
        public List<KeyValuePair<HTTP2Settings, UInt32>> Settings;

        public HTTP2SettingsFrame(HTTP2FrameHeaderAndPayload header)
        {
            this.Header = header;
            this.Settings = null;
        }

        public override string ToString()
        {
            string settings = null;
            if (this.Settings != null)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder("[");
                foreach (var kvp in this.Settings)
                    sb.AppendFormat("[{0}: {1}]", kvp.Key, kvp.Value);
                sb.Append("]");

                settings = sb.ToString();
            }

            return string.Format("[HTTP2SettingsFrame Header: {0}, Flags: {1}, Settings: {2}]", this.Header.ToString(), this.Flags, settings ?? "Empty");
        }
    }

    public struct HTTP2DataFrame
    {
        public readonly HTTP2FrameHeaderAndPayload Header;
        public HTTP2DataFlags Flags { get { return (HTTP2DataFlags)this.Header.Flags; } }

        public byte? PadLength;
        public UInt32 DataIdx;
        public byte[] Data;
        public uint DataLength;

        public HTTP2DataFrame(HTTP2FrameHeaderAndPayload header)
        {
            this.Header = header;
            this.PadLength = null;
            this.DataIdx = 0;
            this.Data = null;
            this.DataLength = 0;
        }

        public override string ToString()
        {
            return string.Format("[HTTP2DataFrame Header: {0}, Flags: {1}, PadLength: {2}, DataLength: {3}]",
                this.Header.ToString(),
                this.Flags,
                this.PadLength == null ? ":Empty" : this.PadLength.Value.ToString(),
                this.DataLength);
        }
    }

    public struct HTTP2HeadersFrame
    {
        public readonly HTTP2FrameHeaderAndPayload Header;
        public HTTP2HeadersFlags Flags { get { return (HTTP2HeadersFlags)this.Header.Flags; } }

        public byte? PadLength;
        public byte? IsExclusive;
        public UInt32? StreamDependency;
        public byte? Weight;
        public UInt32 HeaderBlockFragmentIdx;
        public byte[] HeaderBlockFragment;
        public UInt32 HeaderBlockFragmentLength;

        public HTTP2HeadersFrame(HTTP2FrameHeaderAndPayload header)
        {
            this.Header = header;
            this.PadLength = null;
            this.IsExclusive = null;
            this.StreamDependency = null;
            this.Weight = null;
            this.HeaderBlockFragmentIdx = 0;
            this.HeaderBlockFragment = null;
            this.HeaderBlockFragmentLength = 0;
        }

        public override string ToString()
        {
            return string.Format("[HTTP2HeadersFrame Header: {0}, Flags: {1}, PadLength: {2}, IsExclusive: {3}, StreamDependency: {4}, Weight: {5}, HeaderBlockFragmentLength: {6}]",
                this.Header.ToString(),
                this.Flags,
                this.PadLength == null ? ":Empty" : this.PadLength.Value.ToString(),
                this.IsExclusive == null ? "Empty" : this.IsExclusive.Value.ToString(),
                this.StreamDependency == null ? "Empty" : this.StreamDependency.Value.ToString(),
                this.Weight == null ? "Empty" : this.Weight.Value.ToString(),
                this.HeaderBlockFragmentLength);
        }
    }

    public struct HTTP2PriorityFrame
    {
        public readonly HTTP2FrameHeaderAndPayload Header;

        public byte IsExclusive;
        public UInt32 StreamDependency;
        public byte Weight;

        public HTTP2PriorityFrame(HTTP2FrameHeaderAndPayload header)
        {
            this.Header = header;
            this.IsExclusive = 0;
            this.StreamDependency = 0;
            this.Weight = 0;
        }

        public override string ToString()
        {
            return string.Format("[HTTP2PriorityFrame Header: {0}, IsExclusive: {1}, StreamDependency: {2}, Weight: {3}]",
                this.Header.ToString(), this.IsExclusive, this.StreamDependency, this.Weight);
        }
    }

    public struct HTTP2RSTStreamFrame
    {
        public readonly HTTP2FrameHeaderAndPayload Header;

        public UInt32 ErrorCode;
        public HTTP2ErrorCodes Error { get { return (HTTP2ErrorCodes)this.ErrorCode; } }

        public HTTP2RSTStreamFrame(HTTP2FrameHeaderAndPayload header)
        {
            this.Header = header;
            this.ErrorCode = 0;
        }

        public override string ToString()
        {
            return string.Format("[HTTP2RST_StreamFrame Header: {0}, Error: {1}({2})]", this.Header.ToString(), this.Error, this.ErrorCode);
        }
    }

    public struct HTTP2PushPromiseFrame
    {
        public readonly HTTP2FrameHeaderAndPayload Header;
        public HTTP2PushPromiseFlags Flags { get { return (HTTP2PushPromiseFlags)this.Header.Flags; } }

        public byte? PadLength;
        public byte ReservedBit;
        public UInt32 PromisedStreamId;
        public UInt32 HeaderBlockFragmentIdx;
        public byte[] HeaderBlockFragment;
        public UInt32 HeaderBlockFragmentLength;

        public HTTP2PushPromiseFrame(HTTP2FrameHeaderAndPayload header)
        {
            this.Header = header;
            this.PadLength = null;
            this.ReservedBit = 0;
            this.PromisedStreamId = 0;
            this.HeaderBlockFragmentIdx = 0;
            this.HeaderBlockFragment = null;
            this.HeaderBlockFragmentLength = 0;
        }

        public override string ToString()
        {
            return string.Format("[HTTP2Push_PromiseFrame Header: {0}, Flags: {1}, PadLength: {2}, ReservedBit: {3}, PromisedStreamId: {4}, HeaderBlockFragmentLength: {5}]",
                this.Header.ToString(),
                this.Flags,
                this.PadLength == null ? "Empty" : this.PadLength.Value.ToString(),
                this.ReservedBit,
                this.PromisedStreamId,
                this.HeaderBlockFragmentLength);
        }
    }

    public struct HTTP2PingFrame
    {
        public readonly HTTP2FrameHeaderAndPayload Header;
        public HTTP2PingFlags Flags { get { return (HTTP2PingFlags)this.Header.Flags; } }

        public readonly byte[] OpaqueData;
        public readonly byte OpaqueDataLength;

        public HTTP2PingFrame(HTTP2FrameHeaderAndPayload header)
        {
            this.Header = header;
            this.OpaqueData = BufferPool.Get(8, true);
            this.OpaqueDataLength = 8;
        }

        public override string ToString()
        {
            return string.Format("[HTTP2PingFrame Header: {0}, Flags: {1}, OpaqueData: {2}]",
                this.Header.ToString(),
                this.Flags,
                SecureProtocol.Org.BouncyCastle.Utilities.Encoders.Hex.ToHexString(this.OpaqueData, 0, this.OpaqueDataLength));
        }
    }

    public struct HTTP2GoAwayFrame
    {
        public readonly HTTP2FrameHeaderAndPayload Header;
        public HTTP2ErrorCodes Error { get { return (HTTP2ErrorCodes)this.ErrorCode; } }

        public byte ReservedBit;
        public UInt32 LastStreamId;
        public UInt32 ErrorCode;
        public byte[] AdditionalDebugData;
        public UInt32 AdditionalDebugDataLength;

        public HTTP2GoAwayFrame(HTTP2FrameHeaderAndPayload header)
        {
            this.Header = header;
            this.ReservedBit = 0;
            this.LastStreamId = 0;
            this.ErrorCode = 0;
            this.AdditionalDebugData = null;
            this.AdditionalDebugDataLength = 0;
        }

        public override string ToString()
        {
            return string.Format("[HTTP2GoAwayFrame Header: {0}, ReservedBit: {1}, LastStreamId: {2}, Error: {3}({4}), AdditionalDebugData({5}): {6}]",
                this.Header.ToString(),
                this.ReservedBit,
                this.LastStreamId,
                this.Error,
                this.ErrorCode,
                this.AdditionalDebugDataLength,
                this.AdditionalDebugData == null ? "Empty" : SecureProtocol.Org.BouncyCastle.Utilities.Encoders.Hex.ToHexString(this.AdditionalDebugData, 0, (int)this.AdditionalDebugDataLength));
        }
    }

    public struct HTTP2WindowUpdateFrame
    {
        public readonly HTTP2FrameHeaderAndPayload Header;

        public byte ReservedBit;
        public UInt32 WindowSizeIncrement;

        public HTTP2WindowUpdateFrame(HTTP2FrameHeaderAndPayload header)
        {
            this.Header = header;
            this.ReservedBit = 0;
            this.WindowSizeIncrement = 0;
        }

        public override string ToString()
        {
            return string.Format("[HTTP2WindowUpdateFrame Header: {0}, ReservedBit: {1}, WindowSizeIncrement: {2}]",
                this.Header.ToString(), this.ReservedBit, this.WindowSizeIncrement);
        }
    }

    public struct HTTP2ContinuationFrame
    {
        public readonly HTTP2FrameHeaderAndPayload Header;
        public HTTP2ContinuationFlags Flags { get { return (HTTP2ContinuationFlags)this.Header.Flags; } }

        public byte[] HeaderBlockFragment;
        public UInt32 HeaderBlockFragmentLength { get { return this.Header.PayloadLength; } }

        public HTTP2ContinuationFrame(HTTP2FrameHeaderAndPayload header)
        {
            this.Header = header;
            this.HeaderBlockFragment = null;
        }

        public override string ToString()
        {
            return string.Format("[HTTP2ContinuationFrame Header: {0}, Flags: {1}, HeaderBlockFragmentLength: {2}]",
                this.Header.ToString(),
                this.Flags,
                this.HeaderBlockFragmentLength);
        }
    }

    /// <summary>
    /// https://tools.ietf.org/html/rfc7838#section-4
    /// </summary>
    public struct HTTP2AltSVCFrame
    {
        public readonly HTTP2FrameHeaderAndPayload Header;

        public string Origin;
        public string AltSvcFieldValue;

        public HTTP2AltSVCFrame(HTTP2FrameHeaderAndPayload header)
        {
            this.Header = header;
            this.Origin = null;
            this.AltSvcFieldValue = null;
        }
    }
}

#endif
