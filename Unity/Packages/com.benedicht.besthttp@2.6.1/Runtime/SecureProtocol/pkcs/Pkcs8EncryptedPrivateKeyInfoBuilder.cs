#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.IO;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Pkcs;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.X509;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.IO;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Pkcs
{
    public class Pkcs8EncryptedPrivateKeyInfoBuilder
    {
        private PrivateKeyInfo privateKeyInfo;

        public Pkcs8EncryptedPrivateKeyInfoBuilder(byte[] privateKeyInfo):  this(PrivateKeyInfo.GetInstance(privateKeyInfo))
        {
        }

        public Pkcs8EncryptedPrivateKeyInfoBuilder(PrivateKeyInfo privateKeyInfo)
        {
            this.privateKeyInfo = privateKeyInfo;
        }

        /// <summary>
        /// Create the encrypted private key info using the passed in encryptor.
        /// </summary>
        /// <param name="encryptor">The encryptor to use.</param>
        /// <returns>An encrypted private key info containing the original private key info.</returns>
        public Pkcs8EncryptedPrivateKeyInfo Build(
            ICipherBuilder encryptor)
        {
            try
            {
                MemoryStream bOut = new MemoryOutputStream();
                ICipher cOut = encryptor.BuildCipher(bOut);
                byte[] keyData = privateKeyInfo.GetEncoded();

                Stream str = cOut.Stream;
                str.Write(keyData, 0, keyData.Length);
                BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.Dispose(str);

                return new Pkcs8EncryptedPrivateKeyInfo(new EncryptedPrivateKeyInfo((AlgorithmIdentifier)encryptor.AlgorithmDetails, bOut.ToArray()));
            }
            catch (IOException)
            {
                throw new InvalidOperationException("cannot encode privateKeyInfo");
            }
        }
    }
}
#pragma warning restore
#endif
