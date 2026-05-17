using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HastaneYonetimSistemi.Helpers
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    // Bu siniftaki kodlar bizim veritabanında verilerin saklanması konusunda önemlidir.
    public static class SecurityHelper
    {
        // BU ANAHTAR 32 KARAKTER OLMALI VE ASLA DEÄİÅMEMELİ!
        private static readonly string SecretKey = "9z$B&E)H@McQfTjWnZr4u7x!A%D*G-Ka";

        // 1. HASH OLUÅTURMA (Arama için)
        public static string Hashle(string tcKimlikNo)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(tcKimlikNo));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString(); // Örn: "a665a..."
            }
        }

        // 2. ÅİFRELEME (Kaydetme için)
        public static string Sifrele(string tcKimlikNo)
        {
            byte[] iv = new byte[16];
            using (var rng = RandomNumberGenerator.Create()) { rng.GetBytes(iv); }

            byte[] array;
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(SecretKey);
                aes.IV = iv;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream)) { streamWriter.Write(tcKimlikNo); }
                        array = memoryStream.ToArray();
                    }
                }
            }
            // IV ve Åifreli Veriyi birleştirip Base64 yapıyoruz
            var combinedIvAndCipher = new byte[iv.Length + array.Length];
            Array.Copy(iv, 0, combinedIvAndCipher, 0, iv.Length);
            Array.Copy(array, 0, combinedIvAndCipher, iv.Length, array.Length);
            return Convert.ToBase64String(combinedIvAndCipher);
        }

        // 3. ÅİFRE ÇÖZME (Okuma için)
        public static string Coz(string sifreliMetin)
        {
            byte[] fullCipher = Convert.FromBase64String(sifreliMetin);
            byte[] iv = new byte[16];
            byte[] cipher = new byte[fullCipher.Length - iv.Length];
            Array.Copy(fullCipher, iv, iv.Length);
            Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(SecretKey);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (MemoryStream memoryStream = new MemoryStream(cipher))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream)) { return streamReader.ReadToEnd(); }
                    }
                }
            }
        }
    }
}
