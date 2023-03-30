#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Parameters;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Generators
{
	/**
	 * KDF2 generator for derived keys and ivs as defined by IEEE P1363a/ISO 18033
	 * <br/>
	 * This implementation is based on IEEE P1363/ISO 18033.
	 */
	public class Kdf2BytesGenerator
		: BaseKdfBytesGenerator
	{
		/**
		* Construct a KDF2 bytes generator. Generates key material
		* according to IEEE P1363 or ISO 18033 depending on the initialisation.
		*
		* @param digest the digest to be used as the source of derived keys.
		*/
		public Kdf2BytesGenerator(IDigest digest)
			: base(1, digest)
		{
		}
	}
}
#pragma warning restore
#endif
