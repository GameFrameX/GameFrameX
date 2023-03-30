#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Ocsp
{
	public abstract class CertificateStatus
	{
		public static readonly CertificateStatus Good = null;
	}
}
#pragma warning restore
#endif
