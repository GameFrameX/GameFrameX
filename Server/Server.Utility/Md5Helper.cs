using System.Security.Cryptography;

namespace Server.Utility
{
    public static class Md5Helper
    {
        private static readonly ThreadLocal<MD5> _md5Cache = new(() => MD5.Create());

        public static MD5 MD5Current => _md5Cache.Value;

        public static string Md5(byte[] inputBytes)
        {
            var hashBytes = MD5Current.ComputeHash(inputBytes);
            var hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);
            return hash;
        }
    }
}