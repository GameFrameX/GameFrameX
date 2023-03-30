#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.IO;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.IO
{
    public class DigestSink
        : BaseOutputStream
    {
        private readonly IDigest mDigest;

        public DigestSink(IDigest digest)
        {
            this.mDigest = digest;
        }

        public virtual IDigest Digest
        {
            get { return mDigest; }
        }

        public override void WriteByte(byte b)
        {
            mDigest.Update(b);
        }

        public override void Write(byte[] buf, int off, int len)
        {
            if (len > 0)
            {
                mDigest.BlockUpdate(buf, off, len);
            }
        }
    }
}
#pragma warning restore
#endif
