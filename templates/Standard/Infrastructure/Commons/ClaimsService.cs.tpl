using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using {{InfrastructureNamespace}}.Interfaces;
using {{InfrastructureNamespace}}.Utils;

namespace {{InfrastructureNamespace}}.Commons;

public class ClaimsService : IClaimsService
{
    public ClaimsService(IHttpContextAccessor httpContextAccessor)
    {
        var identity = httpContextAccessor.HttpContext?.User?.Identity as ClaimsIdentity;
        CurrentUserId = Guid.TryParse(AuthenTools.GetCurrentUserId(identity), out var id) ? id : Guid.Empty;
        IpAddress = httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
    }

    public Guid CurrentUserId { get; }

    public string? IpAddress { get; }
}
