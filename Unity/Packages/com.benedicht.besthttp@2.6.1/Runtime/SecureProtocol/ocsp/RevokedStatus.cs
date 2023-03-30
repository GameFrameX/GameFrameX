#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Ocsp;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.X509;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Ocsp
{
	/**
	 * wrapper for the RevokedInfo object
	 */
	public class RevokedStatus
		: CertificateStatus
	{
		internal readonly RevokedInfo info;

		public RevokedStatus(
			RevokedInfo info)
		{
			this.info = info;
		}

		public RevokedStatus(
			DateTime	revocationDate,
			int			reason)
		{
			this.info = new RevokedInfo(new DerGeneralizedTime(revocationDate), new CrlReason(reason));
		}

		public DateTime RevocationTime
		{
			get { return info.RevocationTime.ToDateTime(); }
		}

		public bool HasRevocationReason
		{
			get { return (info.RevocationReason != null); }
		}

		/**
		 * return the revocation reason. Note: this field is optional, test for it
		 * with hasRevocationReason() first.
		 * @exception InvalidOperationException if a reason is asked for and none is avaliable
		 */
		public int RevocationReason
		{
			get
			{
				if (info.RevocationReason == null)
				{
					throw new InvalidOperationException("attempt to get a reason where none is available");
				}

                return info.RevocationReason.IntValueExact;
			}
		}
	}
}
#pragma warning restore
#endif
