#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Tls.Crypto;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls
{
    /// <summary>Interface for verifying SRP config needs to conform to.</summary>
    public interface TlsSrpConfigVerifier
    {
        /// <summary>Check whether the given SRP configuration is acceptable for use.</summary>
        /// <param name="srpConfig">the <see cref="TlsSrpConfig"/> to check.</param>
        /// <returns>true if (and only if) the specified configuration is acceptable.</returns>
        bool Accept(TlsSrpConfig srpConfig);
    }
}
#pragma warning restore
#endif
