#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Engines;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Parameters;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Signers;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls.Crypto.Impl.BC
{
    /// <summary>Operator supporting the verification of RSASSA-PSS signatures using the BC light-weight API.</summary>
    public class BcTlsRsaPssVerifier
        : BcTlsVerifier
    {
        private readonly int m_signatureScheme;

        public BcTlsRsaPssVerifier(BcTlsCrypto crypto, RsaKeyParameters publicKey, int signatureScheme)
            : base(crypto, publicKey)
        {
            if (!SignatureScheme.IsRsaPss(signatureScheme))
                throw new ArgumentException("signatureScheme");

            this.m_signatureScheme = signatureScheme;
        }

        public override bool VerifyRawSignature(DigitallySigned signature, byte[] hash)
        {
            throw new NotSupportedException();
        }

        public override TlsStreamVerifier GetStreamVerifier(DigitallySigned signature)
        {
            SignatureAndHashAlgorithm algorithm = signature.Algorithm;
            if (algorithm == null || SignatureScheme.From(algorithm) != m_signatureScheme)
                throw new InvalidOperationException("Invalid algorithm: " + algorithm);

            int cryptoHashAlgorithm = SignatureScheme.GetCryptoHashAlgorithm(m_signatureScheme);
            IDigest digest = m_crypto.CreateDigest(cryptoHashAlgorithm);

            PssSigner verifier = new PssSigner(new RsaEngine(), digest, digest.GetDigestSize());
            verifier.Init(false, m_publicKey);

            return new BcTlsStreamVerifier(verifier, signature.Signature);
        }
    }
}
#pragma warning restore
#endif
