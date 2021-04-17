using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

//
// Source: https://raw.githubusercontent.com/ADeltaX/ProtoBuildBot/5cce37197c44792f3401b63d876795b5bc2072a4/src/BuildChecker/Classes/Helpers/EsrpDecryptor.cs
// Released under the MIT License (as of 2020-11-04)
// And Updated with original author input on 2021-04-15
//

namespace WindowsUpdateLib
{
    public class EsrpDecryptor : IDisposable
    {
        private readonly EsrpDecryptionInformation esrp;
        private readonly Aes aes;
        private readonly byte[] key;

        public EsrpDecryptor(EsrpDecryptionInformation esrp)
        {
            this.esrp = esrp;

            key = new byte[32];
            Array.Copy(Convert.FromBase64String(esrp.KeyData), 0, key, 0, 32);

            aes = Aes.Create(esrp.AlgorithmName);
            aes.Mode = CipherMode.CBC;
            aes.Key = key;
            aes.Padding = PaddingMode.None;
        }

        public async Task DecryptBufferToStreamAsync(byte[] buffer, Stream to, int bufferLength, long previousSumBlockLength,
            bool isPadded, CancellationToken cancellationToken = default)
        {
            var offsetBytes = new byte[16];
            Array.Copy(BitConverter.GetBytes(previousSumBlockLength), offsetBytes, 8);

            using var ivCrypter = aes.CreateEncryptor(key, new byte[16]);
            var newIv = ivCrypter.TransformFinalBlock(offsetBytes, 0, 16);

            if (isPadded)
                aes.Padding = PaddingMode.PKCS7;

            using var dec = aes.CreateDecryptor(key, newIv);
            using var ms = new MemoryStream(buffer, 0, bufferLength);
            using var cs = new CryptoStream(ms, dec, CryptoStreamMode.Read);

#if NET5_0
            await cs.CopyToAsync(to, cancellationToken).ConfigureAwait(false);
#else
            await cs.CopyToAsync(to).ConfigureAwait(false);
#endif
        }

        public async Task DecryptStreamFullAsync(Stream encryptedFile, Stream decryptedFile, ulong encryptedSize,
            CancellationToken cancellationToken = default)
        {
            int readBytes;
            var buffer = new byte[esrp.EncryptionBufferSize];
#if NET5_0
            while ((readBytes = await encryptedFile.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false)) > 0)
#else
            while ((readBytes = await encryptedFile.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) > 0)
#endif
            {
                var needsPaddingMode = encryptedSize == (ulong)encryptedFile.Position;
                var previousSumBlockLength = encryptedFile.Position - readBytes;
                await DecryptBufferToStreamAsync(buffer, decryptedFile, readBytes, previousSumBlockLength, needsPaddingMode, cancellationToken).ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        public async Task DecryptFileAsync(string encryptedFilePath, string decryptedFilePath,
            CancellationToken cancellationToken = default)
        {
            using var encryptedFile = File.OpenRead(encryptedFilePath);
            using var decryptedFile = File.OpenWrite(decryptedFilePath);
            await DecryptStreamFullAsync(encryptedFile, decryptedFile, (ulong)encryptedFile.Length, cancellationToken).ConfigureAwait(false);
        }

        public void Dispose()
        {
            aes.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}