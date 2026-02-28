using System;
using System.Security.Cryptography;
using System.Text;

namespace CefDotnetApp.AgentCore.Core
{
    /// <summary>
    /// AES-GCM encryption utility for in-memory secret protection.
    /// Key is generated at startup and never persisted.
    /// Encrypted format: "ENC:" + Base64(nonce[12] + ciphertext + tag[16])
    /// </summary>
    internal static class AesCrypto
    {
        private const string Prefix = "ENC:";
        private const int NonceSize = 12;
        private const int TagSize = 16;
        private static byte[]? s_key;

        /// <summary>
        /// Initializes with a random 256-bit key. Call once at startup.
        /// </summary>
        public static void Initialize()
        {
            s_key = new byte[32];
            RandomNumberGenerator.Fill(s_key);
        }

        public static bool IsInitialized => s_key != null;

        /// <summary>
        /// Encrypts a plaintext string. Returns "ENC:base64(...)" string.
        /// If not initialized or input is null/empty, returns input as-is.
        /// </summary>
        public static string Encrypt(string plaintext)
        {
            if (s_key == null || string.IsNullOrEmpty(plaintext))
                return plaintext;

            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            byte[] nonce = new byte[NonceSize];
            RandomNumberGenerator.Fill(nonce);
            byte[] ciphertext = new byte[plaintextBytes.Length];
            byte[] tag = new byte[TagSize];

            using (var aes = new AesGcm(s_key, TagSize))
            {
                aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);
            }

            // Layout: nonce + ciphertext + tag
            byte[] result = new byte[NonceSize + ciphertext.Length + TagSize];
            Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
            Buffer.BlockCopy(ciphertext, 0, result, NonceSize, ciphertext.Length);
            Buffer.BlockCopy(tag, 0, result, NonceSize + ciphertext.Length, TagSize);

            return Prefix + Convert.ToBase64String(result);
        }

        /// <summary>
        /// Decrypts an "ENC:base64(...)" string back to plaintext.
        /// If input does not start with "ENC:" or not initialized, returns input as-is.
        /// </summary>
        public static string Decrypt(string encrypted)
        {
            if (s_key == null || string.IsNullOrEmpty(encrypted) || !encrypted.StartsWith(Prefix))
                return encrypted;

            try
            {
                byte[] data = Convert.FromBase64String(encrypted.Substring(Prefix.Length));
                if (data.Length < NonceSize + TagSize)
                    return encrypted; // malformed, return as-is

                int ciphertextLen = data.Length - NonceSize - TagSize;
                byte[] nonce = new byte[NonceSize];
                byte[] ciphertext = new byte[ciphertextLen];
                byte[] tag = new byte[TagSize];

                Buffer.BlockCopy(data, 0, nonce, 0, NonceSize);
                Buffer.BlockCopy(data, NonceSize, ciphertext, 0, ciphertextLen);
                Buffer.BlockCopy(data, NonceSize + ciphertextLen, tag, 0, TagSize);

                byte[] plaintext = new byte[ciphertextLen];
                using (var aes = new AesGcm(s_key, TagSize))
                {
                    aes.Decrypt(nonce, ciphertext, tag, plaintext);
                }

                return Encoding.UTF8.GetString(plaintext);
            }
            catch
            {
                return encrypted; // decryption failed, return as-is
            }
        }
    }
}
