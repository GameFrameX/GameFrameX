#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Bcpg
{
    public class UnsupportedPacketVersionException
        : Exception
    {
        public UnsupportedPacketVersionException(string msg)
            : base(msg)
        {
        }
    }
}
#pragma warning restore
#endif
