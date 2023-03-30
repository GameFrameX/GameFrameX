#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Utilities;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Digests
{
    /**
    * Draft FIPS 180-2 implementation of SHA-256. <b>Note:</b> As this is
    * based on a draft this implementation is subject to change.
    *
    * <pre>
    *         block  word  digest
    * SHA-1   512    32    160
    * SHA-256 512    32    256
    * SHA-384 1024   64    384
    * SHA-512 1024   64    512
    * </pre>
    */
    [BestHTTP.PlatformSupport.IL2CPP.Il2CppSetOption(BestHTTP.PlatformSupport.IL2CPP.Option.NullChecks, false)]
    [BestHTTP.PlatformSupport.IL2CPP.Il2CppSetOption(BestHTTP.PlatformSupport.IL2CPP.Option.ArrayBoundsChecks, false)]
    [BestHTTP.PlatformSupport.IL2CPP.Il2CppSetOption(BestHTTP.PlatformSupport.IL2CPP.Option.DivideByZeroChecks, false)]
    [BestHTTP.PlatformSupport.IL2CPP.Il2CppEagerStaticClassConstructionAttribute]
    public sealed class Sha256Digest
		: GeneralDigest
    {
        private const int DigestLength = 32;

        private uint H1, H2, H3, H4, H5, H6, H7, H8;
        private uint[] X = new uint[64];
        private int xOff;

        public Sha256Digest()
        {
			initHs();
        }

        /**
        * Copy constructor.  This will copy the state of the provided
        * message digest.
        */
        public Sha256Digest(Sha256Digest t) : base(t)
        {
			CopyIn(t);
		}

		private void CopyIn(Sha256Digest t)
		{
			base.CopyIn(t);

            H1 = t.H1;
            H2 = t.H2;
            H3 = t.H3;
            H4 = t.H4;
            H5 = t.H5;
            H6 = t.H6;
            H7 = t.H7;
            H8 = t.H8;

            Array.Copy(t.X, 0, X, 0, t.X.Length);
            xOff = t.xOff;
        }

        public override string AlgorithmName
		{
			get { return "SHA-256"; }
		}

		public override int GetDigestSize()
		{
			return DigestLength;
		}

		internal override void ProcessLength(
            long bitLength)
        {
            if (xOff > 14)
            {
                ProcessBlock();
            }

            X[14] = (uint)((ulong)bitLength >> 32);
            X[15] = (uint)((ulong)bitLength);
        }

        public override int DoFinal(
            byte[]  output,
            int     outOff)
        {
            Finish();

            Pack.UInt32_To_BE((uint)H1, output, outOff);
            Pack.UInt32_To_BE((uint)H2, output, outOff + 4);
            Pack.UInt32_To_BE((uint)H3, output, outOff + 8);
            Pack.UInt32_To_BE((uint)H4, output, outOff + 12);
            Pack.UInt32_To_BE((uint)H5, output, outOff + 16);
            Pack.UInt32_To_BE((uint)H6, output, outOff + 20);
            Pack.UInt32_To_BE((uint)H7, output, outOff + 24);
            Pack.UInt32_To_BE((uint)H8, output, outOff + 28);

            Reset();

            return DigestLength;
        }

        /**
        * reset the chaining variables
        */
        public override void Reset()
        {
            base.Reset();

			initHs();

            xOff = 0;
			Array.Clear(X, 0, X.Length);
        }

		private void initHs()
		{
            /* SHA-256 initial hash value
            * The first 32 bits of the fractional parts of the square roots
            * of the first eight prime numbers
            */
            H1 = 0x6a09e667;
            H2 = 0xbb67ae85;
            H3 = 0x3c6ef372;
            H4 = 0xa54ff53a;
            H5 = 0x510e527f;
            H6 = 0x9b05688c;
            H7 = 0x1f83d9ab;
            H8 = 0x5be0cd19;
		}

        internal unsafe override void ProcessWord(
            byte[] input,
            int inOff)
        {
            fixed (uint* pX = X)
            {
                fixed (byte* bs = input)
                    pX[xOff] = (uint)bs[inOff] << 24 | (uint)bs[inOff + 1] << 16 | (uint)bs[inOff + 2] << 8 | (uint)bs[inOff + 3];

                if (++xOff == 16)
                {
                    //
                    // expand 16 word block into 64 word blocks.
                    //
                    for (int ti = 16; ti <= 63; ti++)
                    {
                        uint x = pX[ti - 2];
                        uint y = pX[ti - 15];
                        pX[ti] = (((x >> 17) | (x << 15)) ^ ((x >> 19) | (x << 13)) ^ (x >> 10))
                            + pX[ti - 7]
                            + (((y >> 7) | (y << 25)) ^ ((y >> 18) | (y << 14)) ^ (y >> 3))
                            + pX[ti - 16];
                    }

                    //
                    // set up working variables.
                    //
                    uint a = H1;
                    uint b = H2;
                    uint c = H3;
                    uint d = H4;
                    uint e = H5;
                    uint f = H6;
                    uint g = H7;
                    uint h = H8;

                    int t = 0;

                    fixed (uint* pK = K)
                    {
                        uint* pnfK = pK, pnfX = pX;

                        for (int i = 0; i < 8; ++i)
                        {
                            // t = 8 * i
                            h += ((((e >> 6) | (e << 26)) ^ ((e >> 11) | (e << 21)) ^ ((e >> 25) | (e << 7))) + (g ^ (e & (f ^ g))))/*Sum1Ch(e, f, g)*/ + *pnfK++ + *pnfX++;
                            d += h;
                            h += ((((a >> 2) | (a << 30)) ^ ((a >> 13) | (a << 19)) ^ ((a >> 22) | (a << 10))) + ((a & b) | (c & (a ^ b))))/*Sum0Maj(a, b, c)*/;

                            // t = 8 * i + 1
                            g += ((((d >> 6) | (d << 26)) ^ ((d >> 11) | (d << 21)) ^ ((d >> 25) | (d << 7))) + (f ^ (d & (e ^ f))))/*Sum1Ch(d, e, f)*/ + *pnfK++ + *pnfX++;
                            c += g;
                            g += ((((h >> 2) | (h << 30)) ^ ((h >> 13) | (h << 19)) ^ ((h >> 22) | (h << 10))) + ((h & a) | (b & (h ^ a))))/*Sum0Maj(h, a, b)*/;

                            // t = 8 * i + 2
                            f += ((((c >> 6) | (c << 26)) ^ ((c >> 11) | (c << 21)) ^ ((c >> 25) | (c << 7))) + (e ^ (c & (d ^ e))))/*Sum1Ch(c, d, e)*/ + *pnfK++ + *pnfX++;
                            b += f;
                            f += ((((g >> 2) | (g << 30)) ^ ((g >> 13) | (g << 19)) ^ ((g >> 22) | (g << 10))) + ((g & h) | (a & (g ^ h))))/*Sum0Maj(g, h, a)*/;

                            // t = 8 * i + 3
                            e += ((((b >> 6) | (b << 26)) ^ ((b >> 11) | (b << 21)) ^ ((b >> 25) | (b << 7))) + (d ^ (b & (c ^ d))))/*Sum1Ch(b, c, d)*/ + *pnfK++ + *pnfX++;
                            a += e;
                            e += ((((f >> 2) | (f << 30)) ^ ((f >> 13) | (f << 19)) ^ ((f >> 22) | (f << 10))) + ((f & g) | (h & (f ^ g))))/*Sum0Maj(f, g, h)*/;

                            // t = 8 * i + 4
                            d += ((((a >> 6) | (a << 26)) ^ ((a >> 11) | (a << 21)) ^ ((a >> 25) | (a << 7))) + (c ^ (a & (b ^ c))))/*Sum1Ch(a, b, c)*/ + *pnfK++ + *pnfX++;
                            h += d;
                            d += ((((e >> 2) | (e << 30)) ^ ((e >> 13) | (e << 19)) ^ ((e >> 22) | (e << 10))) + ((e & f) | (g & (e ^ f))))/*Sum0Maj(e, f, g)*/;

                            // t = 8 * i + 5
                            c += ((((h >> 6) | (h << 26)) ^ ((h >> 11) | (h << 21)) ^ ((h >> 25) | (h << 7))) + (b ^ (h & (a ^ b))))/*Sum1Ch(h, a, b)*/ + *pnfK++ + *pnfX++;
                            g += c;
                            c += ((((d >> 2) | (d << 30)) ^ ((d >> 13) | (d << 19)) ^ ((d >> 22) | (d << 10))) + ((d & e) | (f & (d ^ e))))/*Sum0Maj(d, e, f)*/;

                            // t = 8 * i + 6
                            b += ((((g >> 6) | (g << 26)) ^ ((g >> 11) | (g << 21)) ^ ((g >> 25) | (g << 7))) + (a ^ (g & (h ^ a))))/*Sum1Ch(g, h, a)*/ + *pnfK++ + *pnfX++;
                            f += b;
                            b += ((((c >> 2) | (c << 30)) ^ ((c >> 13) | (c << 19)) ^ ((c >> 22) | (c << 10))) + ((c & d) | (e & (c ^ d))))/*Sum0Maj(c, d, e)*/;

                            // t = 8 * i + 7
                            a += ((((f >> 6) | (f << 26)) ^ ((f >> 11) | (f << 21)) ^ ((f >> 25) | (f << 7))) + (h ^ (f & (g ^ h))))/*Sum1Ch(f, g, h)*/ + *pnfK++ + *pnfX++;
                            e += a;
                            a += ((((b >> 2) | (b << 30)) ^ ((b >> 13) | (b << 19)) ^ ((b >> 22) | (b << 10))) + ((b & c) | (d & (b ^ c))))/*Sum0Maj(b, c, d)*/;
                        }
                    }

                    H1 += a;
                    H2 += b;
                    H3 += c;
                    H4 += d;
                    H5 += e;
                    H6 += f;
                    H7 += g;
                    H8 += h;

                    //
                    // reset the offset and clean out the word buffer.
                    //
                    xOff = 0;

                    ulong* pulongX = (ulong*)pX;
                    *pulongX++ = 0;
                    *pulongX++ = 0;
                    *pulongX++ = 0;
                    *pulongX++ = 0;
                    *pulongX++ = 0;
                    *pulongX++ = 0;
                    *pulongX++ = 0;
                    *pulongX = 0;
                }
            }
        }

        internal unsafe override void ProcessBlock()
        {
            //
            // expand 16 word block into 64 word blocks.
            //
            fixed(uint* pX = X)
            {
                for (int ti = 16; ti <= 63; ti++)
                {
                    uint x = pX[ti - 2];
                    uint y = pX[ti - 15];
                    pX[ti] = (((x >> 17) | (x << 15)) ^ ((x >> 19) | (x << 13)) ^ (x >> 10))
                        + pX[ti - 7]
                        + (((y >> 7) | (y << 25)) ^ ((y >> 18) | (y << 14)) ^ (y >> 3))
                        + pX[ti - 16];
                }
            }

            //
            // set up working variables.
            //
            uint a = H1;
            uint b = H2;
            uint c = H3;
            uint d = H4;
            uint e = H5;
            uint f = H6;
            uint g = H7;
            uint h = H8;

			int t = 0;

            fixed(uint* pX = X, pK = K)
            {
                uint* pnfK = pK, pnfX = pX;

                for (int i = 0; i < 8; ++i)
                {
                	// t = 8 * i
                	h += ((((e >> 6) | (e << 26)) ^ ((e >> 11) | (e << 21)) ^ ((e >> 25) | (e << 7))) + (g ^ (e & (f ^ g))))/*Sum1Ch(e, f, g)*/ + *pnfK++ + *pnfX++;
                	d += h;
                	h += ((((a >> 2) | (a << 30)) ^ ((a >> 13) | (a << 19)) ^ ((a >> 22) | (a << 10))) + ((a & b) | (c & (a ^ b))))/*Sum0Maj(a, b, c)*/;
                
                	// t = 8 * i + 1
                	g += ((((d >> 6) | (d << 26)) ^ ((d >> 11) | (d << 21)) ^ ((d >> 25) | (d << 7))) + (f ^ (d & (e ^ f))))/*Sum1Ch(d, e, f)*/ + *pnfK++ + *pnfX++;
                	c += g;
                	g += ((((h >> 2) | (h << 30)) ^ ((h >> 13) | (h << 19)) ^ ((h >> 22) | (h << 10))) + ((h & a) | (b & (h ^ a))))/*Sum0Maj(h, a, b)*/;
                
                	// t = 8 * i + 2
                	f += ((((c >> 6) | (c << 26)) ^ ((c >> 11) | (c << 21)) ^ ((c >> 25) | (c << 7))) + (e ^ (c & (d ^ e))))/*Sum1Ch(c, d, e)*/ + *pnfK++ + *pnfX++;
                	b += f;
                	f += ((((g >> 2) | (g << 30)) ^ ((g >> 13) | (g << 19)) ^ ((g >> 22) | (g << 10))) + ((g & h) | (a & (g ^ h))))/*Sum0Maj(g, h, a)*/;
                
                	// t = 8 * i + 3
                	e += ((((b >> 6) | (b << 26)) ^ ((b >> 11) | (b << 21)) ^ ((b >> 25) | (b << 7))) + (d ^ (b & (c ^ d))))/*Sum1Ch(b, c, d)*/ + *pnfK++ + *pnfX++;
                	a += e;
                	e += ((((f >> 2) | (f << 30)) ^ ((f >> 13) | (f << 19)) ^ ((f >> 22) | (f << 10))) + ((f & g) | (h & (f ^ g))))/*Sum0Maj(f, g, h)*/;
                
                	// t = 8 * i + 4
                	d += ((((a >> 6) | (a << 26)) ^ ((a >> 11) | (a << 21)) ^ ((a >> 25) | (a << 7))) + (c ^ (a & (b ^ c))))/*Sum1Ch(a, b, c)*/ + *pnfK++ + *pnfX++;
                	h += d;
                	d += ((((e >> 2) | (e << 30)) ^ ((e >> 13) | (e << 19)) ^ ((e >> 22) | (e << 10))) + ((e & f) | (g & (e ^ f))))/*Sum0Maj(e, f, g)*/;
                
                	// t = 8 * i + 5
                	c += ((((h >> 6) | (h << 26)) ^ ((h >> 11) | (h << 21)) ^ ((h >> 25) | (h << 7))) + (b ^ (h & (a ^ b))))/*Sum1Ch(h, a, b)*/ + *pnfK++ + *pnfX++;
                	g += c;
                	c += ((((d >> 2) | (d << 30)) ^ ((d >> 13) | (d << 19)) ^ ((d >> 22) | (d << 10))) + ((d & e) | (f & (d ^ e))))/*Sum0Maj(d, e, f)*/;
                
                	// t = 8 * i + 6
                	b += ((((g >> 6) | (g << 26)) ^ ((g >> 11) | (g << 21)) ^ ((g >> 25) | (g << 7))) + (a ^ (g & (h ^ a))))/*Sum1Ch(g, h, a)*/ + *pnfK++ + *pnfX++;
                	f += b;
                	b += ((((c >> 2) | (c << 30)) ^ ((c >> 13) | (c << 19)) ^ ((c >> 22) | (c << 10))) + ((c & d) | (e & (c ^ d))))/*Sum0Maj(c, d, e)*/;
                
                	// t = 8 * i + 7
                	a += ((((f >> 6) | (f << 26)) ^ ((f >> 11) | (f << 21)) ^ ((f >> 25) | (f << 7))) + (h ^ (f & (g ^ h))))/*Sum1Ch(f, g, h)*/ + *pnfK++ + *pnfX++;
                	e += a;
                	a += ((((b >> 2) | (b << 30)) ^ ((b >> 13) | (b << 19)) ^ ((b >> 22) | (b << 10))) + ((b & c) | (d & (b ^ c))))/*Sum0Maj(b, c, d)*/;
                }
            }

            H1 += a;
            H2 += b;
            H3 += c;
            H4 += d;
            H5 += e;
            H6 += f;
            H7 += g;
            H8 += h;

            //
            // reset the offset and clean out the word buffer.
            //
            xOff = 0;

            fixed (uint* pX = X)
            {
                ulong* pulongX = (ulong*)pX;
                *pulongX++ = 0;
                *pulongX++ = 0;
                *pulongX++ = 0;
                *pulongX++ = 0;
                *pulongX++ = 0;
                *pulongX++ = 0;
                *pulongX++ = 0;
                *pulongX = 0;
            }
        }

        /* SHA-256 Constants
        * (represent the first 32 bits of the fractional parts of the
        * cube roots of the first sixty-four prime numbers)
        */
        private static readonly uint[] K = {
            0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5,
			0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
            0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3,
            0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
            0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc,
            0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
            0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7,
            0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
            0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13,
            0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
            0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3,
            0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
            0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5,
            0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
            0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208,
            0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2
        };
		
		public override IMemoable Copy()
		{
			return new Sha256Digest(this);
		}

		public override void Reset(IMemoable other)
		{
			Sha256Digest d = (Sha256Digest)other;

			CopyIn(d);
		}

    }
}
#pragma warning restore
#endif
