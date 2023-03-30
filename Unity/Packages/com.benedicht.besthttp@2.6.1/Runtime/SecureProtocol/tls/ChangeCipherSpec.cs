#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls
{
    public abstract class ChangeCipherSpec
    {
        public const short change_cipher_spec = 1;
    }
}
#pragma warning restore
#endif
