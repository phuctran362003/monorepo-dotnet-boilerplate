using System.Security.Cryptography;
using System.Text;

namespace {{InfrastructureNamespace}}.Utils;

public class HashHelper
{
    public static string HmacSHA512(string key, string inputData)
    {
        var hash = HMACSHA512.HashData(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(inputData));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public static string HmacSHA256(string inputData, string key)
    {
        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(inputData));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
