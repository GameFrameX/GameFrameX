#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls
{
    /// <summary>Marker interface to distinguish a TLS server context.</summary>
    public interface TlsServerContext
        : TlsContext
    {
    }
}
#pragma warning restore
#endif
