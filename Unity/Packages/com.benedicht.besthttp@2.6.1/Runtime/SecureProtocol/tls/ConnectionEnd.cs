#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls
{
    /// <summary>RFC 2246</summary>
    /// <remarks>
    /// Note that the values here are implementation-specific and arbitrary. It is recommended not to depend on the
    /// particular values(e.g.serialization).
    /// </remarks>
    public abstract class ConnectionEnd
    {
        public const int server = 0;
        public const int client = 1;
    }
}
#pragma warning restore
#endif
