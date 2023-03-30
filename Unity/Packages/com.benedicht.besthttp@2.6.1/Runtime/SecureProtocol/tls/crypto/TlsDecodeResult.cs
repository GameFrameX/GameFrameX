#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls.Crypto
{
    public sealed class TlsDecodeResult
    {
        public readonly byte[] buf;
        public readonly int off, len;
        public readonly short contentType;

        public readonly bool fromBufferPool;

        public TlsDecodeResult(byte[] buf, int off, int len, short contentType)
        {
            this.buf = buf;
            this.off = off;
            this.len = len;
            this.contentType = contentType;
            this.fromBufferPool = false;
        }

        public TlsDecodeResult(byte[] buf, int off, int len, short contentType, bool fromPool)
        {
            this.buf = buf;
            this.off = off;
            this.len = len;
            this.contentType = contentType;
            this.fromBufferPool = fromPool;
        }
    }
}
#pragma warning restore
#endif
