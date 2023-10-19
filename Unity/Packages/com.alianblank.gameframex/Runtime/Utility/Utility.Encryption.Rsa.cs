using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace GameFrameX
{
    public static partial class Utility
    {
        /// <summary>
        /// 加密解密相关的实用函数。
        /// </summary>
        public static partial class Encryption
        {
            public class Rsa
            {
                private readonly RSACryptoServiceProvider _rsa = null;

                public Rsa(RSACryptoServiceProvider rsa)
                {
                    _rsa = rsa;
                }

                public Rsa(string key)
                {
                    RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                    rsa.FromXmlString(key);
                    this._rsa = rsa;
                }

                public static Dictionary<string, string> Make()
                {
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    RSACryptoServiceProvider dsa = new RSACryptoServiceProvider();
                    dic["privateKey"] = dsa.ToXmlString(true);
                    dic["publicKey"] = dsa.ToXmlString(false);
                    return dic;
                }

                /// <summary>
                /// 加密
                /// </summary>
                /// <param name="publicKey">公钥</param>
                /// <param name="content">所加密的内容</param>
                /// <returns>加密后的内容</returns>
                public static string RSAEncrypt(string publicKey, string content)
                {
                    byte[] res = RSAEncrypt(publicKey, Encoding.UTF8.GetBytes(content));
                    return Convert.ToBase64String(res);
                }

                public string Encrypt(string content)
                {
                    byte[] res = Encrypt(Encoding.UTF8.GetBytes(content));
                    return Convert.ToBase64String(res);
                }

                public static byte[] RSAEncrypt(string publicKey, byte[] content)
                {
                    RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                    rsa.FromXmlString(publicKey);
                    byte[] cipherBytes = rsa.Encrypt(content, false);
                    return cipherBytes;
                }

                public byte[] Encrypt(byte[] content)
                {
                    byte[] cipherBytes = _rsa.Encrypt(content, false);
                    return cipherBytes;
                }

                /// <summary>
                /// 解密
                /// </summary>
                /// <param name="privateKey">私钥</param>
                /// <param name="content">加密后的内容</param>
                /// <returns>解密后的内容</returns>
                public static string RSADecrypt(string privateKey, string content)
                {
                    byte[] res = RSADecrypt(privateKey, Convert.FromBase64String(content));
                    return Encoding.UTF8.GetString(res);
                }


                public static byte[] RSADecrypt(string privateKey, byte[] content)
                {
                    RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                    rsa.FromXmlString(privateKey);
                    byte[] cipherBytes = rsa.Decrypt(content, false);
                    return cipherBytes;
                }

                public string Decrypt(string content)
                {
                    byte[] res = Decrypt(Convert.FromBase64String(content));
                    return Encoding.UTF8.GetString(res);
                }

                public byte[] Decrypt(byte[] content)
                {
                    byte[] bytes = _rsa.Decrypt(content, false);
                    return bytes;
                }

                /// <summary>
                /// 签名
                /// </summary>
                public static byte[] RSASignData(byte[] dataToSign, string privateKey)
                {
                    try
                    {
                        RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                        rsa.FromXmlString(privateKey);
                        return rsa.SignData(dataToSign, new SHA1CryptoServiceProvider());
                    }
                    catch
                    {
                        return null;
                    }
                }

                public static string RSASignData(string dataToSign, string privateKey)
                {
                    byte[] res = RSASignData(Encoding.UTF8.GetBytes(dataToSign), privateKey);
                    return Convert.ToBase64String(res);
                }

                public byte[] SignData(byte[] dataToSign)
                {
                    try
                    {
                        return _rsa.SignData(dataToSign, new SHA1CryptoServiceProvider());
                    }
                    catch
                    {
                        return null;
                    }
                }

                public string SignData(string dataToSign)
                {
                    byte[] res = SignData(Encoding.UTF8.GetBytes(dataToSign));
                    return Convert.ToBase64String(res);
                }

                /// <summary>
                /// 验证签名
                /// </summary>
                public static bool RSAVerifyData(byte[] dataToVerify, byte[] signedData, string publicKey)
                {
                    try
                    {
                        RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                        rsa.FromXmlString(publicKey);
                        return rsa.VerifyData(dataToVerify, new SHA1CryptoServiceProvider(), signedData);
                    }
                    catch
                    {
                        return false;
                    }
                }

                public static bool RSAVerifyData(string dataToVerify, string signedData, string publicKey)
                {
                    return RSAVerifyData(Encoding.UTF8.GetBytes(dataToVerify), Convert.FromBase64String(signedData),
                        publicKey);
                }

                public bool VerifyData(byte[] dataToVerify, byte[] signedData)
                {
                    try
                    {
                        return _rsa.VerifyData(dataToVerify, new SHA1CryptoServiceProvider(), signedData);
                    }
                    catch
                    {
                        return false;
                    }
                }

                public bool VerifyData(string dataToVerify, string signedData)
                {
                    try
                    {
                        return VerifyData(Encoding.UTF8.GetBytes(dataToVerify), Convert.FromBase64String(signedData));
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
        }
    }
}