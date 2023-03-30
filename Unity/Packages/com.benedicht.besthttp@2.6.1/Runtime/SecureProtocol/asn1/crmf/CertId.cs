#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.X509;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Crmf
{
    public class CertId
        : Asn1Encodable
    {
        private readonly GeneralName issuer;
        private readonly DerInteger serialNumber;

        private CertId(Asn1Sequence seq)
        {
            issuer = GeneralName.GetInstance(seq[0]);
            serialNumber = DerInteger.GetInstance(seq[1]);
        }

        public static CertId GetInstance(object obj)
        {
            if (obj is CertId)
                return (CertId)obj;

            if (obj is Asn1Sequence)
                return new CertId((Asn1Sequence)obj);

            throw new ArgumentException("Invalid object: " + BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.GetTypeName(obj), "obj");
        }

        public static CertId GetInstance(Asn1TaggedObject obj, bool isExplicit)
        {
            return GetInstance(Asn1Sequence.GetInstance(obj, isExplicit));
        }

        public virtual GeneralName Issuer
        {
            get { return issuer; }
        }

        public virtual DerInteger SerialNumber
        {
            get { return serialNumber; }
        }

        /**
         * <pre>
         * CertId ::= SEQUENCE {
         *                 issuer           GeneralName,
         *                 serialNumber     INTEGER }
         * </pre>
         * @return a basic ASN.1 object representation.
         */
        public override Asn1Object ToAsn1Object()
        {
            return new DerSequence(issuer, serialNumber);
        }
    }
}
#pragma warning restore
#endif
