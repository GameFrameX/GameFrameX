#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Agreement.Srp;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Math;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls.Crypto.Impl.BC
{
    internal sealed class BcTlsSrp6Client
        : TlsSrp6Client
    {
        private readonly Srp6Client m_srp6Client;

        internal BcTlsSrp6Client(Srp6Client srpClient)
        {
            this.m_srp6Client = srpClient;
        }

        public BigInteger CalculateSecret(BigInteger serverB)
        {
            try
            {
                return m_srp6Client.CalculateSecret(serverB);
            }
            catch (CryptoException e)
            {
                throw new TlsFatalAlert(AlertDescription.illegal_parameter, e);
            }
        }

        public BigInteger GenerateClientCredentials(byte[] srpSalt, byte[] identity, byte[] password)
        {
            return m_srp6Client.GenerateClientCredentials(srpSalt, identity, password);
        }
    }
}
#pragma warning restore
#endif
