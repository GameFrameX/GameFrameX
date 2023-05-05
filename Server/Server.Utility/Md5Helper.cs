using System.Security.Cryptography;

namespace Server.Utility
{
    public static class Md5Helper
    {
        private static readonly ThreadLocal<MD5> Md5Cache = new(MD5.Create);

        public static MD5 Md5Current => Md5Cache.Value;

        public static string Md5(byte[] inputBytes)
        {
            var hashBytes = Md5Current.ComputeHash(inputBytes);
            var hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);
            return hash;
        }
    }
}