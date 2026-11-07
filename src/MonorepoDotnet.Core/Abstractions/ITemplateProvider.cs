namespace MonorepoDotnet.Core.Abstractions;

/// <summary>Provides access to the embedded template files.</summary>
public interface ITemplateProvider
{
    /// <summary>
    /// Reads a template using a path relative to the templates root, for example
    /// <c>DDD/API/Program.cs.tpl</c>.
    /// </summary>
    Task<string> ReadTemplateAsync(string relativePath, CancellationToken cancellationToken = default);
}
