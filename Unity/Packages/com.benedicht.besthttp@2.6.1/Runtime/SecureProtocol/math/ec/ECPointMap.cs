#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Math.EC
{
    public interface ECPointMap
    {
        ECPoint Map(ECPoint p);
    }
}
#pragma warning restore
#endif
