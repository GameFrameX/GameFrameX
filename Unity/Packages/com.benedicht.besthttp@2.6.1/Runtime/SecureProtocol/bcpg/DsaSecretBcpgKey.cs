#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Math;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Bcpg
{
	/// <remarks>Base class for a DSA secret key.</remarks>
	public class DsaSecretBcpgKey
		: BcpgObject, IBcpgKey
    {
		internal MPInteger x;

		/**
		* @param in
		*/
		public DsaSecretBcpgKey(
			BcpgInputStream bcpgIn)
		{
			this.x = new MPInteger(bcpgIn);
		}

		public DsaSecretBcpgKey(
			BigInteger x)
		{
			this.x = new MPInteger(x);
		}

		/// <summary>The format, as a string, always "PGP".</summary>
		public string Format
		{
			get { return "PGP"; }
		}

		/// <summary>Return the standard PGP encoding of the key.</summary>
		public override byte[] GetEncoded()
		{
			try
			{
				return base.GetEncoded();
			}
			catch (Exception)
			{
				return null;
			}
		}

		public override void Encode(
			BcpgOutputStream bcpgOut)
		{
			bcpgOut.WriteObject(x);
		}

		/**
		* @return x
		*/
		public BigInteger X
		{
			get { return x.Value; }
		}
	}
}
#pragma warning restore
#endif
