using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace UnityGameFramework.Runtime
{
    public static class StringHelper
    {
        /// <summary>
        /// 快速比较两个字符串内容是否一致
        /// </summary>
        /// <param name="self">当前字符串</param>
        /// <param name="target">对比的目标字符串</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">当前对象为空</exception>
        public static bool EqualsFast(this string self, string target)
        {
            if (self == null)
            {
                return target == null;
            }

            if (target == null)
            {
                return false;
            }

            if (self.Length != target.Length)
            {
                return false;
            }

            int ap = self.Length - 1;
            int bp = target.Length - 1;

            while (ap >= 0 && bp >= 0 && self[ap] == target[bp])
            {
                ap--;
                bp--;
            }

            return (bp < 0);
        }

        /// <summary>
        /// 判断字符串是否以目标字符串结尾
        /// </summary>
        /// <param name="self"></param>
        /// <param name="target">目标字符串</param>
        /// <returns></returns>
        public static bool EndsWithFast(this string self, string target)
        {
            int ap = self.Length - 1;
            int bp = target.Length - 1;

            while (ap >= 0 && bp >= 0 && self[ap] == target[bp])
            {
                ap--;
                bp--;
            }

            return (bp < 0);
        }

        /// <summary>
        /// 判断字符串是否以目标字符串开始
        /// </summary>
        /// <param name="self"></param>
        /// <param name="target">目标字符串</param>
        /// <returns></returns>
        public static bool StartsWithFast(this string self, string target)
        {
            int aLen = self.Length;
            int bLen = target.Length;

            int ap = 0;
            int bp = 0;

            while (ap < aLen && bp < bLen && self[ap] == target[bp])
            {
                ap++;
                bp++;
            }

            return (bp == bLen);
        }

        public static IEnumerable<byte> ToBytes(this string str)
        {
            byte[] byteArray = Encoding.Default.GetBytes(str);
            return byteArray;
        }

        public static byte[] ToByteArray(this string str)
        {
            byte[] byteArray = Encoding.Default.GetBytes(str);
            return byteArray;
        }

        /// <summary>
        /// 字符串转UTF8字符数组
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] ToUtf8(this string str)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(str);
            return byteArray;
        }

        public static byte[] HexToBytes(this string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}",
                    hexString));
            }

            var hexAsBytes = new byte[hexString.Length / 2];
            for (int index = 0; index < hexAsBytes.Length; index++)
            {
                string byteValue = "";
                byteValue += hexString[index * 2];
                byteValue += hexString[index * 2 + 1];
                hexAsBytes[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return hexAsBytes;
        }

        /// <summary>
        /// 指定的字符串是 null、空还是仅由空白字符组成。
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsNullOrWhiteSpace(this string self)
        {
            const string nullString = "null";
            return self.EqualsFast(nullString) || string.IsNullOrWhiteSpace(self);
        }

        /// <summary>
        /// 指定的字符串是 null 还是 Empty 字符串。
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string self)
        {
            return string.IsNullOrEmpty(self);
        }

        /// <summary>
        /// 指定的字符串[不]是 null、空还是仅由空白字符组成。
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsNotNullOrWhiteSpace(this string self)
        {
            return !self.IsNullOrWhiteSpace();
        }

        /// <summary>
        /// 指定的字符串[不]是 null 还是 Empty 字符串。
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsNotNullOrEmpty(this string self)
        {
            return !self.IsNullOrEmpty();
        }

        public static string Format(this string text, params object[] args)
        {
            return string.Format(text, args);
        }

        public static string ListToString<T>(this List<T> list)
        {
            StringBuilder sb = new StringBuilder();
            foreach (T t in list)
            {
                sb.Append(t);
                sb.Append(",");
            }

            return sb.ToString();
        }

        public static string TrimEmpty(string origStr)
        {
            origStr = origStr.Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "");
            return origStr;
        }

        /// <summary>
        /// 替换中文为空字符串
        /// </summary>
        /// <param name="origStr"></param>
        /// <returns></returns>
        public static string TrimZhCn(string origStr)
        {
            Regex reg = new Regex(@"[\u4e00-\u9fa5]");
            origStr = reg.Replace(origStr, string.Empty);
            return origStr;
        }
    }
}