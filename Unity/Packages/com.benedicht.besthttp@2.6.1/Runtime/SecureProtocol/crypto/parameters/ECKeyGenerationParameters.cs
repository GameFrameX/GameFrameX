#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.CryptoPro;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Security;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Parameters
{
    public class ECKeyGenerationParameters
		: KeyGenerationParameters
    {
        private readonly ECDomainParameters domainParams;
		private readonly DerObjectIdentifier publicKeyParamSet;

		public ECKeyGenerationParameters(
			ECDomainParameters	domainParameters,
			SecureRandom		random)
			: base(random, domainParameters.N.BitLength)
        {
            this.domainParams = domainParameters;
        }

		public ECKeyGenerationParameters(
			DerObjectIdentifier	publicKeyParamSet,
			SecureRandom		random)
			: this(ECKeyParameters.LookupParameters(publicKeyParamSet), random)
		{
			this.publicKeyParamSet = publicKeyParamSet;
		}

		public ECDomainParameters DomainParameters
        {
			get { return domainParams; }
        }

		public DerObjectIdentifier PublicKeyParamSet
		{
			get { return publicKeyParamSet; }
		}
    }
}
#pragma warning restore
#endif
