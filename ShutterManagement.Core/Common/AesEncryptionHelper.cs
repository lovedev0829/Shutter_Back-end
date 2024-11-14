using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.Common
{
    public static  class AesEncryptionHelper
    {
        private static byte[] Key;
        private static byte[] IV;

        private static void AesEncryption()
        {
            // Generate Key and IV from the provided key string
            using (var sha256 = SHA256.Create())
            {
                Key = sha256.ComputeHash(Encoding.UTF8.GetBytes("ShutterManagement1234%^"));
            }
            IV = new byte[16]; // AES requires a 16-byte IV
        }

        public static string Encrypt(string plainText)
        {
            AesEncryption();
            using (var aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var writer = new StreamWriter(cs))
                    {
                        writer.Write(plainText);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public static string Decrypt(string cipherText)
        {
            AesEncryption();
            using (var aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var reader = new StreamReader(cs))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
