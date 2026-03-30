// ┌────────────────────────────────────────────────────────────────────────────┐
// │ UniSchedule                                                                │
// │ WebAPI расширение для отслеживания расписания учебных занятий              │
// ├────────────────────────────────────────────────────────────────────────────┤
// │ Файл: Crypto.cs                                                            │
// │ Описание: Класс криптографии для повышения безопасности использования api  │
// └────────────────────────────────────────────────────────────────────────────┘

// Подключения
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Основное пространство имен api
/// </summary>
namespace UniSchedule{
  /// <summary>
  /// Класс криптографии
  /// </summary>
  static class Crypto{
    /// <summary>
    /// Ключ для шифрования
    /// </summary>
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("0123456789ABCDEF0123456789ABCDEF");
    /// <summary>
    /// Вектор для шифрования
    /// </summary>
    private static readonly byte[] IV = Encoding.UTF8.GetBytes("0123456789ABCDEF");
    /// <summary>
    /// Возможные символы для генерации пароля
    /// </summary>
    private static readonly string allowed_chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890%$#@&";

    /// <summary>
    /// Метод хеширующий строку алгоритмом MD5
    /// </summary>
    /// <param name="input">Строка для хеширования</param>
    /// <returns>Захешированную строку</returns>
    public static string MD5HashCreate(string input){
      MD5 MD5Hash = MD5.Create();
      byte[] inputBytes = Encoding.ASCII.GetBytes(input);
      byte[] hash = MD5Hash.ComputeHash(inputBytes);
      return Convert.ToHexString(hash); 
    }

    /// <summary>
    /// Шифрование строки
    /// </summary>
    /// <param name="plainText">Исходный текст</param>
    /// <returns>Зашифрованный массив byte</returns>
    static public byte[] Encrypt(string plainText){
      if (plainText == null || plainText.Length <= 0)
        return new byte[0];
      byte[] encrypted;

      using (Aes aesAlg = Aes.Create()){
        aesAlg.Key = Key;
        aesAlg.IV = IV;

        ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        using (MemoryStream msEncrypt = new MemoryStream()){
          using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)){
            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt)){
              swEncrypt.Write(plainText);
            }
          }
          encrypted = msEncrypt.ToArray();
        }
      }

      return encrypted;
    }

    /// <summary>
    /// Дешифрование строки
    /// </summary>
    /// <param name="cipherText">Зашифрованный массив byte</param>
    /// <returns>Расшифрованный текст</returns>
    static public string Decrypt(byte[] cipherText)
    {
      if (cipherText == null || cipherText.Length <= 0)
          return string.Empty;

      string plaintext = string.Empty;

      using (Aes aesAlg = Aes.Create())
      {
        aesAlg.Key = Key;
        aesAlg.IV = IV;

        ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        using (MemoryStream msDecrypt = new MemoryStream(cipherText))
        {
          using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
          {
            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
            {
              plaintext = srDecrypt.ReadToEnd();
            }
          }
        }
      }

      return plaintext;
    }

    /// <summary>
    /// Метод генерирующий пароль заданной длины
    /// </summary>
    /// <param name="lenght">Длина пароля</param>
    /// <returns>Сгенерированный пароль</returns>
    public static string GeneratePassword(int lenght){
      string pass = "";
      var generator = new Random();
      for(int i = 0; i < lenght; i++){
        pass += (allowed_chars[generator.Next(0, allowed_chars.Length)]);
      }
      return pass;
    }
  }
}
