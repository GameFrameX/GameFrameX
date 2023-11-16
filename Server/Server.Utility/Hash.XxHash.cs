using System.Runtime.CompilerServices;
using System.Text;

namespace Server.Utility
{
    /// <summary>
    /// 哈希计算相关的实用函数。
    /// </summary>
    public static partial class Hash
    {
        /// <summary>
        /// xxHash
        /// </summary>
        public static class XXHash
        {
            /// <summary>
            /// 计算给定字节数组的32位哈希值
            /// </summary>
            /// <param name="buffer">要计算哈希值的字节数组</param>
            /// <returns>32位哈希值</returns>
            public static uint Hash32(byte[] buffer)
            {
                return XXHashHelper.Hash32(buffer);
            }

            /// <summary>
            /// 计算给定文本的32位哈希值。
            /// </summary>
            /// <param name="text">要计算哈希值的文本。</param>
            /// <returns>32位哈希值。</returns>
            public static uint Hash32(string text)
            {
                return XXHashHelper.Hash32(text);
            }

            /// <summary>
            /// 计算给定类型的32位哈希值。
            /// </summary>
            /// <param name="type">要计算哈希值的类型。</param>
            /// <returns>32位哈希值。</returns>
            public static uint Hash32(Type type)
            {
                return XXHashHelper.Hash32(type);
            }

            /// <summary>
            /// 计算给定泛型类型参数的32位哈希值。
            /// </summary>
            /// <typeparam name="T">要计算哈希值的泛型类型参数。</typeparam>
            /// <returns>32位哈希值。</returns>
            public static uint Hash32<T>()
            {
                return XXHashHelper.Hash32<T>();
            }

            /// <summary>
            /// 计算给定字节数组的64位哈希值。
            /// </summary>
            /// <param name="buffer">要计算哈希值的字节数组。</param>
            /// <returns>64位哈希值。</returns>
            public static ulong Hash64(byte[] buffer)
            {
                return XXHashHelper.Hash64(buffer);
            }

            /// <summary>
            /// 计算给定文本的64位哈希值。
            /// </summary>
            /// <param name="text">要计算哈希值的文本。</param>
            /// <returns>64位哈希值。</returns>
            public static ulong Hash64(string text)
            {
                return XXHashHelper.Hash64(text);
            }

            /// <summary>
            /// 计算给定类型的64位哈希值。
            /// </summary>
            /// <param name="type">要计算哈希值的类型。</param>
            /// <returns>64位哈希值。</returns>
            public static ulong Hash64(Type type)
            {
                return XXHashHelper.Hash64(type);
            }

            /// <summary>
            /// 计算给定泛型类型参数的64位哈希值。
            /// </summary>
            /// <typeparam name="T">要计算哈希值的泛型类型参数。</typeparam>
            /// <returns>64位哈希值。</returns>
            public static ulong Hash64<T>()
            {
                return XXHashHelper.Hash64<T>();
            }
        }

        /// <summary>
        /// XX Hash 计算帮助类
        /// </summary>
        static class XXHashHelper
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static unsafe uint Hash32(byte* input, int length, uint seed = 0)
            {
                unchecked
                {
                    const uint prime1 = 2654435761u;
                    const uint prime2 = 2246822519u;
                    const uint prime3 = 3266489917u;
                    const uint prime4 = 0668265263u;
                    const uint prime5 = 0374761393u;

                    uint hash = seed + prime5;

                    if (length >= 16)
                    {
                        uint val0 = seed + prime1 + prime2;
                        uint val1 = seed + prime2;
                        uint val2 = seed + 0;
                        uint val3 = seed - prime1;

                        int count = length >> 4;
                        for (int i = 0; i < count; i++)
                        {
                            var pos0 = *(uint*)(input + 0);
                            var pos1 = *(uint*)(input + 4);
                            var pos2 = *(uint*)(input + 8);
                            var pos3 = *(uint*)(input + 12);

                            val0 += pos0 * prime2;
                            val0 = (val0 << 13) | (val0 >> (32 - 13));
                            val0 *= prime1;

                            val1 += pos1 * prime2;
                            val1 = (val1 << 13) | (val1 >> (32 - 13));
                            val1 *= prime1;

                            val2 += pos2 * prime2;
                            val2 = (val2 << 13) | (val2 >> (32 - 13));
                            val2 *= prime1;

                            val3 += pos3 * prime2;
                            val3 = (val3 << 13) | (val3 >> (32 - 13));
                            val3 *= prime1;

                            input += 16;
                        }

                        hash = ((val0 << 01) | (val0 >> (32 - 01))) +
                               ((val1 << 07) | (val1 >> (32 - 07))) +
                               ((val2 << 12) | (val2 >> (32 - 12))) +
                               ((val3 << 18) | (val3 >> (32 - 18)));
                    }

                    hash += (uint)length;

                    length &= 15;
                    while (length >= 4)
                    {
                        hash += *(uint*)input * prime3;
                        hash = ((hash << 17) | (hash >> (32 - 17))) * prime4;
                        input += 4;
                        length -= 4;
                    }

                    while (length > 0)
                    {
                        hash += *input * prime5;
                        hash = ((hash << 11) | (hash >> (32 - 11))) * prime1;
                        ++input;
                        --length;
                    }

                    hash ^= hash >> 15;
                    hash *= prime2;
                    hash ^= hash >> 13;
                    hash *= prime3;
                    hash ^= hash >> 16;

                    return hash;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static unsafe ulong Hash64(byte* input, int length, uint seed = 0)
            {
                unchecked
                {
                    const ulong prime1 = 11400714785074694791ul;
                    const ulong prime2 = 14029467366897019727ul;
                    const ulong prime3 = 01609587929392839161ul;
                    const ulong prime4 = 09650029242287828579ul;
                    const ulong prime5 = 02870177450012600261ul;

                    ulong hash = seed + prime5;

                    if (length >= 32)
                    {
                        ulong val0 = seed + prime1 + prime2;
                        ulong val1 = seed + prime2;
                        ulong val2 = seed + 0;
                        ulong val3 = seed - prime1;

                        int count = length >> 5;
                        for (int i = 0; i < count; i++)
                        {
                            var pos0 = *(ulong*)(input + 0);
                            var pos1 = *(ulong*)(input + 8);
                            var pos2 = *(ulong*)(input + 16);
                            var pos3 = *(ulong*)(input + 24);

                            val0 += pos0 * prime2;
                            val0 = (val0 << 31) | (val0 >> (64 - 31));
                            val0 *= prime1;

                            val1 += pos1 * prime2;
                            val1 = (val1 << 31) | (val1 >> (64 - 31));
                            val1 *= prime1;

                            val2 += pos2 * prime2;
                            val2 = (val2 << 31) | (val2 >> (64 - 31));
                            val2 *= prime1;

                            val3 += pos3 * prime2;
                            val3 = (val3 << 31) | (val3 >> (64 - 31));
                            val3 *= prime1;

                            input += 32;
                        }

                        hash = ((val0 << 01) | (val0 >> (64 - 01))) +
                               ((val1 << 07) | (val1 >> (64 - 07))) +
                               ((val2 << 12) | (val2 >> (64 - 12))) +
                               ((val3 << 18) | (val3 >> (64 - 18)));

                        val0 *= prime2;
                        val0 = (val0 << 31) | (val0 >> (64 - 31));
                        val0 *= prime1;
                        hash ^= val0;
                        hash = hash * prime1 + prime4;

                        val1 *= prime2;
                        val1 = (val1 << 31) | (val1 >> (64 - 31));
                        val1 *= prime1;
                        hash ^= val1;
                        hash = hash * prime1 + prime4;

                        val2 *= prime2;
                        val2 = (val2 << 31) | (val2 >> (64 - 31));
                        val2 *= prime1;
                        hash ^= val2;
                        hash = hash * prime1 + prime4;

                        val3 *= prime2;
                        val3 = (val3 << 31) | (val3 >> (64 - 31));
                        val3 *= prime1;
                        hash ^= val3;
                        hash = hash * prime1 + prime4;
                    }

                    hash += (ulong)length;

                    length &= 31;
                    while (length >= 8)
                    {
                        ulong lane = *(ulong*)input * prime2;
                        lane = ((lane << 31) | (lane >> (64 - 31))) * prime1;
                        hash ^= lane;
                        hash = ((hash << 27) | (hash >> (64 - 27))) * prime1 + prime4;
                        input += 8;
                        length -= 8;
                    }

                    if (length >= 4)
                    {
                        hash ^= *(uint*)input * prime1;
                        hash = ((hash << 23) | (hash >> (64 - 23))) * prime2 + prime3;
                        input += 4;
                        length -= 4;
                    }

                    while (length > 0)
                    {
                        hash ^= *input * prime5;
                        hash = ((hash << 11) | (hash >> (64 - 11))) * prime1;
                        ++input;
                        --length;
                    }

                    hash ^= hash >> 33;
                    hash *= prime2;
                    hash ^= hash >> 29;
                    hash *= prime3;
                    hash ^= hash >> 32;

                    return hash;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint Hash32(byte[] buffer)
            {
                int length = buffer.Length;
                unsafe
                {
                    fixed (byte* pointer = buffer)
                    {
                        return Hash32(pointer, length);
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint Hash32(string text) => Hash32(Encoding.UTF8.GetBytes(text));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint Hash32(Type type) => Hash32(type.FullName);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint Hash32<T>() => Hash32(typeof(T));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong Hash64(byte[] buffer)
            {
                int length = buffer.Length;
                unsafe
                {
                    fixed (byte* pointer = buffer)
                    {
                        return Hash64(pointer, length);
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong Hash64(string text) => Hash64(Encoding.UTF8.GetBytes(text));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong Hash64(Type type) => Hash64(type.FullName);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong Hash64<T>() => Hash64(typeof(T));
        }
    }
}