#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.IO;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Tls.Crypto;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.IO;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls
{
    internal class DigestInputBuffer
        : MemoryStream
    {
        internal void UpdateDigest(TlsHash hash)
        {
            Streams.WriteBufTo(this, new TlsHashSink(hash));
        }

        /// <exception cref="IOException"/>
        internal void CopyTo(Stream output)
        {
            // TODO[tls-port]
            // NOTE: Copy data since the output here may be under control of external code.
            //Streams.PipeAll(new MemoryStream(buf, 0, count), output);
            Streams.WriteBufTo(this, output);
        }
    }
}
#pragma warning restore
#endif
