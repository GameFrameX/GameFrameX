#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.CryptoPro;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Math;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Parameters
{
	public class Gost3410PrivateKeyParameters
		: Gost3410KeyParameters
	{
		private readonly BigInteger x;

		public Gost3410PrivateKeyParameters(
			BigInteger			x,
			Gost3410Parameters	parameters)
			: base(true, parameters)
		{
			if (x.SignValue < 1 || x.BitLength > 256 || x.CompareTo(Parameters.Q) >= 0)
				throw new ArgumentException("Invalid x for GOST3410 private key", "x");

			this.x = x;
		}

		public Gost3410PrivateKeyParameters(
			BigInteger			x,
			DerObjectIdentifier	publicKeyParamSet)
			: base(true, publicKeyParamSet)
		{
			if (x.SignValue < 1 || x.BitLength > 256 || x.CompareTo(Parameters.Q) >= 0)
				throw new ArgumentException("Invalid x for GOST3410 private key", "x");

			this.x = x;
		}

		public BigInteger X
		{
			get { return x; }
		}
	}
}
#pragma warning restore
#endif
