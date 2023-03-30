#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Agreement.Srp;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Math;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls.Crypto.Impl.BC
{
    internal sealed class BcTlsSrp6VerifierGenerator
        : TlsSrp6VerifierGenerator
    {
        private readonly Srp6VerifierGenerator m_srp6VerifierGenerator;

        internal BcTlsSrp6VerifierGenerator(Srp6VerifierGenerator srp6VerifierGenerator)
        {
            this.m_srp6VerifierGenerator = srp6VerifierGenerator;
        }

        public BigInteger GenerateVerifier(byte[] salt, byte[] identity, byte[] password)
        {
            return m_srp6VerifierGenerator.GenerateVerifier(salt, identity, password);
        }
    }
}
#pragma warning restore
#endif
