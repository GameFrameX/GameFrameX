#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto
{
    /**
     * interface that a public/private key pair generator should conform to.
     */
    public interface IAsymmetricCipherKeyPairGenerator
    {
        /**
         * intialise the key pair generator.
         *
         * @param the parameters the key pair is to be initialised with.
         */
        void Init(KeyGenerationParameters parameters);

        /**
         * return an AsymmetricCipherKeyPair containing the Generated keys.
         *
         * @return an AsymmetricCipherKeyPair containing the Generated keys.
         */
        AsymmetricCipherKeyPair GenerateKeyPair();
    }
}
#pragma warning restore
#endif
