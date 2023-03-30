using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace GameFramework
{
    public static partial class Utility
    {
        /// <summary>
        /// 加密解密相关的实用函数。
        /// </summary>
        public static partial class Encryption
        {
            public static class Aes
            {
                #region 加密

                #region 加密字符串

                /// <summary>
                /// AES 加密(高级加密标准，是下一代的加密算法标准，速度快，安全级别高，目前 AES 标准的一个实现是 Rijndael 算法)
                /// </summary>
                /// <param name="EncryptString">待加密密文</param>
                /// <param name="EncryptKey">加密密钥</param>
                public static string AESEncrypt(string EncryptString, string EncryptKey)
                {
                    return Convert.ToBase64String(AESEncrypt(Encoding.UTF8.GetBytes(EncryptString), EncryptKey));
                }

                #endregion

                #region 加密字节数组

                /// <summary>
                /// AES 加密(高级加密标准，是下一代的加密算法标准，速度快，安全级别高，目前 AES 标准的一个实现是 Rijndael 算法)
                /// </summary>
                /// <param name="EncryptString">待加密密文</param>
                /// <param name="EncryptKey">加密密钥</param>
                public static byte[] AESEncrypt(byte[] EncryptByte, string EncryptKey)
                {
                    if (EncryptByte.Length == 0)
                    {
                        throw (new Exception("明文不得为空"));
                    }

                    if (string.IsNullOrEmpty(EncryptKey))
                    {
                        throw (new Exception("密钥不得为空"));
                    }

                    byte[] m_strEncrypt;
                    byte[] m_btIV = new byte[16] {224, 131, 122, 101, 37, 254, 33, 17, 19, 28, 212, 130, 45, 65, 43, 32};
                    byte[] m_salt = new byte[16] {234, 231, 123, 100, 87, 254, 123, 17, 89, 18, 230, 13, 45, 65, 43, 32};
                    Rijndael m_AESProvider = Rijndael.Create();
                    try
                    {
                        MemoryStream m_stream = new MemoryStream();
                        PasswordDeriveBytes pdb = new PasswordDeriveBytes(EncryptKey, m_salt);
                        ICryptoTransform transform = m_AESProvider.CreateEncryptor(pdb.GetBytes(32), m_btIV);
                        CryptoStream m_csstream = new CryptoStream(m_stream, transform, CryptoStreamMode.Write);
                        m_csstream.Write(EncryptByte, 0, EncryptByte.Length);
                        m_csstream.FlushFinalBlock();
                        m_strEncrypt = m_stream.ToArray();
                        m_stream.Close();
                        m_stream.Dispose();
                        m_csstream.Close();
                        m_csstream.Dispose();
                    }
                    catch (IOException ex)
                    {
                        throw ex;
                    }
                    catch (CryptographicException ex)
                    {
                        throw ex;
                    }
                    catch (ArgumentException ex)
                    {
                        throw ex;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        m_AESProvider.Clear();
                    }

                    return m_strEncrypt;
                }

                #endregion

                #endregion

                #region 解密

                #region 解密字符串

                /// <summary>
                /// AES 解密(高级加密标准，是下一代的加密算法标准，速度快，安全级别高，目前 AES 标准的一个实现是 Rijndael 算法)
                /// </summary>
                /// <param name="DecryptString">待解密密文</param>
                /// <param name="DecryptKey">解密密钥</param>
                public static string AESDecrypt(string DecryptString, string DecryptKey)
                {
                    return Encoding.UTF8.GetString((AESDecrypt(Convert.FromBase64String(DecryptString), DecryptKey)));
                }

                #endregion

                #region 解密字节数组

                /// <summary>
                /// AES 解密(高级加密标准，是下一代的加密算法标准，速度快，安全级别高，目前 AES 标准的一个实现是 Rijndael 算法)
                /// </summary>
                /// <param name="DecryptString">待解密密文</param>
                /// <param name="DecryptKey">解密密钥</param>
                public static byte[] AESDecrypt(byte[] DecryptByte, string DecryptKey)
                {
                    if (DecryptByte.Length == 0)
                    {
                        throw (new Exception("密文不得为空"));
                    }

                    if (string.IsNullOrEmpty(DecryptKey))
                    {
                        throw (new Exception("密钥不得为空"));
                    }

                    byte[] m_strDecrypt;
                    byte[] m_btIV = new byte[16] {224, 131, 122, 101, 37, 254, 33, 17, 19, 28, 212, 130, 45, 65, 43, 32};
                    byte[] m_salt = new byte[16] {234, 231, 123, 100, 87, 254, 123, 17, 89, 18, 230, 13, 45, 65, 43, 32};
                    Rijndael m_AESProvider = Rijndael.Create();
                    try
                    {
                        MemoryStream m_stream = new MemoryStream();
                        PasswordDeriveBytes pdb = new PasswordDeriveBytes(DecryptKey, m_salt);
                        ICryptoTransform transform = m_AESProvider.CreateDecryptor(pdb.GetBytes(32), m_btIV);
                        CryptoStream m_csstream = new CryptoStream(m_stream, transform, CryptoStreamMode.Write);
                        m_csstream.Write(DecryptByte, 0, DecryptByte.Length);
                        m_csstream.FlushFinalBlock();
                        m_strDecrypt = m_stream.ToArray();
                        m_stream.Close();
                        m_stream.Dispose();
                        m_csstream.Close();
                        m_csstream.Dispose();
                    }
                    catch (IOException ex)
                    {
                        throw ex;
                    }
                    catch (CryptographicException ex)
                    {
                        throw ex;
                    }
                    catch (ArgumentException ex)
                    {
                        throw ex;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        m_AESProvider.Clear();
                    }

                    return m_strDecrypt;
                }

                #endregion

                #endregion
            }
        }
    }
}