namespace GameFrameX
{
    public static partial class Utility
    {
        /// <summary>
        /// Object 对象工具类
        /// </summary>
        public static class Object
        {
            /// <summary>
            /// 交换对象
            /// </summary>
            /// <param name="t1"></param>
            /// <param name="t2"></param>
            /// <typeparam name="T"></typeparam>
            public static void Swap<T>(ref T t1, ref T t2)
            {
                (t1, t2) = (t2, t1);
            }
        }
    }
}