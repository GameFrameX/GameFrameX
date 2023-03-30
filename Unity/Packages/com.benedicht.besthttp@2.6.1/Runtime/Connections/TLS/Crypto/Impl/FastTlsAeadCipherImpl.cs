#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.PlatformSupport.Memory;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Modes;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Parameters;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Tls;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Tls.Crypto.Impl;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities;

namespace BestHTTP.Connections.TLS.Crypto.Impl
{
    public sealed class NoCopyKeyParameter
        : ICipherParameters
    {
        private readonly byte[] key;

        public NoCopyKeyParameter(byte[] key)
            :this(key, 0, key.Length)
        {
        }

        public NoCopyKeyParameter(
            byte[] key,
            int keyOff,
            int keyLen)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            if (keyOff < 0 || keyOff > key.Length)
                throw new ArgumentOutOfRangeException("keyOff");
            if (keyLen < 0 || keyLen > (key.Length - keyOff))
                throw new ArgumentOutOfRangeException("keyLen");

            this.key = new byte[keyLen];
            Array.Copy(key, keyOff, this.key, 0, keyLen);
        }

        public byte[] GetKey()
        {
            return key;// (byte[])key.Clone();
        }
    }

    public sealed class FastAeadParameters
        : ICipherParameters
    {
        private readonly byte[] associatedText;
        private readonly byte[] nonce;
        private readonly NoCopyKeyParameter key;
        private readonly int macSize;

        /**
         * Base constructor.
         *
         * @param key key to be used by underlying cipher
         * @param macSize macSize in bits
         * @param nonce nonce to be used
         */
        public FastAeadParameters(NoCopyKeyParameter key, int macSize, byte[] nonce)
           : this(key, macSize, nonce, null)
        {
        }

        /**
		 * Base constructor.
		 *
		 * @param key key to be used by underlying cipher
		 * @param macSize macSize in bits
		 * @param nonce nonce to be used
		 * @param associatedText associated text, if any
		 */
        public FastAeadParameters(
            NoCopyKeyParameter key,
            int macSize,
            byte[] nonce,
            byte[] associatedText)
        {
            this.key = key;
            this.nonce = nonce;
            this.macSize = macSize;
            this.associatedText = associatedText;
        }

        public NoCopyKeyParameter Key
        {
            get { return key; }
        }

        public int MacSize
        {
            get { return macSize; }
        }

        public byte[] GetAssociatedText()
        {
            return associatedText;
        }

        public byte[] GetNonce()
        {
            return nonce;
        }
    }

    public sealed class FastParametersWithIV
        : ICipherParameters
    {
        private readonly ICipherParameters parameters;
        private readonly byte[] iv;

        public FastParametersWithIV(ICipherParameters parameters,
            byte[] iv)
            : this(parameters, iv, 0, iv.Length)
        {
        }

        public FastParametersWithIV(ICipherParameters parameters,
            byte[] iv, int ivOff, int ivLen)
        {
            // NOTE: 'parameters' may be null to imply key re-use
            if (iv == null)
                throw new ArgumentNullException("iv");

            this.parameters = parameters;
            this.iv = Arrays.CopyOfRange(iv, ivOff, ivOff + ivLen);
        }

        public byte[] GetIV()
        {
            return iv; // (byte[])iv.Clone();
        }

        public ICipherParameters Parameters
        {
            get { return parameters; }
        }
    }

    internal sealed class FastTlsAeadCipherImpl
        : TlsAeadCipherImpl
    {
        private readonly bool m_isEncrypting;
        private readonly IAeadBlockCipher m_cipher;

        private NoCopyKeyParameter key;

        internal FastTlsAeadCipherImpl(IAeadBlockCipher cipher, bool isEncrypting)
        {
            this.m_cipher = cipher;
            this.m_isEncrypting = isEncrypting;
        }

        public void SetKey(byte[] key, int keyOff, int keyLen)
        {
            this.key = new NoCopyKeyParameter(key, keyOff, keyLen);
        }

        public void Init(byte[] nonce, int macSize, byte[] additionalData)
        {
            m_cipher.Init(m_isEncrypting, new FastAeadParameters(key, macSize * 8, nonce, additionalData));
        }

        public int GetOutputSize(int inputLength)
        {
            return m_cipher.GetOutputSize(inputLength);
        }

        public int DoFinal(byte[] input, int inputOffset, int inputLength, byte[] output, int outputOffset)
        {
            int len = m_cipher.ProcessBytes(input, inputOffset, inputLength, output, outputOffset);

            try
            {
                len += m_cipher.DoFinal(output, outputOffset + len);
            }
            catch (InvalidCipherTextException e)
            {
                throw new TlsFatalAlert(AlertDescription.bad_record_mac, e);
            }

            return len;
        }
    }
}
#pragma warning restore
#endif
