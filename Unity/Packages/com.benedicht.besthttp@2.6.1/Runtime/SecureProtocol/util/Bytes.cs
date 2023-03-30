#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities
{
    public abstract class Bytes
    {
        public const int NumBits = 8;
        public const int NumBytes = 1;
    }
}
#pragma warning restore
#endif
