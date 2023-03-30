#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls.Crypto
{
    /// <summary>Domain interface to service factory for creating Diffie-Hellman operators.</summary>
    public interface TlsDHDomain
    {
        /// <summary>Return an agreement operator suitable for ephemeral Diffie-Hellman.</summary>
        /// <returns>a key agreement operator.</returns>
        TlsAgreement CreateDH();
    }
}
#pragma warning restore
#endif
