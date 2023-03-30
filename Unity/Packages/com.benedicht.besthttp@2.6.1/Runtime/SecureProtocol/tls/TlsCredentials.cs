#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls
{
    /// <summary>Base interface for interfaces/classes carrying TLS credentials.</summary>
    public interface TlsCredentials
    {
        /// <summary>Return the certificate structure representing our identity.</summary>
        /// <returns>our certificate structure.</returns>
		Certificate Certificate { get; }
    }
}
#pragma warning restore
#endif
