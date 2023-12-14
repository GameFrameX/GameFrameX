using System;

namespace GameFrameX
{
    /// <summary>
    /// 游戏框架异常静态方法
    /// </summary>
    public static class GameFrameworkGuard
    {
        /// <summary>
        /// 确保指定的值不为null。
        /// </summary>
        /// <typeparam name="T">值的类型。</typeparam>
        /// <param name="value">要检查的值。</param>
        /// <param name="name">值的名称。</param>
        /// <exception cref="ArgumentNullException">当值为null时引发。</exception>
        public static void NotNull<T>(T value, string name) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(name, " can not be null.");
            }
        }

        /// <summary>
        /// 检查值是否在指定范围内，如果不在范围内则抛出 ArgumentOutOfRangeException 异常。
        /// </summary>
        /// <param name="value">要检查的值。</param>
        /// <param name="min">允许的最小值。</param>
        /// <param name="max">允许的最大值。</param>
        /// <param name="name">值的名称。</param>
        /// <exception cref="ArgumentOutOfRangeException">当值不在指定范围内时抛出。</exception>
        public static void NotRange(int value, int min, int max, string name)
        {
            if (value > max || value < min)
            {
                throw new ArgumentOutOfRangeException(name, "value must between " + min + " and " + max);
            }
        }
    }
}