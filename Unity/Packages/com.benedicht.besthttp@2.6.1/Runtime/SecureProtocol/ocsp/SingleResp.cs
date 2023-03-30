#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.Collections;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Ocsp;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.X509;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Date;
using BestHTTP.SecureProtocol.Org.BouncyCastle.X509;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Ocsp
{
	public class SingleResp
		: X509ExtensionBase
	{
		internal readonly SingleResponse resp;

		public SingleResp(
			SingleResponse resp)
		{
			this.resp = resp;
		}

		public CertificateID GetCertID()
		{
			return new CertificateID(resp.CertId);
		}

		/**
		 * Return the status object for the response - null indicates good.
		 *
		 * @return the status object for the response, null if it is good.
		 */
		public object GetCertStatus()
		{
			CertStatus s = resp.CertStatus;

			if (s.TagNo == 0)
			{
				return null;            // good
			}

			if (s.TagNo == 1)
			{
				return new RevokedStatus(RevokedInfo.GetInstance(s.Status));
			}

			return new UnknownStatus();
		}

		public DateTime ThisUpdate
		{
			get { return resp.ThisUpdate.ToDateTime(); }
		}

		/**
		* return the NextUpdate value - note: this is an optional field so may
		* be returned as null.
		*
		* @return nextUpdate, or null if not present.
		*/
		public DateTimeObject NextUpdate
		{
			get
			{
				return resp.NextUpdate == null
					?	null
					:	new DateTimeObject(resp.NextUpdate.ToDateTime());
			}
		}

		public X509Extensions SingleExtensions
		{
			get { return resp.SingleExtensions; }
		}

		protected override X509Extensions GetX509Extensions()
		{
			return SingleExtensions;
		}
	}
}
#pragma warning restore
#endif
