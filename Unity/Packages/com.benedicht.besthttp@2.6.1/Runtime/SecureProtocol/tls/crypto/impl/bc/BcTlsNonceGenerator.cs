#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Prng;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls.Crypto.Impl.BC
{
    internal sealed class BcTlsNonceGenerator
        : TlsNonceGenerator
    {
        private readonly IRandomGenerator m_randomGenerator;

        internal BcTlsNonceGenerator(IRandomGenerator randomGenerator)
        {
            this.m_randomGenerator = randomGenerator;
        }

        public byte[] GenerateNonce(int size)
        {
            byte[] nonce = new byte[size];
            m_randomGenerator.NextBytes(nonce);
            return nonce;
        }
    }
}
#pragma warning restore
#endif
