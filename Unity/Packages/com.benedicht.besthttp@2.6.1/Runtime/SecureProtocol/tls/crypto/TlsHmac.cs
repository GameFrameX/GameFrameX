#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls.Crypto
{
    /// <summary>Interface for MAC services based on HMAC.</summary>
    public interface TlsHmac
        : TlsMac
    {
        /// <summary>Return the internal block size for the message digest underlying this HMAC service.</summary>
        /// <returns>the internal block size for the digest (in bytes).</returns>
        int InternalBlockSize { get; }
    }
}
#pragma warning restore
#endif
