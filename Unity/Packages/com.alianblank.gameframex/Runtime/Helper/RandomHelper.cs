using System;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// 随机数帮助类
    /// </summary>
    public static class RandomHelper
    {
        private static Random _random = new Random((int) DateTime.UtcNow.Ticks);

        /// <summary>
        /// 设置随机种子
        /// </summary>
        /// <param name="seed"></param>
        public static void SetSeed(int seed)
        {
            _random = new Random(seed);
        }

        /// <summary>
        /// 获取UInt64范围内的随机数
        /// </summary>
        /// <returns></returns>
        public static ulong NextUInt64()
        {
            var bytes = new byte[8];
            _random.NextBytes(bytes);
            return BitConverter.ToUInt64(bytes, 0);
        }

        /// <summary>
        /// 获取Int64范围内的随机数
        /// </summary>
        /// <returns></returns>
        public static long NextInt64()
        {
            var bytes = new byte[8];
            _random.NextBytes(bytes);
            return BitConverter.ToInt64(bytes, 0);
        }

        /// <summary>
        /// 获取lower与Upper之间的随机数
        /// </summary>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        /// <returns></returns>
        public static int Next(int lower, int upper)
        {
            return _random.Next(lower, upper);
        }

        /// <summary>
        /// 获取0与1之间的随机数
        /// </summary>
        /// <returns></returns>
        public static float Next()
        {
            return _random.Next(0, 100_000) / 100_000f;
        }
    }
}