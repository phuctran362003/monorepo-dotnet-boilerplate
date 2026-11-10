using System.Security.Cryptography;
using System.Text;

namespace {{SolutionName}}.Service.Utils;

public class PasswordHasher
{
    public string? HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = HMACSHA256.HashData(salt, Encoding.UTF8.GetBytes(password));
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    public bool VerifyPassword(string password, string? storedHash)
    {
        if (string.IsNullOrWhiteSpace(storedHash))
        {
            return false;
        }

        var parts = storedHash.Split(':');
        if (parts.Length != 2)
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[0]);
        var expected = Convert.FromBase64String(parts[1]);
        var actual = HMACSHA256.HashData(salt, Encoding.UTF8.GetBytes(password));
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
