#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Ocsp
{
	/**
	 * wrapper for the UnknownInfo object
	 */
	public class UnknownStatus
		: CertificateStatus
	{
		public UnknownStatus()
		{
		}
	}
}
#pragma warning restore
#endif
