#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1
{
	/**
	 * A BER Null object.
	 */

	public class BerNull
		: DerNull
	{

        public static new readonly BerNull Instance = new BerNull();

		private BerNull()
            : base()
		{
		}
	}
}
#pragma warning restore
#endif
