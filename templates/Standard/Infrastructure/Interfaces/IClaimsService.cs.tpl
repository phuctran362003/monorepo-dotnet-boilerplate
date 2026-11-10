using System.Security.Claims;

namespace {{InfrastructureNamespace}}.Interfaces;

public interface IClaimsService
{
    Guid CurrentUserId { get; }

    string? IpAddress { get; }
}
