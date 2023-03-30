#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Parameters;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Tls.Crypto.Impl;

namespace BestHTTP.Connections.TLS.Crypto.Impl
{
    internal sealed class FastTlsBlockCipherImpl
        : TlsBlockCipherImpl
    {
        private readonly bool m_isEncrypting;
        private readonly IBlockCipher m_cipher;

        private NoCopyKeyParameter key;

        internal FastTlsBlockCipherImpl(IBlockCipher cipher, bool isEncrypting)
        {
            this.m_cipher = cipher;
            this.m_isEncrypting = isEncrypting;
        }

        public void SetKey(byte[] key, int keyOff, int keyLen)
        {
            this.key = new NoCopyKeyParameter(key, keyOff, keyLen);
        }

        public void Init(byte[] iv, int ivOff, int ivLen)
        {
            m_cipher.Init(m_isEncrypting, new FastParametersWithIV(key, iv, ivOff, ivLen));
        }

        public int DoFinal(byte[] input, int inputOffset, int inputLength, byte[] output, int outputOffset)
        {
            int blockSize = m_cipher.GetBlockSize();

            for (int i = 0; i < inputLength; i += blockSize)
            {
                m_cipher.ProcessBlock(input, inputOffset + i, output, outputOffset + i);
            }

            return inputLength;
        }

        public int GetBlockSize()
        {
            return m_cipher.GetBlockSize();
        }
    }
}
#pragma warning restore
#endif
