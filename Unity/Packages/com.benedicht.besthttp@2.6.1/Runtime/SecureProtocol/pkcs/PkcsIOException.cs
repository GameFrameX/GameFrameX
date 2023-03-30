#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.IO;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Pkcs
{
    /// <summary>
    /// Base exception for parsing related issues in the PKCS namespace.
    /// </summary>
    public class PkcsIOException: IOException
    {
        public PkcsIOException(String message) : base(message)
        {
        }

        public PkcsIOException(String message, Exception underlying) : base(message, underlying)
        {
        }
    }
}
#pragma warning restore
#endif
