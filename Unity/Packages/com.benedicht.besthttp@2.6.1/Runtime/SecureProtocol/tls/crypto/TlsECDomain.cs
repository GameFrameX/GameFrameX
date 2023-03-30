#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls.Crypto
{
    /// <summary>Domain interface to service factory for creating Elliptic-Curve (EC) based operators.</summary>
    public interface TlsECDomain
    {
        /// <summary>Return an agreement operator suitable for ephemeral EC Diffie-Hellman.</summary>
        /// <returns>a key agreement operator.</returns>
        TlsAgreement CreateECDH();
    }
}
#pragma warning restore
#endif
