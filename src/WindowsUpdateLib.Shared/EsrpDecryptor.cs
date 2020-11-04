using System;
using System.IO;
using System.Security.Cryptography;

//
// Source: https://raw.githubusercontent.com/ADeltaX/ProtoBuildBot/5cce37197c44792f3401b63d876795b5bc2072a4/src/BuildChecker/Classes/Helpers/EsrpDecryptor.cs
// Released under the MIT License (as of 2020-11-04)
//

namespace WindowsUpdateLib
{
    public static class EsrpDecryptor
    {
        public static void Decrypt(string encryptedFile, string decryptedFile, byte[] key)
        {
            using (var fsc = new FileStream(encryptedFile, FileMode.Open, FileAccess.Read))
                using (var fsp = new FileStream(decryptedFile, FileMode.Create, FileAccess.ReadWrite))
                    DecryptStream(fsc, fsp, key);
        }

        private static void DecryptStream(FileStream encryptedFile, FileStream decryptedFile, byte[] key)
        {
            var aes = Aes.Create();
            var shrinkedKey = new byte[32];
            var ivBase = new byte[16];

            Array.Copy(key, 0, shrinkedKey, 0, 32);
            Array.Copy(key, 32, ivBase, 0, 16);

            aes.Mode = CipherMode.CBC;
            aes.Key = shrinkedKey;
            aes.Padding = PaddingMode.None;

            int numRead;
            var buffer = new byte[65536];

            var iv = new byte[16];
            Array.Copy(ivBase, iv, 16);

            while ((numRead = encryptedFile.Read(buffer, 0, buffer.Length)) > 0)
            {
                var offsetBytes = new byte[16];
                Array.Copy(BitConverter.GetBytes(encryptedFile.Position - numRead), offsetBytes, 8);

                var ivCrypter = aes.CreateEncryptor(shrinkedKey, new byte[16]);
                var newIv = ivCrypter.TransformFinalBlock(offsetBytes, 0, 16);

                var ms = new MemoryStream(buffer, 0, numRead);

                var dec = aes.CreateDecryptor(shrinkedKey, newIv);

                using (var cs = new CryptoStream(ms, dec, CryptoStreamMode.Read))
                    cs.CopyTo(decryptedFile);
            }
        }
    }
}