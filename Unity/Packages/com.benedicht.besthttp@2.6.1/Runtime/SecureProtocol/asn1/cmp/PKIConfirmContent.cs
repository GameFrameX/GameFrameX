#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Cmp
{
	public class PkiConfirmContent
		: Asn1Encodable
	{
		public static PkiConfirmContent GetInstance(object obj)
		{
			if (obj is PkiConfirmContent)
				return (PkiConfirmContent)obj;

			if (obj is Asn1Null)
				return new PkiConfirmContent();

            throw new ArgumentException("Invalid object: " + BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.GetTypeName(obj), "obj");
		}

		public PkiConfirmContent()
		{
		}

		/**
		 * <pre>
		 * PkiConfirmContent ::= NULL
		 * </pre>
		 * @return a basic ASN.1 object representation.
		 */
		public override Asn1Object ToAsn1Object()
		{
			return DerNull.Instance;
		}
	}
}
#pragma warning restore
#endif
