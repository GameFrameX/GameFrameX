#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls
{
    /// <summary>RFC 6091</summary>
    public abstract class CertificateType
    {
        public const short X509 = 0;
        public const short OpenPGP = 1;

        /*
         * RFC 7250
         */
        public const short RawPublicKey = 2;
    }
}
#pragma warning restore
#endif
