namespace GameFrameX
{
    public static partial class Utility
    {
        /// <summary>
        /// 文件相关的实用函数
        /// </summary>
        public static class File
        {
            private static readonly string[] UnitList = new[] {"B", "KB", "MB", "GB", "TB", "PB"};

            /// <summary>
            /// 获取字节大小
            /// </summary>
            public static string GetBytesSize(long size)
            {
                foreach (var unit in UnitList)
                {
                    if (size <= 1024)
                    {
                        return size + unit;
                    }

                    size /= 1024;
                }

                return size + UnitList[0];
            }
        }
    }
}