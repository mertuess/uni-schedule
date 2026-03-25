using System.Security.Cryptography;
using System.Text;

namespace UniSchedule{
  static class Crypto{
    private static readonly byte[] Salt = new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 };

    static public string MD5HashCreate(string input){
      MD5 MD5Hash = MD5.Create();
      byte[] inputBytes = Encoding.ASCII.GetBytes(input);
      byte[] hash = MD5Hash.ComputeHash(inputBytes);
      return Convert.ToHexString(hash); 
    }

    static public string Encrypt(string data){
      return "";
    }

    static public string Decrypt(string enctryptedData){
      return "";
    }
  }
}
