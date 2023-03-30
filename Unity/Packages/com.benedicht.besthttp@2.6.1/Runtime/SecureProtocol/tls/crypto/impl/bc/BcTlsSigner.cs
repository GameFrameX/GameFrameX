#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls.Crypto.Impl.BC
{
    public abstract class BcTlsSigner
        : TlsSigner
    {
        protected readonly BcTlsCrypto m_crypto;
        protected readonly AsymmetricKeyParameter m_privateKey;

        protected BcTlsSigner(BcTlsCrypto crypto, AsymmetricKeyParameter privateKey)
        {
            if (crypto == null)
                throw new ArgumentNullException("crypto");
            if (privateKey == null)
                throw new ArgumentNullException("privateKey");
            if (!privateKey.IsPrivate)
                throw new ArgumentException("must be private", "privateKey");

            this.m_crypto = crypto;
            this.m_privateKey = privateKey;
        }

        public abstract byte[] GenerateRawSignature(SignatureAndHashAlgorithm algorithm, byte[] hash);

        public virtual TlsStreamSigner GetStreamSigner(SignatureAndHashAlgorithm algorithm)
        {
            return null;
        }
    }
}
#pragma warning restore
#endif
