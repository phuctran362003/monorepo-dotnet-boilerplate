using System.Security.Cryptography;

namespace {{SolutionName}}.Service.Utils;

public static class OtpGenerator
{
    public static string GenerateNumeric(int length = 6)
    {
        var bytes = RandomNumberGenerator.GetBytes(length);
        var characters = new char[length];
        for (var index = 0; index < length; index++)
        {
            characters[index] = (char)('0' + (bytes[index] % 10));
        }

        return new string(characters);
    }

    public static string GenerateAlphanumeric(int length = 8)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var bytes = RandomNumberGenerator.GetBytes(length);
        var characters = new char[length];
        for (var index = 0; index < length; index++)
        {
            characters[index] = alphabet[bytes[index] % alphabet.Length];
        }

        return new string(characters);
    }

    public static OtpToken GenerateToken(int length = 6, TimeSpan? validFor = null)
    {
        var lifetime = validFor ?? TimeSpan.FromMinutes(5);
        var generatedAt = DateTime.UtcNow;
        return new OtpToken(GenerateNumeric(length), generatedAt, generatedAt.Add(lifetime));
    }
}

public class OtpToken
{
    public OtpToken(string code, DateTime generatedAtUtc, DateTime expiresAtUtc)
    {
        Code = code;
        GeneratedAtUtc = generatedAtUtc;
        ExpiresAtUtc = expiresAtUtc;
    }

    public string Code { get; }

    public DateTime GeneratedAtUtc { get; }

    public DateTime ExpiresAtUtc { get; }

    public bool IsExpired => DateTime.UtcNow > ExpiresAtUtc;
}
