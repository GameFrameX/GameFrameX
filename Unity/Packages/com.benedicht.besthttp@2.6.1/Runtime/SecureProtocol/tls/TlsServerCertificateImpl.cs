#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls
{
    internal sealed class TlsServerCertificateImpl
        : TlsServerCertificate
    {
        private readonly Certificate m_certificate;
        private readonly CertificateStatus m_certificateStatus;

        internal TlsServerCertificateImpl(Certificate certificate, CertificateStatus certificateStatus)
        {
            this.m_certificate = certificate;
            this.m_certificateStatus = certificateStatus;
        }

        public Certificate Certificate
        {
            get { return m_certificate; }
        }

        public CertificateStatus CertificateStatus
        {
            get { return m_certificateStatus; }
        }
    }
}
#pragma warning restore
#endif
