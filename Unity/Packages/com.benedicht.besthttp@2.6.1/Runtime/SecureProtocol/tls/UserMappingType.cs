#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls
{
    /// <summary>RFC 4681</summary>
    public abstract class UserMappingType
    {
        /*
         * RFC 4681
         */
        public const short upn_domain_hint = 64;
    }
}
#pragma warning restore
#endif
