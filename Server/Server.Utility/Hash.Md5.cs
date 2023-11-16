using System.Text;

namespace Server.Utility
{
    /// <summary>
    /// 哈希计算相关的实用函数。
    /// </summary>
    public static partial class Hash
    {
        /// <summary>
        /// Md5
        /// </summary>
        public static class MD5
        {
            private static readonly System.Security.Cryptography.MD5 MD5Cryptography = System.Security.Cryptography.MD5.Create();

            /// <summary>
            /// 获取字符串的Md5值
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            public static string Hash(string input)
            {
                var data = MD5Cryptography.ComputeHash(Encoding.UTF8.GetBytes(input));
                return ToHash(data);
            }

            /// <summary>
            /// 获取流的Md5值
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            public static string Hash(Stream input)
            {
                var data = MD5Cryptography.ComputeHash(input);
                return ToHash(data);
            }

            /// <summary>
            /// 验证Md5值是否一致
            /// </summary>
            /// <param name="input"></param>
            /// <param name="hash"></param>
            /// <returns></returns>
            public static bool IsVerify(string input, string hash)
            {
                var comparer = StringComparer.OrdinalIgnoreCase;
                return 0 == comparer.Compare(input, hash);
            }

            static string ToHash(byte[] data)
            {
                var sb = new StringBuilder();
                foreach (var t in data)
                {
                    sb.Append(t.ToString("x2"));
                }

                return sb.ToString();
            }

            /// <summary>
            /// 获取指定文件路径的Md5值
            /// </summary>
            /// <param name="filePath"></param>
            /// <returns></returns>
            public static string HashByFilePath(string filePath)
            {
                using var file = new FileStream(filePath, FileMode.Open);
                return Hash(file);
            }

            /// <summary>
            /// 计算字节数组的Hash 值
            /// </summary>
            /// <param name="inputBytes"></param>
            /// <returns></returns>
            public static string Hash(byte[] inputBytes)
            {
                var hashBytes = MD5Cryptography.ComputeHash(inputBytes);
                var hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);
                return hash;
            }
        }
    }
}