#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls
{
    public class TlsFatalAlertReceived
        : TlsException
    {
        protected readonly short m_alertDescription;

        public TlsFatalAlertReceived(short alertDescription)
            : base(Tls.AlertDescription.GetText(alertDescription))
        {
            this.m_alertDescription = alertDescription;
        }

        public virtual short AlertDescription
        {
            get { return m_alertDescription; }
        }
    }
}
#pragma warning restore
#endif
