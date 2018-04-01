using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Security.Cryptography;

/*----------------------------------------------
 *  DES加密、解密类库，字符串加密结果使用BASE64编码返回，支持文件的加密和解密
 *  转载务必保留此信息
 * ---------------------------------------------
 */
namespace ConfigTable.Editor {
    public static class DES
    {
        //DES加密偏移量，必须是>=8位长的字符串
        //private static string _iv = "vlq;cGalVcm,";
        //DES加密的私钥，必须是8位长的字符串
        //private static string _key = "_fdUx%f0";

        /// <summary>
        /// 对字符串进行DES加密
        /// </summary>
        /// <param name="sourceString">待加密的字符串</param>
        /// <returns>加密后的BASE64编码的字符串</returns>
        public static string EncryptString(string key, string iv, string sourceString)
        {
            byte[] btKey = System.Text.Encoding.UTF8.GetBytes(key);
            byte[] btIV = System.Text.Encoding.UTF8.GetBytes(iv);
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] inData = System.Text.Encoding.UTF8.GetBytes(sourceString);
                try
                {
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(btKey, btIV), CryptoStreamMode.Write))
                    {
                        cs.Write(inData, 0, inData.Length);
                        cs.FlushFinalBlock();
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
                catch
                {
                    throw;
                }
            }
        }
        /// <summary>
        /// 对DES加密后的字符串进行解密
        /// </summary>
        /// <param name="encryptedString">待解密的字符串</param>
        /// <returns>解密后的字符串</returns>
        public static string DecryptString(string key, string iv, string encryptedString)
        {
            byte[] btKey = System.Text.Encoding.UTF8.GetBytes(key);
            byte[] btIV = System.Text.Encoding.UTF8.GetBytes(iv);
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] inData = Convert.FromBase64String(encryptedString);
                try
                {
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(btKey, btIV), CryptoStreamMode.Write))
                    {
                        cs.Write(inData, 0, inData.Length);
                        cs.FlushFinalBlock();
                    }
                    return System.Text.Encoding.UTF8.GetString(ms.ToArray());
                }
                catch
                {
                    throw;
                }
            }
        }


        /// <summary>
        /// 对文件内容进行DES加密
        /// </summary>
        /// <param name="sourceFile">待加密的文件绝对路径</param>
        /// <param name="destFile">加密后的文件保存的绝对路径</param>
        public static void EncryptFile(string key, string iv, string sourceFile)
        {
            EncryptFile(key, iv, sourceFile, sourceFile);
        }

        public static void EncryptFile(string key, string iv, string sourceFile, string destFile)
        {
            if (!File.Exists(sourceFile)) throw new FileNotFoundException("指定的文件路径不存在！", sourceFile);
            byte[] btKey = System.Text.Encoding.UTF8.GetBytes(key);
            byte[] btIV = System.Text.Encoding.UTF8.GetBytes(iv);
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] btFile = File.ReadAllBytes(sourceFile);
            using (FileStream fs = new FileStream(destFile, FileMode.Create, FileAccess.Write))
            {
                try
                {
                    using (CryptoStream cs = new CryptoStream(fs, des.CreateEncryptor(btKey, btIV), CryptoStreamMode.Write))
                    {
                        cs.Write(btFile, 0, btFile.Length);
                        cs.FlushFinalBlock();
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    fs.Close();
                }
            }
        }

        /// <summary>
        /// 对文件内容进行DES解密
        /// </summary>
        /// <param name="sourceFile">待解密的文件绝对路径</param>
        /// <param name="destFile">解密后的文件保存的绝对路径</param>
        /// 
        public static void DecryptFile(string key, string iv, string sourceFile)
        {
            DecryptFile(key, iv, sourceFile, sourceFile);
        }

        public static void DecryptFile(string key, string iv, string sourceFile, string destFile)
        {
            if (!File.Exists(sourceFile)) throw new FileNotFoundException("指定的文件路径不存在！", sourceFile);
            byte[] btKey = System.Text.Encoding.UTF8.GetBytes(key);
            byte[] btIV = System.Text.Encoding.UTF8.GetBytes(iv);
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] btFile = File.ReadAllBytes(sourceFile);
            using (FileStream fs = new FileStream(destFile, FileMode.Create, FileAccess.Write))
            {
                try
                {
                    using (CryptoStream cs = new CryptoStream(fs, des.CreateDecryptor(btKey, btIV), CryptoStreamMode.Write))
                    {
                        cs.Write(btFile, 0, btFile.Length);
                        cs.FlushFinalBlock();
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    fs.Close();
                }
            }
        }


        public static byte[] EncryptBytes(string key, string iv, byte[] sourceBytes)
        {
            byte[] btKey = System.Text.Encoding.UTF8.GetBytes(key);
            byte[] btIV = System.Text.Encoding.UTF8.GetBytes(iv);
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(btKey, btIV), CryptoStreamMode.Write))
                    {
                        cs.Write(sourceBytes, 0, sourceBytes.Length);
                        cs.FlushFinalBlock();
                    }
                    return ms.ToArray();
                }
                catch
                {
                    throw;
                }
            }
        }

        public static byte[] DecryptBytes(string key, string iv, byte[] sourceBytes)
        {
            byte[] btKey = System.Text.Encoding.UTF8.GetBytes(key);
            byte[] btIV = System.Text.Encoding.UTF8.GetBytes(iv);
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(btKey, btIV), CryptoStreamMode.Write))
                    {
                        cs.Write(sourceBytes, 0, sourceBytes.Length);
                        cs.FlushFinalBlock();
                    }
                    return ms.ToArray();
                }
                catch
                {
                    throw;
                }
            }
        }
    }
}