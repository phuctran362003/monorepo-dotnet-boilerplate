using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace {{InfrastructureNamespace}}.Utils;

public static class GenerateJWTToken
{
    public static JwtSecurityToken CreateToken(List<Claim> authClaims, IConfiguration configuration, DateTime currentTime)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:SecretKey"] ?? string.Empty));
        int.TryParse(configuration["JWT:TokenValidityInMinutes"], out var minutes);
        return new JwtSecurityToken(
            configuration["JWT:ValidIssuer"],
            configuration["JWT:ValidAudience"],
            expires: currentTime.AddMinutes(minutes),
            claims: authClaims,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
    }
}
