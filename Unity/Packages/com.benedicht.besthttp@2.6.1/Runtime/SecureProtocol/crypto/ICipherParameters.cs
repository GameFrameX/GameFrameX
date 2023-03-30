#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto
{
    /**
     * all parameter classes implement this.
     */
    public interface ICipherParameters
    {
    }
}
#pragma warning restore
#endif
