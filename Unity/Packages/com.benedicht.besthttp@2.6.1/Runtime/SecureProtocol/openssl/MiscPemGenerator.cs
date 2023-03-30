#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.Collections;
using System.IO;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.CryptoPro;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Pkcs;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.X509;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.X9;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Parameters;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Math;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Pkcs;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Security;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Security.Certificates;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Encoders;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.IO.Pem;
using BestHTTP.SecureProtocol.Org.BouncyCastle.X509;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.OpenSsl
{
    /**
    * PEM generator for the original set of PEM objects used in Open SSL.
    */
    public class MiscPemGenerator
        : PemObjectGenerator
    {
        private object obj;
        private string algorithm;
        private char[] password;
        private SecureRandom random;

        public MiscPemGenerator(object obj)
        {
            this.obj = obj;
        }

        public MiscPemGenerator(
            object			obj,
            string			algorithm,
            char[]			password,
            SecureRandom	random)
        {
            this.obj = obj;
            this.algorithm = algorithm;
            this.password = password;
            this.random = random;
        }

        private static PemObject CreatePemObject(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            if (obj is AsymmetricCipherKeyPair)
            {
                return CreatePemObject(((AsymmetricCipherKeyPair)obj).Private);
            }

            string type;
            byte[] encoding;

            if (obj is PemObject)
                return (PemObject)obj;

            if (obj is PemObjectGenerator)
                return ((PemObjectGenerator)obj).Generate();

            if (obj is X509Certificate)
            {
                // TODO Should we prefer "X509 CERTIFICATE" here?
                type = "CERTIFICATE";
                try
                {
                    encoding = ((X509Certificate)obj).GetEncoded();
                }
                catch (CertificateEncodingException e)
                {
                    throw new IOException("Cannot Encode object: " + e.ToString());
                }
            }
            else if (obj is X509Crl)
            {
                type = "X509 CRL";
                try
                {
                    encoding = ((X509Crl)obj).GetEncoded();
                }
                catch (CrlException e)
                {
                    throw new IOException("Cannot Encode object: " + e.ToString());
                }
            }
            else if (obj is AsymmetricKeyParameter)
            {
                AsymmetricKeyParameter akp = (AsymmetricKeyParameter) obj;
                if (akp.IsPrivate)
                {
                    encoding = EncodePrivateKey(akp, out type);
                }
                else
                {
                    type = "PUBLIC KEY";

                    encoding = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(akp).GetDerEncoded();
                }
            }
            else if (obj is IX509AttributeCertificate)
            {
                type = "ATTRIBUTE CERTIFICATE";
                encoding = ((X509V2AttributeCertificate)obj).GetEncoded();
            }
            else if (obj is Pkcs10CertificationRequest)
            {
                type = "CERTIFICATE REQUEST";
                encoding = ((Pkcs10CertificationRequest)obj).GetEncoded();
            }
            else if (obj is Asn1.Cms.ContentInfo)
            {
                type = "PKCS7";
                encoding = ((Asn1.Cms.ContentInfo)obj).GetEncoded();
            }
            else
            {
                throw new PemGenerationException("Object type not supported: " + BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.GetTypeName(obj));
            }

            return new PemObject(type, encoding);
        }

//		private string GetHexEncoded(byte[] bytes)
//		{
//			bytes = Hex.Encode(bytes);
//
//			char[] chars = new char[bytes.Length];
//
//			for (int i = 0; i != bytes.Length; i++)
//			{
//				chars[i] = (char)bytes[i];
//			}
//
//			return new string(chars);
//		}

        private static PemObject CreatePemObject(
            object			obj,
            string			algorithm,
            char[]			password,
            SecureRandom	random)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (algorithm == null)
                throw new ArgumentNullException("algorithm");
            if (password == null)
                throw new ArgumentNullException("password");
            if (random == null)
                throw new ArgumentNullException("random");

            if (obj is AsymmetricCipherKeyPair)
            {
                return CreatePemObject(((AsymmetricCipherKeyPair)obj).Private, algorithm, password, random);
            }

            string type = null;
            byte[] keyData = null;

            if (obj is AsymmetricKeyParameter)
            {
                AsymmetricKeyParameter akp = (AsymmetricKeyParameter) obj;
                if (akp.IsPrivate)
                {
                    keyData = EncodePrivateKey(akp, out type);
                }
            }

            if (type == null || keyData == null)
            {
                // TODO Support other types?
                throw new PemGenerationException("Object type not supported: " + BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.GetTypeName(obj));
            }


            string dekAlgName = BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.ToUpperInvariant(algorithm);

            // Note: For backward compatibility
            if (dekAlgName == "DESEDE")
            {
                dekAlgName = "DES-EDE3-CBC";
            }

            int ivLength = BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.StartsWith(dekAlgName, "AES-") ? 16 : 8;

            byte[] iv = new byte[ivLength];
            random.NextBytes(iv);

            byte[] encData = PemUtilities.Crypt(true, keyData, password, dekAlgName, iv);

            IList headers = BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.CreateArrayList(2);

            headers.Add(new PemHeader("Proc-Type", "4,ENCRYPTED"));
            headers.Add(new PemHeader("DEK-Info", dekAlgName + "," + Hex.ToHexString(iv)));

            return new PemObject(type, headers, encData);
        }

        private static byte[] EncodePrivateKey(
            AsymmetricKeyParameter	akp,
            out string				keyType)
        {
            PrivateKeyInfo info = PrivateKeyInfoFactory.CreatePrivateKeyInfo(akp);
            AlgorithmIdentifier algID = info.PrivateKeyAlgorithm;
            DerObjectIdentifier oid = algID.Algorithm;

            if (oid.Equals(X9ObjectIdentifiers.IdDsa))
            {
                keyType = "DSA PRIVATE KEY";

                DsaParameter p = DsaParameter.GetInstance(algID.Parameters);

                BigInteger x = ((DsaPrivateKeyParameters) akp).X;
                BigInteger y = p.G.ModPow(x, p.P);

                // TODO Create an ASN1 object somewhere for this?
                return new DerSequence(
                    new DerInteger(0),
                    new DerInteger(p.P),
                    new DerInteger(p.Q),
                    new DerInteger(p.G),
                    new DerInteger(y),
                    new DerInteger(x)).GetEncoded();
            }

            if (oid.Equals(PkcsObjectIdentifiers.RsaEncryption))
            {
                keyType = "RSA PRIVATE KEY";

                return info.ParsePrivateKey().GetEncoded();
            }
            else if (oid.Equals(CryptoProObjectIdentifiers.GostR3410x2001)
                || oid.Equals(X9ObjectIdentifiers.IdECPublicKey))
            {
                keyType = "EC PRIVATE KEY";

                return info.ParsePrivateKey().GetEncoded();
            }
            else
            {
                keyType = "PRIVATE KEY";

                return info.GetEncoded();
            }
        }

        public PemObject Generate()
        {
            try
            {
                if (algorithm != null)
                {
                    return CreatePemObject(obj, algorithm, password, random);
                }

                return CreatePemObject(obj);
            }
            catch (IOException e)
            {
                throw new PemGenerationException("encoding exception", e);
            }
        }
    }
}
#pragma warning restore
#endif
