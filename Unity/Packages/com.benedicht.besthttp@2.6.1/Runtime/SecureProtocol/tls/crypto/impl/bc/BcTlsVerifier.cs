#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls.Crypto.Impl.BC
{
    public abstract class BcTlsVerifier
        : TlsVerifier
    {
        protected readonly BcTlsCrypto m_crypto;
        protected readonly AsymmetricKeyParameter m_publicKey;

        protected BcTlsVerifier(BcTlsCrypto crypto, AsymmetricKeyParameter publicKey)
        {
            if (crypto == null)
                throw new ArgumentNullException("crypto");
            if (publicKey == null)
                throw new ArgumentNullException("publicKey");
            if (publicKey.IsPrivate)
                throw new ArgumentException("must be public", "publicKey");

            this.m_crypto = crypto;
            this.m_publicKey = publicKey;
        }

        public virtual TlsStreamVerifier GetStreamVerifier(DigitallySigned signature)
        {
            return null;
        }

        public abstract bool VerifyRawSignature(DigitallySigned signature, byte[] hash);
    }
}
#pragma warning restore
#endif
