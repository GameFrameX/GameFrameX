#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Pkcs
{
    /// <summary>
    /// Base exception for PKCS related issues.
    /// </summary>
    public class PkcsException
        : Exception
    {
        public PkcsException(string message)
            : base(message)
        {
        }

        public PkcsException(string message, Exception underlying)
            : base(message, underlying)
        {
        }
    }
}
#pragma warning restore
#endif
