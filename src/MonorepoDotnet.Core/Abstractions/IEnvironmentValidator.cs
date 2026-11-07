using MonorepoDotnet.Core.Models;

namespace MonorepoDotnet.Core.Abstractions;

/// <summary>Validates the host environment before generation starts.</summary>
public interface IEnvironmentValidator
{
    Task<EnvironmentReport> ValidateAsync(
        EnvironmentValidationRequest request,
        CancellationToken cancellationToken = default);
}
