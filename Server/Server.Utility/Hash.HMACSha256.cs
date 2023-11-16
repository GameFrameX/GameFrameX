using System.Security.Cryptography;
using System.Text;

namespace Server.Utility
{
    /// <summary>
    /// 哈希计算相关的实用函数。
    /// </summary>
    public static partial class Hash
    {
        /// <summary>
        /// HMACSha256
        /// </summary>
        public static class HMACSha256
        {
            /// <summary>
            /// 使用提供的密钥对指定消息进行HMACSHA256哈希计算。
            /// </summary>
            /// <param name="message">要进行哈希计算的消息。</param>
            /// <param name="key">用于哈希计算的密钥。</param>
            /// <returns>Base64编码的哈希值。</returns>
            public static string Hash(string message, string key)
            {
                byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);

                using (var hmac = new HMACSHA256(keyBytes))
                {
                    byte[] hashBytes = hmac.ComputeHash(messageBytes);
                    return Convert.ToBase64String(hashBytes);
                }
            }
        }
    }
}