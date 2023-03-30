#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls
{
    public abstract class RecordFormat
    {
        public const int TypeOffset = 0;
        public const int VersionOffset = 1;
        public const int LengthOffset = 3;
        public const int FragmentOffset = 5;
    }
}
#pragma warning restore
#endif
