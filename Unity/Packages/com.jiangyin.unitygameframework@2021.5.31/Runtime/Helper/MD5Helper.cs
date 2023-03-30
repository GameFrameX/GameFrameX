using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// Md5 帮助类
    /// </summary>
    public static class MD5Helper
    {
        private static readonly MD5 MD5 = MD5.Create();

        /// <summary>
        /// 获取字符串的Md5值
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string GetMD5Hash(string input)
        {
            var data = MD5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return ToHash(data);
        }

        /// <summary>
        /// 获取流的Md5值
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string GetMD5Hash(Stream input)
        {
            var data = MD5.ComputeHash(input);
            return ToHash(data);
        }

        /// <summary>
        /// 验证Md5值是否一致
        /// </summary>
        /// <param name="input"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static bool VerifyMd5Hash(string input, string hash)
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
        public static string FileMD5(string filePath)
        {
            using (FileStream file = new FileStream(filePath, FileMode.Open))
            {
                return GetMD5Hash(file);
            }
        }
    }
}