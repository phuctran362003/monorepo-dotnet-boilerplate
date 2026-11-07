using MonorepoDotnet.Core.Models;

namespace MonorepoDotnet.Core.Abstractions;

/// <summary>
/// Reusable asynchronous process runner. Implementations must use
/// <see cref="System.Diagnostics.ProcessStartInfo.ArgumentList"/> rather than
/// shell command concatenation.
/// </summary>
public interface IProcessRunner
{
    Task<ProcessResult> RunAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        string? workingDirectory = null,
        CancellationToken cancellationToken = default);
}
