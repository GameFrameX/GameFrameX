#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.X509;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Generators;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Parameters;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Utilities;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Agreement.Kdf
{
    /**
    * X9.63 based key derivation function for ECDH CMS.
    */
    public class ECDHKekGenerator
        : IDerivationFunction
    {
        private readonly IDerivationFunction kdf;

        private DerObjectIdentifier	algorithm;
        private int					keySize;
        private byte[]				z;

        public ECDHKekGenerator(IDigest digest)
        {
            this.kdf = new Kdf2BytesGenerator(digest);
        }

        public virtual void Init(IDerivationParameters param)
        {
            DHKdfParameters parameters = (DHKdfParameters)param;

            this.algorithm = parameters.Algorithm;
            this.keySize = parameters.KeySize;
            this.z = parameters.GetZ(); // TODO Clone?
        }

        public virtual IDigest Digest
        {
            get { return kdf.Digest; }
        }

        public virtual int GenerateBytes(byte[]	outBytes, int outOff, int len)
        {
            // TODO Create an ASN.1 class for this (RFC3278)
            // ECC-CMS-SharedInfo
            DerSequence s = new DerSequence(
                new AlgorithmIdentifier(algorithm, DerNull.Instance),
                new DerTaggedObject(true, 2, new DerOctetString(Pack.UInt32_To_BE((uint)keySize))));

            kdf.Init(new KdfParameters(z, s.GetDerEncoded()));

            return kdf.GenerateBytes(outBytes, outOff, len);
        }
    }
}
#pragma warning restore
#endif
