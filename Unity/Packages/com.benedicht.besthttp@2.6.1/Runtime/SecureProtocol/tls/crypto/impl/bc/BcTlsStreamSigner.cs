#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.IO;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.IO;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls.Crypto.Impl.BC
{
    internal sealed class BcTlsStreamSigner
        : TlsStreamSigner
    {
        private readonly SignerSink m_output;

        internal BcTlsStreamSigner(ISigner signer)
        {
            this.m_output = new SignerSink(signer);
        }

        public Stream GetOutputStream()
        {
            return m_output;
        }

        public byte[] GetSignature()
        {
            try
            {
                return m_output.Signer.GenerateSignature();
            }
            catch (CryptoException e)
            {
                throw new TlsFatalAlert(AlertDescription.internal_error, e);
            }
        }
    }
}
#pragma warning restore
#endif
