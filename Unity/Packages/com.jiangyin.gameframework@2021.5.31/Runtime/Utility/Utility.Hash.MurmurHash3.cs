using System;

namespace GameFramework
{
    public static partial class Utility
    {
        /// <summary>
        /// 哈希计算相关的实用函数。
        /// </summary>
        public static partial class Hash
        {
            /// <summary>
            /// MurmurHash3
            /// </summary>
            public static class MurmurHash3
            {
                public static uint Hash(string str, uint seed = 27)
                {
                    var data = System.Text.Encoding.UTF8.GetBytes(str);
                    return Hash(data, (uint) data.Length, seed);
                }

                private static uint Hash(byte[] data, uint length, uint seed)
                {
                    uint nBlocks = length >> 2;

                    uint h1 = seed;

                    const uint c1 = 0xcc9e2d51;
                    const uint c2 = 0x1b873593;

                    //----------
                    // body

                    int i = 0;

                    for (uint j = nBlocks; j > 0; --j)
                    {
                        uint k1L = BitConverter.ToUInt32(data, i);

                        k1L *= c1;
                        k1L = Rotl32(k1L, 15);
                        k1L *= c2;

                        h1 ^= k1L;
                        h1 = Rotl32(h1, 13);
                        h1 = h1 * 5 + 0xe6546b64;

                        i += 4;
                    }

                    //----------
                    // tail

                    nBlocks <<= 2;

                    uint k1 = 0;

                    uint tailLength = length & 3;

                    if (tailLength == 3)
                        k1 ^= (uint) data[2 + nBlocks] << 16;
                    if (tailLength >= 2)
                        k1 ^= (uint) data[1 + nBlocks] << 8;
                    if (tailLength >= 1)
                    {
                        k1 ^= data[nBlocks];
                        k1 *= c1;
                        k1 = Rotl32(k1, 15);
                        k1 *= c2;
                        h1 ^= k1;
                    }

                    //----------
                    // finalization

                    h1 ^= length;

                    h1 = Fmix32(h1);

                    return h1;
                }

                static uint Fmix32(uint h)
                {
                    h ^= h >> 16;
                    h *= 0x85ebca6b;
                    h ^= h >> 13;
                    h *= 0xc2b2ae35;
                    h ^= h >> 16;

                    return h;
                }

                static uint Rotl32(uint x, byte r)
                {
                    return x << r | x >> 32 - r;
                }
            }
        }
    }
}