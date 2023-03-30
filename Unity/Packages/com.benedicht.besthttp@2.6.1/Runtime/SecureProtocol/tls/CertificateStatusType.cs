#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls
{
    public abstract class CertificateStatusType
    {
        /*
         *  RFC 6066
         */
        public const short ocsp = 1;

        /*
         *  RFC 6961
         */
        public const short ocsp_multi = 2;
    }
}
#pragma warning restore
#endif
