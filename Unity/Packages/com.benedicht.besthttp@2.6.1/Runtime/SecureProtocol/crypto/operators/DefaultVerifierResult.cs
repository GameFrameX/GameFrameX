#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Operators
{
    public class DefaultVerifierResult
        : IVerifier
    {
        private readonly ISigner mSigner;

        public DefaultVerifierResult(ISigner signer)
        {
            this.mSigner = signer;
        }

        public bool IsVerified(byte[] signature)
        {
            return mSigner.VerifySignature(signature);
        }

        public bool IsVerified(byte[] sig, int sigOff, int sigLen)
        {
            byte[] signature = Arrays.CopyOfRange(sig, sigOff, sigOff + sigLen);

            return IsVerified(signature);
        }
    }
}
#pragma warning restore
#endif
