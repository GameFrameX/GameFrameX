#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Cmp;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.X509;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Math;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Cmp
{
    public class RevocationDetails
    {
        private readonly RevDetails revDetails;

        public RevocationDetails(RevDetails revDetails)
        {
            this.revDetails = revDetails;
        }

        public X509Name Subject
        {
            get { return revDetails.CertDetails.Subject; }
        }

        public X509Name Issuer
        {
            get { return revDetails.CertDetails.Issuer; }
        }

        public BigInteger SerialNumber
        {
            get { return revDetails.CertDetails.SerialNumber.Value; }
        }

        public RevDetails ToASN1Structure()
        {
            return revDetails;
        }
    }
}
#pragma warning restore
#endif
