using MonorepoDotnet.Core.Abstractions;
using MonorepoDotnet.Core.Models;

namespace MonorepoDotnet.Tests;

/// <summary>In-memory double for <see cref="IFileSystem"/> used by environment tests.</summary>
public sealed class FakeFileSystem : IFileSystem
{
    private readonly HashSet<string> _directories = new(StringComparer.Ordinal);
    private readonly HashSet<string> _files = new(StringComparer.Ordinal);

    public bool ThrowOnWrite { get; set; }
    public string? ThrowOnWriteMessage { get; set; }

    public bool DirectoryExists(string path) => _directories.Contains(path);
    public bool FileExists(string path) => _files.Contains(path);
    public void CreateDirectory(string path) => _directories.Add(path);

    public bool IsDirectoryEmpty(string path)
    {
        var prefix = path.EndsWith(Path.DirectorySeparatorChar)
            ? path
            : path + Path.DirectorySeparatorChar;
        return !_files.Any(f => f.StartsWith(prefix, StringComparison.Ordinal));
    }

    public Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default)
    {
        if (ThrowOnWrite)
        {
            throw new IOException(ThrowOnWriteMessage ?? "write denied");
        }

        _files.Add(path);
        return Task.CompletedTask;
    }

    public Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default) =>
        Task.FromResult(string.Empty);

    public void DeleteFile(string path) => _files.Remove(path);
    public void DeleteDirectory(string path, bool recursive) => _directories.Remove(path);
    public IEnumerable<string> EnumerateFiles(
        string path,
        string searchPattern,
        SearchOption searchOption) =>
        _files.Where(f => f.StartsWith(path, StringComparison.Ordinal));
}

/// <summary>Configurable double for <see cref="IProcessRunner"/>.</summary>
public sealed class FakeProcessRunner : IProcessRunner
{
    public readonly List<(string FileName, IReadOnlyList<string> Arguments, string? WorkingDirectory)> Calls = [];

    private readonly Dictionary<string, ProcessResult> _results = new(StringComparer.OrdinalIgnoreCase);
    public ProcessResult DefaultResult { get; set; } = new(0, string.Empty, string.Empty);

    public void SetResult(string fileName, ProcessResult result) => _results[NormalizeKey(fileName)] = result;

    public Task<ProcessResult> RunAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        string? workingDirectory = null,
        CancellationToken cancellationToken = default)
    {
        Calls.Add((fileName, arguments, workingDirectory));
        if (_results.TryGetValue(NormalizeKey(fileName), out var result))
        {
            return Task.FromResult(result);
        }

        return Task.FromResult(DefaultResult);
    }

    private static string NormalizeKey(string fileName) =>
        Path.GetFileName(fileName).Replace(".exe", string.Empty, StringComparison.OrdinalIgnoreCase).ToLowerInvariant();
}
