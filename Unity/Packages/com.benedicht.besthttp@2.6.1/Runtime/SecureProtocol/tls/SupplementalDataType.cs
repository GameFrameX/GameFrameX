#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls
{
    /// <summary>RFC 4680</summary>
    public abstract class SupplementalDataType
    {
        /*
         * RFC 4681
         */
        public const int user_mapping_data = 0;
    }
}
#pragma warning restore
#endif
