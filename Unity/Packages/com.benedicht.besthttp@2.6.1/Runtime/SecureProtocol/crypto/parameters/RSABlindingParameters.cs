#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Math;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Parameters
{
	public class RsaBlindingParameters
		: ICipherParameters
	{
		private readonly RsaKeyParameters	publicKey;
		private readonly BigInteger			blindingFactor;

		public RsaBlindingParameters(
			RsaKeyParameters	publicKey,
			BigInteger			blindingFactor)
		{
			if (publicKey.IsPrivate)
				throw new ArgumentException("RSA parameters should be for a public key");

			this.publicKey = publicKey;
			this.blindingFactor = blindingFactor;
		}

		public RsaKeyParameters PublicKey
		{
			get { return publicKey; }
		}

		public BigInteger BlindingFactor
		{
			get { return blindingFactor; }
		}
	}
}
#pragma warning restore
#endif
