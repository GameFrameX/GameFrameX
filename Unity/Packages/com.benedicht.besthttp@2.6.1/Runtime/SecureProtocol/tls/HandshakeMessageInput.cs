#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.IO;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Tls.Crypto;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.IO;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls
{
    // TODO Rewrite without MemoryStream
    public sealed class HandshakeMessageInput
        : MemoryStream
    {
        private readonly int m_offset;

        internal HandshakeMessageInput(byte[] buf, int offset, int length)
#if PORTABLE || NETFX_CORE
            : base(buf, offset, length, false)
#else
            : base(buf, offset, length, false, true)
#endif
        {
#if PORTABLE || NETFX_CORE
            this.m_offset = 0;
#else
            this.m_offset = offset;
#endif
        }

        public void UpdateHash(TlsHash hash)
        {
            Streams.WriteBufTo(this, new TlsHashSink(hash));
        }

        internal void UpdateHashPrefix(TlsHash hash, int bindersSize)
        {
#if PORTABLE || NETFX_CORE
            byte[] buf = ToArray();
            int count = buf.Length;
#else
            byte[] buf = GetBuffer();
            int count = (int)Length;
#endif

            hash.Update(buf, m_offset, count - bindersSize);
        }

        internal void UpdateHashSuffix(TlsHash hash, int bindersSize)
        {
#if PORTABLE || NETFX_CORE
            byte[] buf = ToArray();
            int count = buf.Length;
#else
            byte[] buf = GetBuffer();
            int count = (int)Length;
#endif

            hash.Update(buf, m_offset + count - bindersSize, bindersSize);
        }
    }
}
#pragma warning restore
#endif
