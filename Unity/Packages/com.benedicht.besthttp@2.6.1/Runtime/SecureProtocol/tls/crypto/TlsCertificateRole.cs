#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls.Crypto
{
    public abstract class TlsCertificateRole
    {
        public const int DH = 1;
        public const int ECDH = 2;
        public const int RsaEncryption = 3;
        public const int Sm2Encryption = 4;
    }
}
#pragma warning restore
#endif
