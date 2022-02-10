using System.Security.Cryptography;
using System.Text;

namespace Common.Helpers;

public static class Hasher
{
    public static string CalculateHash(string payload)
    {
        var payloadBytes = Encoding.ASCII.GetBytes(payload);
        var hashBytes = SHA256.HashData(payloadBytes);
        return Convert.ToBase64String(hashBytes);
    }
}