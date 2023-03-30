#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto
{
    /// <summary>
    /// Base interface for a provider to support the dynamic creation of signature verifiers.
    /// </summary>
    public interface IVerifierFactoryProvider
	{
        /// <summary>
        /// Return a signature verfier for signature algorithm described in the passed in algorithm details object.
        /// </summary>
        /// <param name="algorithmDetails">The details of the signature algorithm verification is required for.</param>
        /// <returns>A new signature verifier.</returns>
		IVerifierFactory CreateVerifierFactory (Object algorithmDetails);
	}
}

#pragma warning restore
#endif
