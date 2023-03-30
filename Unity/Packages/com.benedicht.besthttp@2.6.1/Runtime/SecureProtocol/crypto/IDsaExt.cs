#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Math;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto
{
    /// <summary>
    /// An "extended" interface for classes implementing DSA-style algorithms, that provides access
    /// to the group order.
    /// </summary>
    public interface IDsaExt
        : IDsa
    {
        /// <summary>The order of the group that the r, s values in signatures belong to.</summary>
        BigInteger Order { get; }
    }
}
#pragma warning restore
#endif
