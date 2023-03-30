#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.IO;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.IO
{
    public class MemoryOutputStream
        : MemoryStream
    {
        public sealed override bool CanRead
        {
            get { return false; }
        }
    }
}
#pragma warning restore
#endif
