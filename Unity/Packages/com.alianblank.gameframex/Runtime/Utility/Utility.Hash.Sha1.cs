using System;
using System.IO;
using System.Text;

namespace GameFrameX
{
    public static partial class Utility
    {
        /// <summary>
        /// 哈希计算相关的实用函数。
        /// </summary>
        public static partial class Hash
        {
            /// <summary>
            /// Sha1
            /// </summary>
            public static class Sha1
            {
                /// <summary>
                /// 使用UTF-8 编码计算Sha1
                /// </summary>
                /// <param name="content"></param>
                /// <returns></returns>
                public static string Hash(string content)
                {
                    return Hash(content, Encoding.UTF8);
                }

                /// <summary>
                /// 使用指定编码 计算Sha1
                /// </summary>
                /// <param name="content"></param>
                /// <param name="encode"></param>
                /// <returns></returns>
                public static string Hash(string content, Encoding encode)
                {
                    //创建SHA1对象
                    using (var sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider())
                    {
                        //将待加密字符串转为byte类型
                        var bytesIn = encode.GetBytes(content);
                        var bytesOut = sha1.ComputeHash(bytesIn);
                        var result = BitConverter.ToString(bytesOut); //将运算结果转为string类型
                        result = result.Replace("-", "").ToLower();
                        return result;
                    }
                }
            }
        }
    }
}