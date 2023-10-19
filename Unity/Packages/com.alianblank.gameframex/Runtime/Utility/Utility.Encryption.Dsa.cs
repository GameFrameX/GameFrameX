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
            public class Dsa
            {
                private readonly DSACryptoServiceProvider _dsa = null;

                public Dsa(DSACryptoServiceProvider dsa)
                {
                    _dsa = dsa;
                }

                public Dsa(string key)
                {
                    DSACryptoServiceProvider rsa = new DSACryptoServiceProvider();
                    rsa.FromXmlString(key);
                    this._dsa = rsa;
                }

                public static Dictionary<string, string> Make()
                {
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    DSACryptoServiceProvider dsa = new DSACryptoServiceProvider();
                    dic["privatekey"] = dsa.ToXmlString(true);
                    dic["publickey"] = dsa.ToXmlString(false);
                    return dic;
                }

                /// <summary>
                /// 签名
                /// </summary>
                /// <param name="dataToSign"></param>
                /// <param name="privateKey"></param>
                /// <returns></returns>
                public static byte[] DSASignData(byte[] dataToSign, string privateKey)
                {
                    try
                    {
                        DSACryptoServiceProvider dsa = new DSACryptoServiceProvider();
                        dsa.FromXmlString(privateKey);
                        return dsa.SignData(dataToSign);
                    }
                    catch
                    {
                        return null;
                    }
                }

                public static string DSASignData(string dataToSign, string privateKey)
                {
                    byte[] res = DSASignData(Encoding.UTF8.GetBytes(dataToSign), privateKey);
                    return Convert.ToBase64String(res);
                }


                /// <summary>
                /// 签名
                /// </summary>
                /// <param name="dataToSign"></param>
                /// <returns></returns>
                public byte[] SignData(byte[] dataToSign)
                {
                    try
                    {
                        return _dsa.SignData(dataToSign);
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
                /// <param name="dataToVerify"></param>
                /// <param name="signedData"></param>
                /// <param name="privateKey"></param>
                /// <returns></returns>
                public static bool DSAVerifyData(byte[] dataToVerify, byte[] signedData, string privateKey)
                {
                    try
                    {
                        DSACryptoServiceProvider dsa = new DSACryptoServiceProvider();
                        dsa.FromXmlString(privateKey);
                        return dsa.VerifyData(dataToVerify, signedData);
                    }
                    catch
                    {
                        return false;
                    }
                }

                public static bool DSAVerifyData(string dataToVerify, string signedData, string privateKey)
                {
                    return DSAVerifyData(Encoding.UTF8.GetBytes(dataToVerify), Convert.FromBase64String(signedData),
                        privateKey);
                }

                public bool VerifyData(byte[] dataToVerify, byte[] signedData)
                {
                    try
                    {
                        return _dsa.VerifyData(dataToVerify, signedData);
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