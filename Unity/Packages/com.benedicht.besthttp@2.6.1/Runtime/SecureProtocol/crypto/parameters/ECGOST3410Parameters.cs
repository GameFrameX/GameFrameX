#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Math;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Math.EC;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Parameters
{
    public class ECGost3410Parameters
        : ECNamedDomainParameters
    {
        private readonly DerObjectIdentifier _publicKeyParamSet;
        private readonly DerObjectIdentifier _digestParamSet;
        private readonly DerObjectIdentifier _encryptionParamSet;

        public DerObjectIdentifier PublicKeyParamSet
        {
            get { return _publicKeyParamSet; }
        }

        public DerObjectIdentifier DigestParamSet
        {
            get { return _digestParamSet; }
        }

        public DerObjectIdentifier EncryptionParamSet
        {
            get { return _encryptionParamSet; }
        }

        public ECGost3410Parameters(
            ECNamedDomainParameters dp,
            DerObjectIdentifier publicKeyParamSet,
            DerObjectIdentifier digestParamSet,
            DerObjectIdentifier encryptionParamSet)
            : base(dp.Name, dp.Curve, dp.G, dp.N, dp.H, dp.GetSeed())
        {
            this._publicKeyParamSet = publicKeyParamSet;
            this._digestParamSet = digestParamSet;
            this._encryptionParamSet = encryptionParamSet;
        }

        public ECGost3410Parameters(ECDomainParameters dp, DerObjectIdentifier publicKeyParamSet,
            DerObjectIdentifier digestParamSet,
            DerObjectIdentifier encryptionParamSet)
            : base(publicKeyParamSet, dp.Curve, dp.G, dp.N, dp.H, dp.GetSeed())
        {
            this._publicKeyParamSet = publicKeyParamSet;
            this._digestParamSet = digestParamSet;
            this._encryptionParamSet = encryptionParamSet;
        }
    }
}
#pragma warning restore
#endif
