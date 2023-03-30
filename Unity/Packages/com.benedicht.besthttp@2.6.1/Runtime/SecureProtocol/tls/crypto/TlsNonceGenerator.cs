#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls.Crypto
{
    public interface TlsNonceGenerator
    {
        /// <summary>Generate a nonce byte[] string.</summary>
        /// <param name="size">the length, in bytes, of the nonce to generate.</param>
        /// <returns>the nonce value.</returns>
        byte[] GenerateNonce(int size);
    }
}
#pragma warning restore
#endif
