#if (!UNITY_WEBGL || UNITY_EDITOR) && !BESTHTTP_DISABLE_ALTERNATE_SSL && !BESTHTTP_DISABLE_HTTP2

using BestHTTP.Core;
using BestHTTP.Extensions;
using BestHTTP.PlatformSupport.Memory;
using System;
using System.Collections.Generic;
using System.IO;

namespace BestHTTP.Connections.HTTP2
{
    public sealed class HTTP2Response : HTTPResponse
    {
        // For progress report
        public long ExpectedContentLength { get; private set; }
        public bool IsCompressed { get; private set; }

        public HTTP2Response(HTTPRequest request, bool isFromCache)
            : base(request, isFromCache)
        {
            this.VersionMajor = 2;
            this.VersionMinor = 0;
        }

        internal void AddHeaders(List<KeyValuePair<string, string>> headers)
        {
            this.ExpectedContentLength = -1;
            Dictionary<string, List<string>> newHeaders = this.baseRequest.OnHeadersReceived != null ? new Dictionary<string, List<string>>() : null;

            for (int i = 0; i < headers.Count; ++i)
            {
                KeyValuePair<string, string> header = headers[i];

                if (header.Key.Equals(":status", StringComparison.Ordinal))
                {
                    base.StatusCode = int.Parse(header.Value);
                    base.Message = string.Empty;
                }
                else
                {
                    if (!this.IsCompressed && header.Key.Equals("content-encoding", StringComparison.OrdinalIgnoreCase))
                    {
                        this.IsCompressed = true;
                    }
                    else if (base.baseRequest.OnDownloadProgress != null && header.Key.Equals("content-length", StringComparison.OrdinalIgnoreCase))
                    {
                        long contentLength;
                        if (long.TryParse(header.Value, out contentLength))
                            this.ExpectedContentLength = contentLength;
                        else
                            HTTPManager.Logger.Information("HTTP2Response", string.Format("AddHeaders - Can't parse Content-Length as an int: '{0}'", header.Value), this.baseRequest.Context, this.Context);
                    }

                    base.AddHeader(header.Key, header.Value);
                }

                if (newHeaders != null)
                {
                    List<string> values;
                    if (!newHeaders.TryGetValue(header.Key, out values))
                        newHeaders.Add(header.Key, values = new List<string>(1));

                    values.Add(header.Value);
                }
            }

            if (this.ExpectedContentLength == -1 && base.baseRequest.OnDownloadProgress != null)
                HTTPManager.Logger.Information("HTTP2Response", "AddHeaders - No Content-Length header found!", this.baseRequest.Context, this.Context);

            RequestEventHelper.EnqueueRequestEvent(new RequestEventInfo(this.baseRequest, newHeaders));
        }

        internal void AddData(Stream stream)
        {
            if (this.IsCompressed)
            {
                using (var decoderStream = new Decompression.Zlib.GZipStream(stream, Decompression.Zlib.CompressionMode.Decompress))
                {
                    using (var ms = new BufferPoolMemoryStream((int)stream.Length))
                    {
                        var buf = BufferPool.Get(8 * 1024, true);
                        int byteCount = 0;

                        while ((byteCount = decoderStream.Read(buf, 0, buf.Length)) > 0)
                            ms.Write(buf, 0, byteCount);

                        BufferPool.Release(buf);

                        base.Data = ms.ToArray();
                    }
                }
            }
            else
            {
                base.Data = BufferPool.Get(stream.Length, false);
                stream.Read(base.Data, 0, (int)stream.Length);
            }
        }

        bool isPrepared;
        private Decompression.GZipDecompressor decompressor;
        
        internal void ProcessData(byte[] payload, int payloadLength)
        {
            if (!this.isPrepared)
            {
                this.isPrepared = true;
                base.BeginReceiveStreamFragments();
            }

            if (this.IsCompressed)
            {
                if (this.decompressor == null)
                    this.decompressor = new Decompression.GZipDecompressor(0);
                var result = this.decompressor.Decompress(payload, 0, payloadLength, true, true);

                base.FeedStreamFragment(result.Data, 0, result.Length);
            }
            else
                base.FeedStreamFragment(payload, 0, payloadLength);
        }

        internal void FinishProcessData()
        {
            base.FlushRemainingFragmentBuffer();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (this.decompressor != null)
                {
                    this.decompressor.Dispose();
                    this.decompressor = null;
                }
            }
        }
    }
}

#endif
