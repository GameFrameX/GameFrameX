#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Modes
{
	/// <summary>An IAeadCipher based on an IBlockCipher.</summary>
	public interface IAeadBlockCipher
        : IAeadCipher
	{
        /// <returns>The block size for this cipher, in bytes.</returns>
        int GetBlockSize();

        /// <summary>The block cipher underlying this algorithm.</summary>
		IBlockCipher GetUnderlyingCipher();
	}
}
#pragma warning restore
#endif
