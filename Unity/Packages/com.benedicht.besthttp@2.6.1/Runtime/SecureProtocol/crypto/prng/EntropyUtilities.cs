#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Security;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Prng
{
    public abstract class EntropyUtilities
    {
        /**
         * Generate numBytes worth of entropy from the passed in entropy source.
         *
         * @param entropySource the entropy source to request the data from.
         * @param numBytes the number of bytes of entropy requested.
         * @return a byte array populated with the random data.
         */
        public static byte[] GenerateSeed(IEntropySource entropySource, int numBytes)
        {
            byte[] bytes = new byte[numBytes];
            int count = 0;
            while (count < numBytes)
            {
                byte[] entropy = entropySource.GetEntropy();
                int toCopy = System.Math.Min(bytes.Length, numBytes - count);
                Array.Copy(entropy, 0, bytes, count, toCopy);
                count += toCopy;
            }
            return bytes;
        }
    }
}
#pragma warning restore
#endif
