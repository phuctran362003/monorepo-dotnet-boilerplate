namespace MonorepoDotnet.Core.Abstractions;

/// <summary>
/// Looks up the published global tool and reports a newer stable version.
/// A feed failure returns null so the CLI can continue offline.
/// </summary>
public interface IToolUpdateChecker
{
    Task<string?> FindNewerVersionAsync(string currentVersion, CancellationToken cancellationToken = default);
}
