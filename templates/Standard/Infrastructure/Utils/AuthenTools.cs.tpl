using System.Security.Claims;

namespace {{InfrastructureNamespace}}.Utils;

public static class AuthenTools
{
    public static string? GetCurrentUserId(ClaimsIdentity? identity) =>
        identity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}
