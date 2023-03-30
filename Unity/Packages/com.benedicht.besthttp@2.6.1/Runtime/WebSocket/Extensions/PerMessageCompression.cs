#if !BESTHTTP_DISABLE_WEBSOCKET && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using BestHTTP.Extensions;
using BestHTTP.WebSocket.Frames;
using BestHTTP.Decompression.Zlib;
using BestHTTP.PlatformSupport.Memory;

namespace BestHTTP.WebSocket.Extensions
{
    /// <summary>
    /// Compression Extensions for WebSocket implementation.
    /// http://tools.ietf.org/html/rfc7692
    /// </summary>
    public sealed class PerMessageCompression : IExtension
    {
        public const int MinDataLengthToCompressDefault = 256;

        private static readonly byte[] Trailer = new byte[] { 0x00, 0x00, 0xFF, 0xFF };

        #region Public Properties

        /// <summary>
        /// By including this extension parameter in an extension negotiation offer, a client informs the peer server
        /// of a hint that even if the server doesn't include the "client_no_context_takeover" extension parameter in
        /// the corresponding extension negotiation response to the offer, the client is not going to use context takeover.
        /// </summary>
        public bool ClientNoContextTakeover { get; private set; }

        /// <summary>
        /// By including this extension parameter in an extension negotiation offer, a client prevents the peer server from using context takeover.
        /// </summary>
        public bool ServerNoContextTakeover { get; private set; }

        /// <summary>
        /// This parameter indicates the base-2 logarithm of the LZ77 sliding window size of the client context.
        /// </summary>
        public int ClientMaxWindowBits { get; private set; }

        /// <summary>
        /// This parameter indicates the base-2 logarithm of the LZ77 sliding window size of the server context.
        /// </summary>
        public int ServerMaxWindowBits { get; private set; }

        /// <summary>
        /// The compression level that the client will use to compress the frames.
        /// </summary>
        public CompressionLevel Level { get; private set; }

        /// <summary>
        /// What minimum data length will trigger the compression.
        /// </summary>
        public int MinimumDataLegthToCompress { get; set; }

        #endregion

        #region Private fields

        /// <summary>
        /// Cached object to support context takeover.
        /// </summary>
        private BufferPoolMemoryStream compressorOutputStream;
        private DeflateStream compressorDeflateStream;

        /// <summary>
        /// Cached object to support context takeover.
        /// </summary>
        private BufferPoolMemoryStream decompressorInputStream;
        private BufferPoolMemoryStream decompressorOutputStream;
        private DeflateStream decompressorDeflateStream;

        #endregion

        public PerMessageCompression()
            :this(CompressionLevel.Default, false, false, ZlibConstants.WindowBitsMax, ZlibConstants.WindowBitsMax, MinDataLengthToCompressDefault)
        { }

        public PerMessageCompression(CompressionLevel level,
                                     bool clientNoContextTakeover,
                                     bool serverNoContextTakeover,
                                     int desiredClientMaxWindowBits,
                                     int desiredServerMaxWindowBits,
                                     int minDatalengthToCompress)
        {
            this.Level = level;
            this.ClientNoContextTakeover = clientNoContextTakeover;
            this.ServerNoContextTakeover = serverNoContextTakeover;
            this.ClientMaxWindowBits = desiredClientMaxWindowBits;
            this.ServerMaxWindowBits = desiredServerMaxWindowBits;
            this.MinimumDataLegthToCompress = minDatalengthToCompress;
        }

        #region IExtension Implementation

        /// <summary>
        /// This will start the permessage-deflate negotiation process.
        /// <seealso href="http://tools.ietf.org/html/rfc7692#section-5.1"/>
        /// </summary>
        public void AddNegotiation(HTTPRequest request)
        {
            // The default header value that we will send out minimum.
            string headerValue = "permessage-deflate";


            // http://tools.ietf.org/html/rfc7692#section-7.1.1.1
            // A client MAY include the "server_no_context_takeover" extension parameter in an extension negotiation offer.  This extension parameter has no value.
            // By including this extension parameter in an extension negotiation offer, a client prevents the peer server from using context takeover.
            // If the peer server doesn't use context takeover, the client doesn't need to reserve memory to retain the LZ77 sliding window between messages.
            if (this.ServerNoContextTakeover)
                headerValue += "; server_no_context_takeover";


            // http://tools.ietf.org/html/rfc7692#section-7.1.1.2
            // A client MAY include the "client_no_context_takeover" extension parameter in an extension negotiation offer.
            // This extension parameter has no value.  By including this extension parameter in an extension negotiation offer,
            // a client informs the peer server of a hint that even if the server doesn't include the "client_no_context_takeover"
            // extension parameter in the corresponding extension negotiation response to the offer, the client is not going to use context takeover.
            if (this.ClientNoContextTakeover)
                headerValue += "; client_no_context_takeover";

            // http://tools.ietf.org/html/rfc7692#section-7.1.2.1
            // By including this parameter in an extension negotiation offer, a client limits the LZ77 sliding window size that the server
            // will use to compress messages.If the peer server uses a small LZ77 sliding window to compress messages, the client can reduce the memory needed for the LZ77 sliding window.
            if (this.ServerMaxWindowBits != ZlibConstants.WindowBitsMax)
                headerValue += "; server_max_window_bits=" + this.ServerMaxWindowBits.ToString();
            else
                // Absence of this parameter in an extension negotiation offer indicates that the client can receive messages compressed using an LZ77 sliding window of up to 32,768 bytes.
                this.ServerMaxWindowBits = ZlibConstants.WindowBitsMax;

            // http://tools.ietf.org/html/rfc7692#section-7.1.2.2
            // By including this parameter in an offer, a client informs the peer server that the client supports the "client_max_window_bits"
            // extension parameter in an extension negotiation response and, optionally, a hint by attaching a value to the parameter.
            if (this.ClientMaxWindowBits != ZlibConstants.WindowBitsMax)
                headerValue += "; client_max_window_bits=" + this.ClientMaxWindowBits.ToString();
            else
            {
                headerValue += "; client_max_window_bits";

                // If the "client_max_window_bits" extension parameter in an extension negotiation offer has a value, the parameter also informs the
                // peer server of a hint that even if the server doesn't include the "client_max_window_bits" extension parameter in the corresponding
                // extension negotiation response with a value greater than the one in the extension negotiation offer or if the server doesn't include
                // the extension parameter at all, the client is not going to use an LZ77 sliding window size greater than the size specified
                // by the value in the extension negotiation offer to compress messages.
                this.ClientMaxWindowBits = ZlibConstants.WindowBitsMax;
            }

            // Add the new header to the request.
            request.AddHeader("Sec-WebSocket-Extensions", headerValue);
        }

        public bool ParseNegotiation(WebSocketResponse resp)
        {
            // Search for any returned neogitation offer
            var headerValues = resp.GetHeaderValues("Sec-WebSocket-Extensions");
            if (headerValues == null)
                return false;

            for (int i = 0; i < headerValues.Count; ++i)
            {
                // If found, tokenize it
                HeaderParser parser = new HeaderParser(headerValues[i]);

                for (int cv = 0; cv < parser.Values.Count; ++cv)
                {
                    HeaderValue value = parser.Values[i];

                    if (!string.IsNullOrEmpty(value.Key) && value.Key.StartsWith("permessage-deflate", StringComparison.OrdinalIgnoreCase))
                    {
                        HTTPManager.Logger.Information("PerMessageCompression", "Enabled with header: " + headerValues[i]);

                        HeaderValue option;
                        if (value.TryGetOption("client_no_context_takeover", out option))
                            this.ClientNoContextTakeover = true;

                        if (value.TryGetOption("server_no_context_takeover", out option))
                            this.ServerNoContextTakeover = true;

                        if (value.TryGetOption("client_max_window_bits", out option))
                            if (option.HasValue)
                            {
                                int windowBits;
                                if (int.TryParse(option.Value, out windowBits))
                                    this.ClientMaxWindowBits = windowBits;
                            }

                        if (value.TryGetOption("server_max_window_bits", out option))
                            if (option.HasValue)
                            {
                                int windowBits;
                                if (int.TryParse(option.Value, out windowBits))
                                    this.ServerMaxWindowBits = windowBits;
                            }

                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// IExtension implementation to set the Rsv1 flag in the header if we are we will want to compress the data
        /// in the writer.
        /// </summary>
        public byte GetFrameHeader(WebSocketFrame writer, byte inFlag)
        {
            // http://tools.ietf.org/html/rfc7692#section-7.2.3.1
            //  the RSV1 bit is set only on the first frame.
            if ((writer.Type == WebSocketFrameTypes.Binary || writer.Type == WebSocketFrameTypes.Text) &&
                writer.Data != null && writer.DataLength >= this.MinimumDataLegthToCompress)
                return (byte)(inFlag | 0x40);
            else
                return inFlag;
        }

        /// <summary>
        /// IExtension implementation to be able to compress the data hold in the writer.
        /// </summary>
        public byte[] Encode(WebSocketFrame writer)
        {
            if (writer.Data == null)
                return BufferPool.NoData;

            // Is compressing enabled for this frame? If so, compress it.
            if ((writer.Header & 0x40) != 0)
                return Compress(writer.Data, writer.DataLength);
            else
                return writer.Data;
        }

        /// <summary>
        /// IExtension implementation to possible decompress the data.
        /// </summary>
        public byte[] Decode(byte header, byte[] data, int length)
        {
            // Is the server compressed the data? If so, decompress it.
            if ((header & 0x40) != 0)
                return Decompress(data, length);
            else
                return data;
        }

        #endregion

        #region Private Helper Functions

        /// <summary>
        /// A function to compress and return the data parameter with possible context takeover support (reusing the DeflateStream).
        /// </summary>
        private byte[] Compress(byte[] data, int length)
        {
            if (compressorOutputStream == null)
                compressorOutputStream = new BufferPoolMemoryStream();
            compressorOutputStream.SetLength(0);

            if (compressorDeflateStream == null)
            {
                compressorDeflateStream = new DeflateStream(compressorOutputStream, CompressionMode.Compress, this.Level, true, this.ClientMaxWindowBits);
                compressorDeflateStream.FlushMode = FlushType.Sync;
            }

            byte[] result = null;
            try
            {
                compressorDeflateStream.Write(data, 0, length);
                compressorDeflateStream.Flush();

                compressorOutputStream.Position = 0;

                // http://tools.ietf.org/html/rfc7692#section-7.2.1
                // Remove 4 octets (that are 0x00 0x00 0xff 0xff) from the tail end. After this step, the last octet of the compressed data contains (possibly part of) the DEFLATE header bits with the "BTYPE" bits set to 00.
                compressorOutputStream.SetLength(compressorOutputStream.Length - 4);

                result = compressorOutputStream.ToArray();
            }
            finally
            {
                if (this.ClientNoContextTakeover)
                {
                    compressorDeflateStream.Dispose();
                    compressorDeflateStream = null;
                }
            }

            return result;
        }

        /// <summary>
        /// A function to decompress and return the data parameter with possible context takeover support (reusing the DeflateStream).
        /// </summary>
        private byte[] Decompress(byte[] data, int length)
        {
            if (decompressorInputStream == null)
                decompressorInputStream = new BufferPoolMemoryStream(length + 4);

            decompressorInputStream.Write(data, 0, length);

            // http://tools.ietf.org/html/rfc7692#section-7.2.2
            // Append 4 octets of 0x00 0x00 0xff 0xff to the tail end of the payload of the message.
            decompressorInputStream.Write(PerMessageCompression.Trailer, 0, PerMessageCompression.Trailer.Length);

            decompressorInputStream.Position = 0;

            if (decompressorDeflateStream == null)
            {
                decompressorDeflateStream = new DeflateStream(decompressorInputStream, CompressionMode.Decompress, CompressionLevel.Default, true, this.ServerMaxWindowBits);
                decompressorDeflateStream.FlushMode = FlushType.Sync;
            }

            if (decompressorOutputStream == null)
                decompressorOutputStream = new BufferPoolMemoryStream();
            decompressorOutputStream.SetLength(0);

            byte[] copyBuffer = BufferPool.Get(1024, true);
            int readCount;
            while ((readCount = decompressorDeflateStream.Read(copyBuffer, 0, copyBuffer.Length)) != 0)
                decompressorOutputStream.Write(copyBuffer, 0, readCount);

            BufferPool.Release(copyBuffer);

            decompressorDeflateStream.SetLength(0);

            byte[] result = decompressorOutputStream.ToArray();

            if (this.ServerNoContextTakeover)
            {
                decompressorDeflateStream.Dispose();
                decompressorDeflateStream = null;
            }

            return result;
        }

        #endregion
    }
}

#endif
