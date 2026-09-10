using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace HRMS.Application.Common
{
    // تشفير AES ثابت (نفس المفتاح و IV دايمًا) عشان نقدر نستخدم = فى الداتابيز
    // على الرقم القومى المشفر (Deterministic Encryption)
    public class AesEncryptionService : IEncryptionService
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public AesEncryptionService(IConfiguration configuration)
        {
            _key = Convert.FromBase64String(configuration["Encryption:Key"]!);
            _iv = Convert.FromBase64String(configuration["Encryption:IV"]!);
        }

        public string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            return Convert.ToBase64String(cipherBytes);
        }

        public string Decrypt(string cipherText)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;

            using var decryptor = aes.CreateDecryptor();
            var cipherBytes = Convert.FromBase64String(cipherText);
            var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}
