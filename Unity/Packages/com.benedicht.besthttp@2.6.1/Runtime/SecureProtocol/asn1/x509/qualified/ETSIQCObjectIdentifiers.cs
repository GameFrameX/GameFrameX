#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.X509.Qualified
{
	public abstract class EtsiQCObjectIdentifiers
	{
		//
		// base id
		//
		public static readonly DerObjectIdentifier IdEtsiQcs                 = new DerObjectIdentifier("0.4.0.1862.1");

		public static readonly DerObjectIdentifier IdEtsiQcsQcCompliance     = new DerObjectIdentifier(IdEtsiQcs+".1");
		public static readonly DerObjectIdentifier IdEtsiQcsLimitValue       = new DerObjectIdentifier(IdEtsiQcs+".2");
		public static readonly DerObjectIdentifier IdEtsiQcsRetentionPeriod  = new DerObjectIdentifier(IdEtsiQcs+".3");
		public static readonly DerObjectIdentifier IdEtsiQcsQcSscd           = new DerObjectIdentifier(IdEtsiQcs+".4");
	}
}
#pragma warning restore
#endif
