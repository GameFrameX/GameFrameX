#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.IO;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Cms;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.X509;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Parameters;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Security;
using BestHTTP.SecureProtocol.Org.BouncyCastle.X509;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Cms
{
    public class KeyTransRecipientInfoGenerator : RecipientInfoGenerator
    {
        private static readonly CmsEnvelopedHelper Helper = CmsEnvelopedHelper.Instance;

        private Asn1OctetString subjectKeyIdentifier;
        private IKeyWrapper keyWrapper;

        // Derived fields
        private SubjectPublicKeyInfo info;
        private IssuerAndSerialNumber issuerAndSerialNumber;
        private SecureRandom random;
       

        public KeyTransRecipientInfoGenerator(X509Certificate recipCert, IKeyWrapper keyWrapper)
            : this(new Asn1.Cms.IssuerAndSerialNumber(recipCert.IssuerDN, new DerInteger(recipCert.SerialNumber)), keyWrapper)
        {
        }

        public KeyTransRecipientInfoGenerator(IssuerAndSerialNumber issuerAndSerial, IKeyWrapper keyWrapper)
        {
            this.issuerAndSerialNumber = issuerAndSerial;
            this.keyWrapper = keyWrapper;
        }

        public KeyTransRecipientInfoGenerator(byte[] subjectKeyID, IKeyWrapper keyWrapper)
        {
            this.subjectKeyIdentifier = new DerOctetString(subjectKeyIdentifier);
            this.keyWrapper = keyWrapper;
        }

        public RecipientInfo Generate(KeyParameter contentEncryptionKey, SecureRandom random)
        {
            AlgorithmIdentifier keyEncryptionAlgorithm = this.AlgorithmDetails;

            this.random = random;

            byte[] encryptedKeyBytes = GenerateWrappedKey(contentEncryptionKey);

            RecipientIdentifier recipId;
            if (issuerAndSerialNumber != null)
            {
                recipId = new RecipientIdentifier(issuerAndSerialNumber);
            }
            else
            {
                recipId = new RecipientIdentifier(subjectKeyIdentifier);
            }

            return new RecipientInfo(new KeyTransRecipientInfo(recipId, keyEncryptionAlgorithm,
                new DerOctetString(encryptedKeyBytes)));
        }

        protected virtual AlgorithmIdentifier AlgorithmDetails
        {
            get
            {
                if (this.keyWrapper != null)
                {
                    return (AlgorithmIdentifier)keyWrapper.AlgorithmDetails;
                }
                return info.AlgorithmID;
            }
        }

        protected virtual byte[] GenerateWrappedKey(KeyParameter contentEncryptionKey)
        {
            return keyWrapper.Wrap(contentEncryptionKey.GetKey()).Collect();
        }
    }
}
#pragma warning restore
#endif
