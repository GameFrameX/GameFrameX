#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.X509
{
	public class X509Attributes
	{
		public static readonly DerObjectIdentifier RoleSyntax = new DerObjectIdentifier("2.5.4.72");
	}
}
#pragma warning restore
#endif
