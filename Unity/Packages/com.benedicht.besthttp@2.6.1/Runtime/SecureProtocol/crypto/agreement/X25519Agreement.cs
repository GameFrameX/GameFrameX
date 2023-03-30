#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Parameters;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Agreement
{
    public sealed class X25519Agreement
        : IRawAgreement
    {
        private X25519PrivateKeyParameters privateKey;

        public void Init(ICipherParameters parameters)
        {
            this.privateKey = (X25519PrivateKeyParameters)parameters;
        }

        public int AgreementSize
        {
            get { return X25519PrivateKeyParameters.SecretSize; }
        }

        public void CalculateAgreement(ICipherParameters publicKey, byte[] buf, int off)
        {
            privateKey.GenerateSecret((X25519PublicKeyParameters)publicKey, buf, off);
        }
    }
}
#pragma warning restore
#endif
