#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Bcpg
{
	/// <remarks>Base interface for a PGP key.</remarks>
    public interface IBcpgKey
    {
		/// <summary>
		/// The base format for this key - in the case of the symmetric keys it will generally
		/// be raw indicating that the key is just a straight byte representation, for an asymmetric
		/// key the format will be PGP, indicating the key is a string of MPIs encoded in PGP format.
		/// </summary>
		/// <returns>"RAW" or "PGP".</returns>
        string Format { get; }
    }
}
#pragma warning restore
#endif
