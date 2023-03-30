#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.IO;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Parameters;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Math.EC.Rfc8032;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Signers
{
    public class Ed448phSigner
        : ISigner
    {
        private readonly IXof prehash = Ed448.CreatePrehash();
        private readonly byte[] context;

        private bool forSigning;
        private Ed448PrivateKeyParameters privateKey;
        private Ed448PublicKeyParameters publicKey;

        public Ed448phSigner(byte[] context)
        {
            this.context = Arrays.Clone(context);
        }

        public virtual string AlgorithmName
        {
            get { return "Ed448ph"; }
        }

        public virtual void Init(bool forSigning, ICipherParameters parameters)
        {
            this.forSigning = forSigning;

            if (forSigning)
            {
                this.privateKey = (Ed448PrivateKeyParameters)parameters;
                this.publicKey = null;
            }
            else
            {
                this.privateKey = null;
                this.publicKey = (Ed448PublicKeyParameters)parameters;
            }

            Reset();
        }

        public virtual void Update(byte b)
        {
            prehash.Update(b);
        }

        public virtual void BlockUpdate(byte[] buf, int off, int len)
        {
            prehash.BlockUpdate(buf, off, len);
        }

        public virtual byte[] GenerateSignature()
        {
            if (!forSigning || null == privateKey)
                throw new InvalidOperationException("Ed448phSigner not initialised for signature generation.");

            byte[] msg = new byte[Ed448.PrehashSize];
            if (Ed448.PrehashSize != prehash.DoFinal(msg, 0, Ed448.PrehashSize))
                throw new InvalidOperationException("Prehash digest failed");

            byte[] signature = new byte[Ed448PrivateKeyParameters.SignatureSize];
            privateKey.Sign(Ed448.Algorithm.Ed448ph, context, msg, 0, Ed448.PrehashSize, signature, 0);
            return signature;
        }

        public virtual bool VerifySignature(byte[] signature)
        {
            if (forSigning || null == publicKey)
                throw new InvalidOperationException("Ed448phSigner not initialised for verification");
            if (Ed448.SignatureSize != signature.Length)
            {
                prehash.Reset();
                return false;
            }

            byte[] pk = publicKey.GetEncoded();
            return Ed448.VerifyPrehash(signature, 0, pk, 0, context, prehash);
        }

        public void Reset()
        {
            prehash.Reset();
        }
    }
}
#pragma warning restore
#endif
