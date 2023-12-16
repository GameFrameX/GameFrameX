//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System.Runtime.Intrinsics.Arm;

namespace Server.Utility
{
    /// <summary>
    /// 校验相关的实用函数。
    /// </summary>
    public static partial class Verifier
    {
        private const int CachedBytesLength = 0x1000;
        private static readonly byte[] SCachedBytes = new byte[CachedBytesLength];
        private static readonly Server.Utility.Verifier.Crc32 SAlgorithm = new Server.Utility.Verifier.Crc32();
        private static readonly Server.Utility.Verifier.Crc64 SAlgorithm64 = new Server.Utility.Verifier.Crc64();

        /// <summary>
        /// 计算二进制流的CRC64
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static ulong GetCrc64(byte[] bytes)
        {
            SAlgorithm64.Reset();
            SAlgorithm64.Append(bytes);
            return SAlgorithm64.GetCurrentHashAsUInt64();
        }

        /// <summary>
        /// 计算流的CRC64
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static ulong GetCrc64(Stream stream)
        {
            SAlgorithm64.Reset();
            SAlgorithm64.Append(stream);
            return SAlgorithm64.GetCurrentHashAsUInt64();
        }

        /// <summary>
        /// 计算二进制流的 CRC32。
        /// </summary>
        /// <param name="bytes">指定的二进制流。</param>
        /// <returns>计算后的 CRC32。</returns>
        public static int GetCrc32(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes), "Bytes is invalid.");
            }

            return GetCrc32(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// 计算二进制流的 CRC32。
        /// </summary>
        /// <param name="bytes">指定的二进制流。</param>
        /// <param name="offset">二进制流的偏移。</param>
        /// <param name="length">二进制流的长度。</param>
        /// <returns>计算后的 CRC32。</returns>
        public static int GetCrc32(byte[] bytes, int offset, int length)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes), "Bytes is invalid.");
            }

            if (offset < 0 || length < 0 || offset + length > bytes.Length)
            {
                throw new ArgumentException("Offset or length is invalid.", nameof(offset));
            }

            SAlgorithm.HashCore(bytes, offset, length);
            int result = (int)SAlgorithm.HashFinal();
            SAlgorithm.Initialize();
            return result;
        }

        /// <summary>
        /// 计算二进制流的 CRC32。
        /// </summary>
        /// <param name="stream">指定的二进制流。</param>
        /// <returns>计算后的 CRC32。</returns>
        public static int GetCrc32(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream), "Stream is invalid.");
            }

            while (true)
            {
                int bytesRead = stream.Read(SCachedBytes, 0, CachedBytesLength);
                if (bytesRead > 0)
                {
                    SAlgorithm.HashCore(SCachedBytes, 0, bytesRead);
                }
                else
                {
                    break;
                }
            }

            int result = (int)SAlgorithm.HashFinal();
            SAlgorithm.Initialize();
            Array.Clear(SCachedBytes, 0, CachedBytesLength);
            return result;
        }

        /// <summary>
        /// 获取 CRC32 数值的二进制数组。
        /// </summary>
        /// <param name="crc32">CRC32 数值。</param>
        /// <returns>CRC32 数值的二进制数组。</returns>
        public static byte[] GetCrc32Bytes(int crc32)
        {
            return new byte[] { (byte)((crc32 >> 24) & 0xff), (byte)((crc32 >> 16) & 0xff), (byte)((crc32 >> 8) & 0xff), (byte)(crc32 & 0xff) };
        }

        /// <summary>
        /// 获取 CRC32 数值的二进制数组。
        /// </summary>
        /// <param name="crc32">CRC32 数值。</param>
        /// <param name="bytes">要存放结果的数组。</param>
        public static void GetCrc32Bytes(int crc32, byte[] bytes)
        {
            GetCrc32Bytes(crc32, bytes, 0);
        }

        /// <summary>
        /// 获取 CRC32 数值的二进制数组。
        /// </summary>
        /// <param name="crc32">CRC32 数值。</param>
        /// <param name="bytes">要存放结果的数组。</param>
        /// <param name="offset">CRC32 数值的二进制数组在结果数组内的起始位置。</param>
        public static void GetCrc32Bytes(int crc32, byte[] bytes, int offset)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes), "Result is invalid.");
            }

            if (offset < 0 || offset + 4 > bytes.Length)
            {
                throw new ArgumentException("Offset or length is invalid.", nameof(offset));
            }

            bytes[offset] = (byte)((crc32 >> 24) & 0xff);
            bytes[offset + 1] = (byte)((crc32 >> 16) & 0xff);
            bytes[offset + 2] = (byte)((crc32 >> 8) & 0xff);
            bytes[offset + 3] = (byte)(crc32 & 0xff);
        }

        internal static int GetCrc32(Stream stream, byte[] code, int length)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream), "Stream is invalid.");
            }

            if (code == null)
            {
                throw new ArgumentNullException(nameof(code), "Code is invalid.");
            }

            int codeLength = code.Length;
            if (codeLength <= 0)
            {
                throw new ArgumentException("Code length is invalid.", nameof(codeLength));
            }

            int bytesLength = (int)stream.Length;
            if (length < 0 || length > bytesLength)
            {
                length = bytesLength;
            }

            int codeIndex = 0;
            while (true)
            {
                int bytesRead = stream.Read(SCachedBytes, 0, CachedBytesLength);
                if (bytesRead > 0)
                {
                    if (length > 0)
                    {
                        for (int i = 0; i < bytesRead && i < length; i++)
                        {
                            SCachedBytes[i] ^= code[codeIndex++];
                            codeIndex %= codeLength;
                        }

                        length -= bytesRead;
                    }

                    SAlgorithm.HashCore(SCachedBytes, 0, bytesRead);
                }
                else
                {
                    break;
                }
            }

            int result = (int)SAlgorithm.HashFinal();
            SAlgorithm.Initialize();
            Array.Clear(SCachedBytes, 0, CachedBytesLength);
            return result;
        }
    }
}