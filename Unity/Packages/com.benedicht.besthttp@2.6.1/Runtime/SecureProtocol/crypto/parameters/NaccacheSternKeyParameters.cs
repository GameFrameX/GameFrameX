#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Math;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Parameters
{
	/**
	 * Public key parameters for NaccacheStern cipher. For details on this cipher,
	 * please see
	 *
	 * http://www.gemplus.com/smart/rd/publications/pdf/NS98pkcs.pdf
	 */
	public class NaccacheSternKeyParameters : AsymmetricKeyParameter
	{
		private readonly BigInteger g, n;
		private readonly int lowerSigmaBound;

		/**
		 * @param privateKey
		 */
		public NaccacheSternKeyParameters(bool privateKey, BigInteger g, BigInteger n, int lowerSigmaBound)
			: base(privateKey)
		{
			this.g = g;
			this.n = n;
			this.lowerSigmaBound = lowerSigmaBound;
		}

		/**
		 * @return Returns the g.
		 */
		public BigInteger G { get  { return g; } }

		/**
		 * @return Returns the lowerSigmaBound.
		 */
		public int LowerSigmaBound { get { return lowerSigmaBound; } }

		/**
		 * @return Returns the n.
		 */
		public BigInteger Modulus { get { return n; } }
	}
}
#pragma warning restore
#endif
