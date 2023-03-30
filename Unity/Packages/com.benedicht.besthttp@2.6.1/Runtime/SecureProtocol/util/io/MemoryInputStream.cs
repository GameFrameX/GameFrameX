#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.IO;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.IO
{
    public class MemoryInputStream
        : MemoryStream
    {
        public MemoryInputStream(byte[] buffer)
            : base(buffer, false)
        {
        }

        public sealed override bool CanWrite
        {
            get { return false; }
        }
    }
}
#pragma warning restore
#endif
