#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Prng.Drbg
{
	/**
	 * Interface to SP800-90A deterministic random bit generators.
	 */
	public interface ISP80090Drbg
	{
	    /**
	     * Return the block size of the DRBG.
	     *
	     * @return the block size (in bits) produced by each round of the DRBG.
	     */
		int BlockSize { get; }

	    /**
	     * Populate a passed in array with random data.
	     *
	     * @param output output array for generated bits.
	     * @param additionalInput additional input to be added to the DRBG in this step.
	     * @param predictionResistant true if a reseed should be forced, false otherwise.
	     *
	     * @return number of bits generated, -1 if a reseed required.
	     */
	    int Generate(byte[] output, byte[] additionalInput, bool predictionResistant);

	    /**
	     * Reseed the DRBG.
	     *
	     * @param additionalInput additional input to be added to the DRBG in this step.
	     */
	    void Reseed(byte[] additionalInput);
	}
}
#pragma warning restore
#endif
