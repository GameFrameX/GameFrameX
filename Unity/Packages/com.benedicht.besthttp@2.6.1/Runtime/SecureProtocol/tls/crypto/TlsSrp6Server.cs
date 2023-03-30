#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.IO;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Math;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls.Crypto
{
    /// <summary>Basic interface for an SRP-6 server implementation.</summary>
    public interface TlsSrp6Server
    {
        /// <summary>Generates the server's credentials that are to be sent to the client.</summary>
        /// <returns>The server's public value to the client</returns>
        BigInteger GenerateServerCredentials();

        /// <summary>Processes the client's credentials. If valid the shared secret is generated and returned.
        /// </summary>
        /// <param name="clientA">The client's credentials.</param>
        /// <returns>A shared secret <see cref="BigInteger"/>.</returns>
        /// <exception cref="IOException">If client's credentials are invalid.</exception>
        BigInteger CalculateSecret(BigInteger clientA);
    }
}
#pragma warning restore
#endif
