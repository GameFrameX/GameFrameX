#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Parameters;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Signers;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls.Crypto.Impl.BC
{
    public class BcTlsEd25519Verifier
        : BcTlsVerifier
    {
        public BcTlsEd25519Verifier(BcTlsCrypto crypto, Ed25519PublicKeyParameters publicKey)
            : base(crypto, publicKey)
        {
        }

        public override bool VerifyRawSignature(DigitallySigned signature, byte[] hash)
        {
            throw new NotSupportedException();
        }

        public override TlsStreamVerifier GetStreamVerifier(DigitallySigned signature)
        {
            SignatureAndHashAlgorithm algorithm = signature.Algorithm;
            if (algorithm == null || SignatureScheme.From(algorithm) != SignatureScheme.ed25519)
                throw new InvalidOperationException("Invalid algorithm: " + algorithm);

            Ed25519Signer verifier = new Ed25519Signer();
            verifier.Init(false, m_publicKey);

            return new BcTlsStreamVerifier(verifier, signature.Signature);
        }
    }
}
#pragma warning restore
#endif
