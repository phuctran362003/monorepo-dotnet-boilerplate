using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace {{SolutionName}}.Service.Utils;

public static class JwtUtils
{
    public static string GenerateJwtToken(
        Guid userId,
        string email,
        string role,
        IConfiguration configuration,
        TimeSpan validityPeriod)
    {
        var secret = configuration["JWT:SecretKey"] ?? string.Empty;
        if (secret.Length < 32)
        {
            throw new ArgumentException("JWT:SecretKey must be at least 32 characters.", nameof(configuration));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.Role, role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(validityPeriod),
            Issuer = configuration["JWT:Issuer"],
            Audience = configuration["JWT:Audience"],
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        });

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
