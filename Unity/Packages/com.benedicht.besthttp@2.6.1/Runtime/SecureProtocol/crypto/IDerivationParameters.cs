#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto
{
    /**
     * Parameters for key/byte stream derivation classes
     */
    public interface IDerivationParameters
    {
    }
}
#pragma warning restore
#endif
