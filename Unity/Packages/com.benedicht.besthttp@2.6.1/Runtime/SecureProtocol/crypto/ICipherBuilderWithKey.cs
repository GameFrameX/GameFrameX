#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto
{
    /// <summary>
    /// A cipher builder that can also return the key it was initialized with.
    /// </summary>
    public interface ICipherBuilderWithKey
        : ICipherBuilder
    {
        /// <summary>
        /// Return the key we were initialized with.
        /// </summary>
        ICipherParameters Key { get; }
    }
}
#pragma warning restore
#endif
