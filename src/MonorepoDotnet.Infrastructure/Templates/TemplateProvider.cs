using System.Reflection;
using MonorepoDotnet.Core.Abstractions;

namespace MonorepoDotnet.Infrastructure.Templates;

/// <summary>
/// Loads templates embedded in the Infrastructure assembly (so single-file
/// publishes always contain them) and falls back to the repository
/// <c>templates/</c> folder during development.
/// </summary>
public sealed class TemplateProvider : ITemplateProvider
{
    private const string ResourcePrefix = "templates/";
    private readonly Assembly _assembly;
    private readonly IReadOnlyDictionary<string, string> _resourceNames;

    public TemplateProvider()
    {
        _assembly = typeof(TemplateProvider).Assembly;
        _resourceNames = _assembly.GetManifestResourceNames()
            .ToDictionary(NormalizeResourceName, name => name, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<string> ReadTemplateAsync(
        string relativePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);

        var key = ResourcePrefix + relativePath.Replace('\\', '/').TrimStart('/');

        if (_resourceNames.TryGetValue(key, out var resourceName))
        {
            await using var stream = _assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Embedded template '{relativePath}' could not be opened.");
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        }

        var diskPath = ResolveDiskTemplate(relativePath);
        if (diskPath is not null)
        {
            return await File.ReadAllTextAsync(diskPath, cancellationToken).ConfigureAwait(false);
        }

        throw new FileNotFoundException(
            $"Template '{relativePath}' was not found as an embedded resource or on disk.", relativePath);
    }

    private static string NormalizeResourceName(string name) => name.Replace('\\', '/');

    private static string? ResolveDiskTemplate(string relativePath)
    {
        var candidates = new List<string>();

        var overrideRoot = System.Environment.GetEnvironmentVariable("MONOREPODOTNET_TEMPLATES");
        if (!string.IsNullOrWhiteSpace(overrideRoot))
        {
            candidates.Add(Path.Combine(overrideRoot, relativePath));
        }

        foreach (var start in new[] { AppContext.BaseDirectory, System.Environment.CurrentDirectory })
        {
            var directory = new DirectoryInfo(start);
            while (directory is not null)
            {
                candidates.Add(Path.Combine(directory.FullName, "templates", relativePath));
                directory = directory.Parent;
            }
        }

        return candidates.FirstOrDefault(File.Exists);
    }
}
