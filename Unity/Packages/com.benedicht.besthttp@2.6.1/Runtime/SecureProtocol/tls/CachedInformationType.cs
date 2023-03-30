#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls
{
    public abstract class CachedInformationType
    {
        public const short cert = 1;
        public const short cert_req = 2;

        public static string GetName(short cachedInformationType)
        {
            switch (cachedInformationType)
            {
            case cert:
                return "cert";
            case cert_req:
                return "cert_req";
            default:
                return "UNKNOWN";
            }
        }

        public static string GetText(short cachedInformationType)
        {
            return GetName(cachedInformationType) + "(" + cachedInformationType + ")";
        }
    }
}
#pragma warning restore
#endif
