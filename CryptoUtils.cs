using System.Security.Cryptography;
using System.Text;

namespace Lunar.Core;

public static class CryptoUtils
{
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("LUNAR_ENGINE_SECURE_KEY_2026_!!!").Take(32).ToArray();
    private static readonly byte[] IV = Encoding.UTF8.GetBytes("LUNAR_INIT_VEC_X").Take(16).ToArray();

    public static string Encrypt(string plainText)
    {
        using Aes aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;
        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using MemoryStream ms = new();
        using CryptoStream cs = new(ms, encryptor, CryptoStreamMode.Write);
        using (StreamWriter sw = new(cs)) { sw.Write(plainText); }
        return Convert.ToBase64String(ms.ToArray());
    }

    public static string Decrypt(string cipherText)
    {
        using Aes aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;
        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using MemoryStream ms = new(Convert.FromBase64String(cipherText));
        using CryptoStream cs = new(ms, decryptor, CryptoStreamMode.Read);
        using StreamReader sr = new(cs);
        return sr.ReadToEnd();
    }
}
