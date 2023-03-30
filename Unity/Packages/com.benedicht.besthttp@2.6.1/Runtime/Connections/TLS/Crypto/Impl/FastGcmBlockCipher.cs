#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.PlatformSupport.Memory;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Macs;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Modes;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Modes.Gcm;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Parameters;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Utilities;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities;

namespace BestHTTP.Connections.TLS.Crypto.Impl
{
    /// <summary>
    /// Implements the Galois/Counter mode (GCM) detailed in
    /// NIST Special Publication 800-38D.
    /// </summary>
    [BestHTTP.PlatformSupport.IL2CPP.Il2CppSetOption(BestHTTP.PlatformSupport.IL2CPP.Option.NullChecks, false)]
    [BestHTTP.PlatformSupport.IL2CPP.Il2CppSetOption(BestHTTP.PlatformSupport.IL2CPP.Option.ArrayBoundsChecks, false)]
    [BestHTTP.PlatformSupport.IL2CPP.Il2CppSetOption(BestHTTP.PlatformSupport.IL2CPP.Option.DivideByZeroChecks, false)]
    [BestHTTP.PlatformSupport.IL2CPP.Il2CppEagerStaticClassConstructionAttribute]
    public sealed class FastGcmBlockCipher
        : IAeadBlockCipher
    {
        private const int BlockSize = 16;
        byte[] ctrBlock = new byte[BlockSize];

        private readonly IBlockCipher cipher;
        private IGcmExponentiator exp;

        // These fields are set by Init and not modified by processing
        private bool forEncryption;
        private bool initialised;
        private int macSize;
        private byte[] lastKey;
        private byte[] nonce;
        private byte[] initialAssociatedText;
        private byte[] H;
        private byte[] J0;

        // These fields are modified during processing
        private int bufLength;
        private byte[] bufBlock;
        private byte[] macBlock;
        private byte[] S, S_at, S_atPre;
        private byte[] counter;
        private uint blocksRemaining;
        private int bufOff;
        private ulong totalLength;
        private byte[] atBlock;
        private int atBlockPos;
        private ulong atLength;
        private ulong atLengthPre;

        public FastGcmBlockCipher(
            IBlockCipher c)
            : this(c, null)
        {
        }

        public FastGcmBlockCipher(
            IBlockCipher c,
            IGcmMultiplier m)
        {
            if (c.GetBlockSize() != BlockSize)
                throw new ArgumentException("cipher required with a block size of " + BlockSize + ".");

            if (m != null)
                throw new NotImplementedException("IGcmMultiplier");
            this.cipher = c;
        }

        public string AlgorithmName
        {
            get { return cipher.AlgorithmName + "/GCM"; }
        }

        public IBlockCipher GetUnderlyingCipher()
        {
            return cipher;
        }

        public int GetBlockSize()
        {
            return BlockSize;
        }

        /// <remarks>
        /// MAC sizes from 32 bits to 128 bits (must be a multiple of 8) are supported. The default is 128 bits.
        /// Sizes less than 96 are not recommended, but are supported for specialized applications.
        /// </remarks>
        public void Init(
            bool forEncryption,
            ICipherParameters parameters)
        {
            this.forEncryption = forEncryption;
            //this.macBlock = null;
            if (this.macBlock != null)
                Array.Clear(this.macBlock, 0, this.macBlock.Length);
            this.initialised = true;

            NoCopyKeyParameter keyParam;
            byte[] newNonce = null;

            if (parameters is FastAeadParameters)
            {
                FastAeadParameters param = (FastAeadParameters)parameters;

                newNonce = param.GetNonce();
                initialAssociatedText = param.GetAssociatedText();

                int macSizeBits = param.MacSize;
                if (macSizeBits < 32 || macSizeBits > 128 || macSizeBits % 8 != 0)
                {
                    throw new ArgumentException("Invalid value for MAC size: " + macSizeBits);
                }

                macSize = macSizeBits / 8;
                keyParam = param.Key;
            }
            else if (parameters is FastParametersWithIV)
            {
                FastParametersWithIV param = (FastParametersWithIV)parameters;
            
                newNonce = param.GetIV();
                initialAssociatedText = null;
                macSize = 16;
                keyParam = (NoCopyKeyParameter)param.Parameters;
            }
            else
            {
                throw new ArgumentException("invalid parameters passed to GCM");
            }

            this.bufLength = forEncryption ? BlockSize : (BlockSize + macSize);
            if (this.bufBlock == null || this.bufLength < this.bufBlock.Length)
                BufferPool.Resize(ref this.bufBlock, this.bufLength, true, true);

            if (newNonce == null || newNonce.Length < 1)
            {
                throw new ArgumentException("IV must be at least 1 byte");
            }

            if (forEncryption)
            {
                if (nonce != null && Arrays.AreEqual(nonce, newNonce))
                {
                    if (keyParam == null)
                    {
                        throw new ArgumentException("cannot reuse nonce for GCM encryption");
                    }
                    if (lastKey != null && Arrays.AreEqual(lastKey, keyParam.GetKey()))
                    {
                        throw new ArgumentException("cannot reuse nonce for GCM encryption");
                    }
                }
            }

            nonce = newNonce;
            if (keyParam != null)
            {
                lastKey = keyParam.GetKey();
            }

            // TODO Restrict macSize to 16 if nonce length not 12?

            // Cipher always used in forward mode
            // if keyParam is null we're reusing the last key.
            if (keyParam != null)
            {
                cipher.Init(true, keyParam);

                if (this.H == null)
                    this.H = new byte[BlockSize];
                else
                    Array.Clear(this.H, 0, BlockSize);
                cipher.ProcessBlock(H, 0, H, 0);

                // if keyParam is null we're reusing the last key and the multiplier doesn't need re-init
                Tables8kGcmMultiplier_Init(H);
                exp = null;
            }
            else if (this.H == null)
            {
                throw new ArgumentException("Key must be specified in initial init");
            }

            if (this.J0 == null)
                this.J0 = new byte[BlockSize];
            else
                Array.Clear(this.J0, 0, BlockSize);

            if (nonce.Length == 12)
            {
                Array.Copy(nonce, 0, J0, 0, nonce.Length);
                this.J0[BlockSize - 1] = 0x01;
            }
            else
            {
                gHASH(J0, nonce, nonce.Length);
                byte[] X = BufferPool.Get(BlockSize, false);
                Pack.UInt64_To_BE((ulong)nonce.Length * 8UL, X, 8);
                gHASHBlock(J0, X);
                BufferPool.Release(X);
            }

            //BufferPool.Resize(ref this.S, BlockSize, false, true);
            //BufferPool.Resize(ref this.S_at, BlockSize, false, true);
            //BufferPool.Resize(ref this.S_atPre, BlockSize, false, true);
            //BufferPool.Resize(ref this.atBlock, BlockSize, false, true);
            if (this.S == null)
                this.S = new byte[BlockSize];
            else
                Array.Clear(this.S, 0, this.S.Length);

            if (this.S_at == null)
                this.S_at = new byte[BlockSize];
            else
                Array.Clear(this.S_at, 0, this.S_at.Length);

            if (this.S_atPre == null)
                this.S_atPre = new byte[BlockSize];
            else
                Array.Clear(this.S_atPre, 0, this.S_atPre.Length);

            if (this.atBlock == null)
                this.atBlock = new byte[BlockSize];
            else
                Array.Clear(this.atBlock, 0, this.atBlock.Length);

            this.atBlockPos = 0;
            this.atLength = 0;
            this.atLengthPre = 0;

            //this.counter = Arrays.Clone(J0);
            //BufferPool.Resize(ref this.counter, BlockSize, false, true);
            if (this.counter == null)
                this.counter = new byte[BlockSize];
            else
                Array.Clear(this.counter, 0, this.counter.Length);

            Array.Copy(this.J0, 0, this.counter, 0, BlockSize);

            this.blocksRemaining = uint.MaxValue - 1; // page 8, len(P) <= 2^39 - 256, 1 block used by tag
            this.bufOff = 0;
            this.totalLength = 0;

            if (initialAssociatedText != null)
            {
                ProcessAadBytes(initialAssociatedText, 0, initialAssociatedText.Length);
            }
        }

        public byte[] GetMac()
        {
            return macBlock == null
                ? new byte[macSize]
                : Arrays.Clone(macBlock);
        }

        public int GetOutputSize(
            int len)
        {
            int totalData = len + bufOff;

            if (forEncryption)
            {
                return totalData + macSize;
            }

            return totalData < macSize ? 0 : totalData - macSize;
        }

        public int GetUpdateOutputSize(
            int len)
        {
            int totalData = len + bufOff;
            if (!forEncryption)
            {
                if (totalData < macSize)
                {
                    return 0;
                }
                totalData -= macSize;
            }
            return totalData - totalData % BlockSize;
        }

        public void ProcessAadByte(byte input)
        {
            CheckStatus();

            atBlock[atBlockPos] = input;
            if (++atBlockPos == BlockSize)
            {
                // Hash each block as it fills
                gHASHBlock(S_at, atBlock);
                atBlockPos = 0;
                atLength += BlockSize;
            }
        }

        public void ProcessAadBytes(byte[] inBytes, int inOff, int len)
        {
            CheckStatus();

            for (int i = 0; i < len; ++i)
            {
                atBlock[atBlockPos] = inBytes[inOff + i];
                if (++atBlockPos == BlockSize)
                {
                    // Hash each block as it fills
                    gHASHBlock(S_at, atBlock);
                    atBlockPos = 0;
                    atLength += BlockSize;
                }
            }
        }

        private void InitCipher()
        {
            if (atLength > 0)
            {
                Array.Copy(S_at, 0, S_atPre, 0, BlockSize);
                atLengthPre = atLength;
            }

            // Finish hash for partial AAD block
            if (atBlockPos > 0)
            {
                gHASHPartial(S_atPre, atBlock, 0, atBlockPos);
                atLengthPre += (uint)atBlockPos;
            }

            if (atLengthPre > 0)
            {
                Array.Copy(S_atPre, 0, S, 0, BlockSize);
            }
        }

        public int ProcessByte(
            byte input,
            byte[] output,
            int outOff)
        {
            CheckStatus();

            bufBlock[bufOff] = input;
            if (++bufOff == bufLength)
            {
                ProcessBlock(bufBlock, 0, output, outOff);
                if (forEncryption)
                {
                    bufOff = 0;
                }
                else
                {
                    Array.Copy(bufBlock, BlockSize, bufBlock, 0, macSize);
                    bufOff = macSize;
                }
                return BlockSize;
            }
            return 0;
        }

        public unsafe int ProcessBytes(
            byte[] input,
            int inOff,
            int len,
            byte[] output,
            int outOff)
        {
            CheckStatus();

            Check.DataLength(input, inOff, len, "input buffer too short");

            int resultLen = 0;

            if (forEncryption)
            {
                if (bufOff != 0)
                {
                    while (len > 0)
                    {
                        --len;
                        bufBlock[bufOff] = input[inOff++];
                        if (++bufOff == BlockSize)
                        {
                            ProcessBlock(bufBlock, 0, output, outOff);
                            bufOff = 0;
                            resultLen += BlockSize;
                            break;
                        }
                    }
                }

                fixed (byte* pctrBlock = ctrBlock, pbuf = input, pS = S, poutput = output)
                {
                    while (len >= BlockSize)
                    {
                        // ProcessBlock(byte[] buf, int bufOff, byte[] output, int outOff)
                        #region ProcessBlock(buf: input, bufOff: inOff, output: output, outOff: outOff + resultLen);

                        if (totalLength == 0)
                            InitCipher();

                        #region GetNextCtrBlock(ctrBlock);
                        blocksRemaining--;

                        uint c = 1;
                        c += counter[15]; counter[15] = (byte)c; c >>= 8;
                        c += counter[14]; counter[14] = (byte)c; c >>= 8;
                        c += counter[13]; counter[13] = (byte)c; c >>= 8;
                        c += counter[12]; counter[12] = (byte)c;

                        cipher.ProcessBlock(counter, 0, ctrBlock, 0);
                        #endregion

                        ulong* pulongBuf = (ulong*)&pbuf[inOff];
                        ulong* pulongctrBlock = (ulong*)pctrBlock;
                        pulongctrBlock[0] ^= pulongBuf[0];
                        pulongctrBlock[1] ^= pulongBuf[1];

                        ulong* pulongS = (ulong*)pS;
                        pulongS[0] ^= pulongctrBlock[0];
                        pulongS[1] ^= pulongctrBlock[1];

                        Tables8kGcmMultiplier_MultiplyH(S);

                        ulong* pulongoutput = (ulong*)&poutput[outOff + resultLen];
                        pulongoutput[0] = pulongctrBlock[0];
                        pulongoutput[1] = pulongctrBlock[1];

                        totalLength += BlockSize;

                        #endregion

                        inOff += BlockSize;
                        len -= BlockSize;
                        resultLen += BlockSize;
                    }
                }

                if (len > 0)
                {
                    Array.Copy(input, inOff, bufBlock, 0, len);
                    bufOff = len;
                }
            }
            else
            {
                fixed (byte* pinput = input, pbufBlock = bufBlock, pctrBlock = ctrBlock, pS = S, poutput = output)
                {
                    ulong* pulongbufBlock = (ulong*)pbufBlock;

                    // adjust bufOff to be on a 8 byte boundary
                    int adjustCount = 0;
                    for (int i = 0; i < len && (bufOff % 8) != 0; ++i)
                    {
                        pbufBlock[bufOff++] = pinput[inOff++ + i];
                        adjustCount++;

                        if (bufOff == bufLength)
                        {
                            ProcessBlock(bufBlock, 0, output, outOff + resultLen);

                            pulongbufBlock[0] = pulongbufBlock[2];
                            pulongbufBlock[1] = pulongbufBlock[3];

                            bufOff = macSize;
                            resultLen += BlockSize;
                        }
                    }

                    int longLen = (len - adjustCount) / 8;
                    if (longLen > 0)
                    {
                        ulong* pulonginput = (ulong*)&pinput[inOff];

                        int bufLongOff = bufOff / 8;

                        // copy 8 bytes per cycle instead of just 1
                        for (int i = 0; i < longLen; ++i)
                        {
                            pulongbufBlock[bufLongOff++] = pulonginput[i];
                            bufOff += 8;

                            if (bufOff == bufLength)
                            {
                                #region ProcessBlock(buf: bufBlock, bufOff: 0, output: output, outOff: outOff + resultLen);
                                if (totalLength == 0)
                                    InitCipher();

                                #region GetNextCtrBlock(ctrBlock);
                                blocksRemaining--;

                                uint c = 1;
                                c += counter[15]; counter[15] = (byte)c; c >>= 8;
                                c += counter[14]; counter[14] = (byte)c; c >>= 8;
                                c += counter[13]; counter[13] = (byte)c; c >>= 8;
                                c += counter[12]; counter[12] = (byte)c;

                                cipher.ProcessBlock(counter, 0, ctrBlock, 0);
                                #endregion

                                ulong* pulongS = (ulong*)pS;

                                pulongS[0] ^= pulongbufBlock[0];
                                pulongS[1] ^= pulongbufBlock[1];

                                Tables8kGcmMultiplier_MultiplyH(S);

                                ulong* pulongOutput = (ulong*)&poutput[outOff + resultLen];
                                ulong* pulongctrBlock = (ulong*)pctrBlock;

                                pulongOutput[0] = pulongctrBlock[0] ^ pulongbufBlock[0];
                                pulongOutput[1] = pulongctrBlock[1] ^ pulongbufBlock[1];

                                totalLength += BlockSize;

                                #endregion

                                pulongbufBlock[0] = pulongbufBlock[2];
                                pulongbufBlock[1] = pulongbufBlock[3];

                                bufOff = macSize;
                                resultLen += BlockSize;

                                bufLongOff = bufOff / 8;
                            }
                        }
                    }

                    for (int i = longLen * 8; i < len; i++)
                    {
                        pbufBlock[bufOff++] = pinput[inOff + i];

                        if (bufOff == bufLength)
                        {
                            ProcessBlock(bufBlock, 0, output, outOff + resultLen);

                            pulongbufBlock[0] = pulongbufBlock[2];
                            pulongbufBlock[1] = pulongbufBlock[3];

                            bufOff = macSize;
                            resultLen += BlockSize;
                        }
                    }
                }
            }

            return resultLen;
        }

        private unsafe void ProcessBlock(byte[] buf, int bufOff, byte[] output, int outOff)
        {
            if (totalLength == 0)
                InitCipher();

            GetNextCtrBlock(ctrBlock);

            if (forEncryption)
            {
                fixed (byte* pctrBlock = ctrBlock, pbuf = buf, pS = S)
                {

                    ulong* pulongBuf = (ulong*)&pbuf[bufOff];
                    ulong* pulongctrBlock = (ulong*)pctrBlock;
                    pulongctrBlock[0] ^= pulongBuf[0];
                    pulongctrBlock[1] ^= pulongBuf[1];

                    ulong* pulongS = (ulong*)pS;
                    pulongS[0] ^= pulongctrBlock[0];
                    pulongS[1] ^= pulongctrBlock[1];

                    Tables8kGcmMultiplier_MultiplyH(S);

                    fixed (byte* poutput = output)
                    {
                        ulong* pulongoutput = (ulong*)&poutput[outOff];
                        pulongoutput[0] = pulongctrBlock[0];
                        pulongoutput[1] = pulongctrBlock[1];
                    }
                }
            }
            else
            {
                // moved this part to ProcessBytes's main part 
                fixed (byte* pctrBlock = ctrBlock, pbuf = buf, pS = S, poutput = output)
                {
                    ulong* pulongS = (ulong*)pS;
                    ulong* pulongBuf = (ulong*)&pbuf[bufOff];

                    pulongS[0] ^= pulongBuf[0];
                    pulongS[1] ^= pulongBuf[1];

                    Tables8kGcmMultiplier_MultiplyH(S);

                    ulong* pulongOutput = (ulong*)&poutput[outOff];
                    ulong* pulongctrBlock = (ulong*)pctrBlock;

                    pulongOutput[0] = pulongctrBlock[0] ^ pulongBuf[0];
                    pulongOutput[1] = pulongctrBlock[1] ^ pulongBuf[1];
                }
            }

            totalLength += BlockSize;
        }

        public int DoFinal(byte[] output, int outOff)
        {
            CheckStatus();

            if (totalLength == 0)
            {
                InitCipher();
            }

            int extra = bufOff;

            if (forEncryption)
            {
                Check.OutputLength(output, outOff, extra + macSize, "Output buffer too short");
            }
            else
            {
                if (extra < macSize)
                    throw new InvalidCipherTextException("data too short");

                extra -= macSize;

                Check.OutputLength(output, outOff, extra, "Output buffer too short");
            }

            if (extra > 0)
            {
                ProcessPartial(bufBlock, 0, extra, output, outOff);
            }

            atLength += (uint)atBlockPos;

            if (atLength > atLengthPre)
            {
                /*
                 *  Some AAD was sent after the cipher started. We determine the difference b/w the hash value
                 *  we actually used when the cipher started (S_atPre) and the final hash value calculated (S_at).
                 *  Then we carry this difference forward by multiplying by H^c, where c is the number of (full or
                 *  partial) cipher-text blocks produced, and adjust the current hash.
                 */

                // Finish hash for partial AAD block
                if (atBlockPos > 0)
                {
                    gHASHPartial(S_at, atBlock, 0, atBlockPos);
                }

                // Find the difference between the AAD hashes
                if (atLengthPre > 0)
                {
                    GcmUtilities.Xor(S_at, S_atPre);
                }

                // Number of cipher-text blocks produced
                long c = (long)(((totalLength * 8) + 127) >> 7);

                // Calculate the adjustment factor
                byte[] H_c = BufferPool.Get(16, true);
                if (exp == null)
                {
                    exp = new Tables1kGcmExponentiator();
                    exp.Init(H);
                }
                exp.ExponentiateX(c, H_c);

                // Carry the difference forward
                GcmUtilities.Multiply(S_at, H_c);

                // Adjust the current hash
                GcmUtilities.Xor(S, S_at);

                BufferPool.Release(H_c);
            }

            // Final gHASH
            byte[] X = BufferPool.Get(BlockSize, false);
            Pack.UInt64_To_BE(atLength * 8UL, X, 0);
            Pack.UInt64_To_BE(totalLength * 8UL, X, 8);

            gHASHBlock(S, X);

            BufferPool.Release(X);

            // T = MSBt(GCTRk(J0,S))
            byte[] tag = BufferPool.Get(BlockSize, false);
            cipher.ProcessBlock(J0, 0, tag, 0);
            GcmUtilities.Xor(tag, S);

            int resultLen = extra;

            // We place into macBlock our calculated value for T

            if (this.macBlock == null || this.macBlock.Length < macSize)
                this.macBlock = BufferPool.Resize(ref this.macBlock, macSize, false, false);

            Array.Copy(tag, 0, macBlock, 0, macSize);

            BufferPool.Release(tag);

            if (forEncryption)
            {
                // Append T to the message
                Array.Copy(macBlock, 0, output, outOff + bufOff, macSize);
                resultLen += macSize;
            }
            else
            {
                // Retrieve the T value from the message and compare to calculated one
                byte[] msgMac = BufferPool.Get(macSize, false);
                Array.Copy(bufBlock, extra, msgMac, 0, macSize);
                if (!Arrays.ConstantTimeAreEqual(this.macBlock, msgMac))
                    throw new InvalidCipherTextException("mac check in GCM failed");
                BufferPool.Release(msgMac);
            }

            Reset(false);

            return resultLen;
        }

        public void Reset()
        {
            Reset(true);
        }

        private unsafe void Reset(
            bool clearMac)
        {
            cipher.Reset();

            // note: we do not reset the nonce.

            //BufferPool.Resize(ref this.S, BlockSize, false, true);
            //BufferPool.Resize(ref this.S_at, BlockSize, false, true);
            //BufferPool.Resize(ref this.S_atPre, BlockSize, false, true);
            //BufferPool.Resize(ref this.atBlock, BlockSize, false, true);
            fixed (byte* pS = S, pS_at = S_at, pS_atPre = S_atPre, patBlock = atBlock)
            {
                for (int i = 0; i < BlockSize; ++i)
                {
                    pS[i] = pS_at[i] = pS_atPre[i] = patBlock[i] = 0;
                }
            }

            atBlockPos = 0;
            atLength = 0;
            atLengthPre = 0;

            //BufferPool.Resize(ref this.counter, BlockSize, false, false);
            Array.Copy(this.J0, 0, this.counter, 0, BlockSize);

            blocksRemaining = uint.MaxValue - 1;
            bufOff = 0;
            totalLength = 0;

            if (bufBlock != null)
            {
                //Arrays.Fill(bufBlock, 0);
            }

            if (clearMac)
            {
                //macBlock = null;
                Array.Clear(this.macBlock, 0, this.macSize);
            }

            if (forEncryption)
            {
                initialised = false;
            }
            else
            {
                if (initialAssociatedText != null)
                {
                    ProcessAadBytes(initialAssociatedText, 0, initialAssociatedText.Length);
                }
            }
        }

        private void ProcessPartial(byte[] buf, int off, int len, byte[] output, int outOff)
        {
            //byte[] ctrBlock = new byte[BlockSize];
            GetNextCtrBlock(ctrBlock);

            if (forEncryption)
            {
                GcmUtilities.Xor(buf, off, ctrBlock, 0, len);
                gHASHPartial(S, buf, off, len);
            }
            else
            {
                gHASHPartial(S, buf, off, len);
                GcmUtilities.Xor(buf, off, ctrBlock, 0, len);
            }

            Array.Copy(buf, off, output, outOff, len);
            totalLength += (uint)len;
        }

        private void gHASH(byte[] Y, byte[] b, int len)
        {
            for (int pos = 0; pos < len; pos += BlockSize)
            {
                int num = System.Math.Min(len - pos, BlockSize);
                gHASHPartial(Y, b, pos, num);
            }
        }

        private void gHASHBlock(byte[] Y, byte[] b)
        {
            GcmUtilities.Xor(Y, b);
            Tables8kGcmMultiplier_MultiplyH(Y);
        }

        private void gHASHBlock(byte[] Y, byte[] b, int off)
        {
            GcmUtilities.Xor(Y, b, off);
            Tables8kGcmMultiplier_MultiplyH(Y);
        }

        private void gHASHPartial(byte[] Y, byte[] b, int off, int len)
        {
            GcmUtilities.Xor(Y, b, off, len);
            Tables8kGcmMultiplier_MultiplyH(Y);
        }

        private void GetNextCtrBlock(byte[] block)
        {
            if (blocksRemaining == 0)
                throw new InvalidOperationException("Attempt to process too many blocks");

            blocksRemaining--;

            uint c = 1;
            c += counter[15]; counter[15] = (byte)c; c >>= 8;
            c += counter[14]; counter[14] = (byte)c; c >>= 8;
            c += counter[13]; counter[13] = (byte)c; c >>= 8;
            c += counter[12]; counter[12] = (byte)c;

            cipher.ProcessBlock(counter, 0, block, 0);
        }

        private void CheckStatus()
        {
            if (!initialised)
            {
                if (forEncryption)
                {
                    throw new InvalidOperationException("GCM cipher cannot be reused for encryption");
                }
                throw new InvalidOperationException("GCM cipher needs to be initialised");
            }
        }

        #region Tables8kGcmMultiplier

        private byte[] Tables8kGcmMultiplier_H;
        private uint[][][] Tables8kGcmMultiplier_M;

        public void Tables8kGcmMultiplier_Init(byte[] H)
        {
            if (Tables8kGcmMultiplier_M == null)
            {
                Tables8kGcmMultiplier_M = new uint[32][][];
            }
            else if (Arrays.AreEqual(this.Tables8kGcmMultiplier_H, H))
            {
                return;
            }

            this.Tables8kGcmMultiplier_H = Arrays.Clone(H);

            Tables8kGcmMultiplier_M[0] = new uint[16][];
            Tables8kGcmMultiplier_M[1] = new uint[16][];
            Tables8kGcmMultiplier_M[0][0] = new uint[4];
            Tables8kGcmMultiplier_M[1][0] = new uint[4];
            Tables8kGcmMultiplier_M[1][8] = GcmUtilities.AsUints(H);

            for (int j = 4; j >= 1; j >>= 1)
            {
                uint[] tmp = (uint[])Tables8kGcmMultiplier_M[1][j + j].Clone();
                GcmUtilities.MultiplyP(tmp);
                Tables8kGcmMultiplier_M[1][j] = tmp;
            }

            {
                uint[] tmp = (uint[])Tables8kGcmMultiplier_M[1][1].Clone();
                GcmUtilities.MultiplyP(tmp);
                Tables8kGcmMultiplier_M[0][8] = tmp;
            }

            for (int j = 4; j >= 1; j >>= 1)
            {
                uint[] tmp = (uint[])Tables8kGcmMultiplier_M[0][j + j].Clone();
                GcmUtilities.MultiplyP(tmp);
                Tables8kGcmMultiplier_M[0][j] = tmp;
            }

            for (int i = 0; ;)
            {
                for (int j = 2; j < 16; j += j)
                {
                    for (int k = 1; k < j; ++k)
                    {
                        uint[] tmp = (uint[])Tables8kGcmMultiplier_M[i][j].Clone();
                        GcmUtilities.Xor(tmp, Tables8kGcmMultiplier_M[i][k]);
                        Tables8kGcmMultiplier_M[i][j + k] = tmp;
                    }
                }

                if (++i == 32) return;

                if (i > 1)
                {
                    Tables8kGcmMultiplier_M[i] = new uint[16][];
                    Tables8kGcmMultiplier_M[i][0] = new uint[4];
                    for (int j = 8; j > 0; j >>= 1)
                    {
                        uint[] tmp = (uint[])Tables8kGcmMultiplier_M[i - 2][j].Clone();
                        GcmUtilities.MultiplyP8(tmp);
                        Tables8kGcmMultiplier_M[i][j] = tmp;
                    }
                }
            }
        }
        uint[] Tables8kGcmMultiplier_z = new uint[4];

        public unsafe void Tables8kGcmMultiplier_MultiplyH(byte[] x)
        {
            fixed (byte* px = x)
            fixed (uint* pz = Tables8kGcmMultiplier_z)
            {
                ulong* pulongZ = (ulong*)pz;
                pulongZ[0] = 0;
                pulongZ[1] = 0;

                for (int i = 15; i >= 0; --i)
                {
                    uint[] m = Tables8kGcmMultiplier_M[i + i][px[i] & 0x0f];
                    fixed (uint* pm = m)
                    {
                        ulong* pulongm = (ulong*)pm;

                        pulongZ[0] ^= pulongm[0];
                        pulongZ[1] ^= pulongm[1];
                    }

                    m = Tables8kGcmMultiplier_M[i + i + 1][(px[i] & 0xf0) >> 4];
                    fixed (uint* pm = m)
                    {
                        ulong* pulongm = (ulong*)pm;

                        pulongZ[0] ^= pulongm[0];
                        pulongZ[1] ^= pulongm[1];
                    }
                }

                byte* pbyteZ = (byte*)pz;
                px[0] = pbyteZ[3];
                px[1] = pbyteZ[2];
                px[2] = pbyteZ[1];
                px[3] = pbyteZ[0];

                px[4] = pbyteZ[7];
                px[5] = pbyteZ[6];
                px[6] = pbyteZ[5];
                px[7] = pbyteZ[4];

                px[8] = pbyteZ[11];
                px[9] = pbyteZ[10];
                px[10] = pbyteZ[9];
                px[11] = pbyteZ[8];

                px[12] = pbyteZ[15];
                px[13] = pbyteZ[14];
                px[14] = pbyteZ[13];
                px[15] = pbyteZ[12];
            }
        }
        #endregion
    }
}
#pragma warning restore
#endif
