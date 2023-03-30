#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Digests;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Encodings;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Engines;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Parameters;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Signers;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls.Crypto.Impl.BC
{
    /// <summary>Operator supporting the verification of RSASSA-PKCS1-v1_5 signatures using the BC light-weight API.
    /// </summary>
    public class BcTlsRsaVerifier
        : BcTlsVerifier
    {
        public BcTlsRsaVerifier(BcTlsCrypto crypto, RsaKeyParameters publicKey)
            : base(crypto, publicKey)
        {
        }

        public override bool VerifyRawSignature(DigitallySigned signedParams, byte[] hash)
        {
            IDigest nullDigest = new NullDigest();

            SignatureAndHashAlgorithm algorithm = signedParams.Algorithm;
            ISigner signer;
            if (algorithm != null)
            {
                if (algorithm.Signature != SignatureAlgorithm.rsa)
                    throw new InvalidOperationException("Invalid algorithm: " + algorithm);

                /*
                 * RFC 5246 4.7. In RSA signing, the opaque vector contains the signature generated
                 * using the RSASSA-PKCS1-v1_5 signature scheme defined in [PKCS1].
                 */
                signer = new RsaDigestSigner(nullDigest, TlsUtilities.GetOidForHashAlgorithm(algorithm.Hash));
            }
            else
            {
                /*
                 * RFC 5246 4.7. Note that earlier versions of TLS used a different RSA signature scheme
                 * that did not include a DigestInfo encoding.
                 */
                signer = new GenericSigner(new Pkcs1Encoding(new RsaBlindedEngine()), nullDigest);
            }
            signer.Init(false, m_publicKey);
            signer.BlockUpdate(hash, 0, hash.Length);
            return signer.VerifySignature(signedParams.Signature);
        }
    }
}
#pragma warning restore
#endif
