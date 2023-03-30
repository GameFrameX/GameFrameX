#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto
{
    /**
     * this exception is thrown whenever we find something we don't expect in a
     * message.
     */
#if !(NETCF_1_0 || NETCF_2_0 || SILVERLIGHT || PORTABLE || NETFX_CORE)
    [Serializable]
#endif
    public class InvalidCipherTextException
		: CryptoException
    {
		/**
		* base constructor.
		*/
        public InvalidCipherTextException()
        {
        }

		/**
         * create a InvalidCipherTextException with the given message.
         *
         * @param message the message to be carried with the exception.
         */
        public InvalidCipherTextException(
            string message)
			: base(message)
        {
        }

		public InvalidCipherTextException(
            string		message,
            Exception	exception)
			: base(message, exception)
        {
        }
    }
}
#pragma warning restore
#endif
