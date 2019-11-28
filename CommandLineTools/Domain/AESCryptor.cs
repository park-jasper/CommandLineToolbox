using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CommandLineTools.Domain
{
    public class AESCryptor
    {
        private readonly SymmetricAlgorithm _internalCryptor;
        private readonly HashAlgorithm _hashAlgorithm;
        private readonly RNGCryptoServiceProvider _randomSource;
        public const int BitsPerByte = 8;
        public const int BlockSize = 128 / BitsPerByte;
        public AESCryptor()
        {
            _internalCryptor = Aes.Create();
            _hashAlgorithm = SHA256.Create();
            _randomSource = new RNGCryptoServiceProvider();
        }

        public byte[] Encrypt(string plaintext, string password)
        {
            _internalCryptor.GenerateIV();
            var encryptor = _internalCryptor.CreateEncryptor(_hashAlgorithm.ComputeHash(ToByteArray(password)), _internalCryptor.IV);
            var ptArray = ToByteArray(plaintext, BlockSize);
            var ptArrayWithPadding = new byte[ptArray.Length + BlockSize];
            _randomSource.GetBytes(ptArrayWithPadding, 0, BlockSize);
            Array.Copy(ptArray, 0, ptArrayWithPadding, BlockSize, ptArray.Length);

            var ctArray = encryptor.TransformFinalBlock(ptArrayWithPadding, 0, ptArrayWithPadding.Length);
            return ctArray;
        }

        public string Decrypt(byte[] ciphertext, string password)
        {
            var decryptor = _internalCryptor.CreateDecryptor(_hashAlgorithm.ComputeHash(ToByteArray(password)), _internalCryptor.IV);

            var ptArray = decryptor.TransformFinalBlock(ciphertext, 0, ciphertext.Length);
            return ToString(ptArray, BlockSize, ptArray.Length - BlockSize);
        }

        public byte[] ToByteArray(string text, int blockSize = 1)
        {
            if (text.Length % blockSize != 0)
            {
                var times = text.Length / blockSize;
                var totalSize = (times + 1) * blockSize;
                var sb = new StringBuilder(totalSize);
                sb.Append(text);
                sb.Append(' ', totalSize - text.Length);
                text = sb.ToString();
            }
            return Encoding.UTF8.GetBytes(text);
        }

        public string ToString(byte[] array, int offset, int count)
        {
            return Encoding.UTF8.GetString(array, offset, count);
        }
    }
}