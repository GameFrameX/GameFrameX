#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.IO;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls
{
    public class TlsException
        : IOException
    {
        public TlsException()
            : base()
        {
        }

        public TlsException(string message)
            : base(message)
        {
        }

        public TlsException(string message, Exception cause)
            : base(message, cause)
        {
        }
    }
}
#pragma warning restore
#endif
