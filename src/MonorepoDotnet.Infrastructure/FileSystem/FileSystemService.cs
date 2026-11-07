using MonorepoDotnet.Core.Abstractions;

namespace MonorepoDotnet.Infrastructure.FileSystem;

/// <summary>Default <see cref="IFileSystem"/> implementation over System.IO.</summary>
public sealed class FileSystemService : IFileSystem
{
    public bool DirectoryExists(string path) => Directory.Exists(path);

    public bool FileExists(string path) => File.Exists(path);

    public void CreateDirectory(string path) => Directory.CreateDirectory(path);

    public bool IsDirectoryEmpty(string path)
    {
        if (!Directory.Exists(path))
        {
            return true;
        }

        return !Directory.EnumerateFileSystemEntries(path).Any();
    }

    public async Task WriteAllTextAsync(
        string path,
        string content,
        CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(path, content, cancellationToken).ConfigureAwait(false);
    }

    public Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default) =>
        File.ReadAllTextAsync(path, cancellationToken);

    public void DeleteFile(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public void DeleteDirectory(string path, bool recursive)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive);
        }
    }

    public IEnumerable<string> EnumerateFiles(
        string path,
        string searchPattern,
        SearchOption searchOption) =>
        Directory.EnumerateFiles(path, searchPattern, searchOption);
}
