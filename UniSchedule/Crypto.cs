// ┌────────────────────────────────────────────────────────────────────────────┐
// │ UniSchedule                                                                │
// │ WebAPI расширение для отслеживания расписания учебных занятий              │
// ├────────────────────────────────────────────────────────────────────────────┤
// │ Файл: Crypto.cs                                                            │
// │ Описание: Класс криптографии для повышения безопасности использования api  │
// └────────────────────────────────────────────────────────────────────────────┘

using System.Security.Cryptography;
using System.Text;

namespace UniSchedule;

/// <summary>
///     Класс криптографии
/// </summary>
internal static class Crypto
{
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("0123456789ABCDEF0123456789ABCDEF");
    private static readonly byte[] IV = Encoding.UTF8.GetBytes("0123456789ABCDEF");

    private static readonly string
        allowed_chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890%$#@&";

    /// <summary>
    ///     Метод хеширующий строку алгоритмом MD5
    /// </summary>
    /// <param name="input">Строка для хеширования</param>
    /// <returns>Захешированную строку</returns>
    public static string MD5HashCreate(string input)
    {
        var MD5Hash = MD5.Create();
        var inputBytes = Encoding.ASCII.GetBytes(input);
        var hash = MD5Hash.ComputeHash(inputBytes);
        return Convert.ToHexString(hash);
    }

    /// <summary>
    ///     Шифрование строки
    /// </summary>
    /// <param name="plainText">Исходный текст</param>
    /// <returns>Зашифрованный массив byte</returns>
    public static byte[] Encrypt(string plainText)
    {
        if (plainText == null || plainText.Length <= 0)
            return new byte[0];
        byte[] encrypted;

        using (var aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (var msEncrypt = new MemoryStream())
            {
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                }

                encrypted = msEncrypt.ToArray();
            }
        }

        return encrypted;
    }

    /// <summary>
    ///     Дешифрование строки
    /// </summary>
    /// <param name="cipherText">Зашифрованный массив byte</param>
    /// <returns>Расшифрованный текст</returns>
    public static string Decrypt(byte[] cipherText)
    {
        if (cipherText == null || cipherText.Length <= 0)
            return string.Empty;

        var plaintext = string.Empty;

        using (var aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (var msDecrypt = new MemoryStream(cipherText))
            {
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
        }

        return plaintext;
    }

    /// <summary>
    ///     Метод генерирующий пароль заданной длины
    /// </summary>
    /// <param name="lenght">Длина пароля</param>
    /// <returns>Сгенерированный пароль</returns>
    public static string GeneratePassword(int lenght)
    {
        var pass = "";
        var generator = new Random();
        for (var i = 0; i < lenght; i++) pass += allowed_chars[generator.Next(0, allowed_chars.Length)];
        return pass;
    }
}