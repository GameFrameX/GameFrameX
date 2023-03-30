#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Parameters
{
	/// <remarks>Parameters for mask derivation functions.</remarks>
    public class MgfParameters
		: IDerivationParameters
    {
        private readonly byte[] seed;

		public MgfParameters(
            byte[] seed)
			: this(seed, 0, seed.Length)
        {
        }

		public MgfParameters(
            byte[]  seed,
            int     off,
            int     len)
        {
            this.seed = new byte[len];
            Array.Copy(seed, off, this.seed, 0, len);
        }

		public byte[] GetSeed()
        {
            return (byte[]) seed.Clone();
        }
    }
}
#pragma warning restore
#endif
