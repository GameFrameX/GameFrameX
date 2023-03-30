#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Ocsp;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Ocsp
{
	/**
	 * base generator for an OCSP response - at the moment this only supports the
	 * generation of responses containing BasicOCSP responses.
	 */
	public class OCSPRespGenerator
	{
		public const int Successful			= 0;	// Response has valid confirmations
		public const int MalformedRequest	= 1;	// Illegal confirmation request
		public const int InternalError		= 2;	// Internal error in issuer
		public const int TryLater			= 3;	// Try again later
		// (4) is not used
		public const int SigRequired		= 5;	// Must sign the request
		public const int Unauthorized		= 6;	// Request unauthorized

		public OcspResp Generate(
			int     status,
			object  response)
		{
			if (response == null)
			{
				return new OcspResp(new OcspResponse(new OcspResponseStatus(status),null));
			}
			if (response is BasicOcspResp)
			{
				BasicOcspResp r = (BasicOcspResp)response;
				Asn1OctetString octs;

				try
				{
					octs = new DerOctetString(r.GetEncoded());
				}
				catch (Exception e)
				{
					throw new OcspException("can't encode object.", e);
				}

				ResponseBytes rb = new ResponseBytes(
					OcspObjectIdentifiers.PkixOcspBasic, octs);

				return new OcspResp(new OcspResponse(
					new OcspResponseStatus(status), rb));
			}

			throw new OcspException("unknown response object");
		}
	}
}
#pragma warning restore
#endif
