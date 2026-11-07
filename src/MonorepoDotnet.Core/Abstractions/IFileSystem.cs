namespace MonorepoDotnet.Core.Abstractions;

/// <summary>File system abstraction used by generator use cases.</summary>
public interface IFileSystem
{
    bool DirectoryExists(string path);

    bool FileExists(string path);

    void CreateDirectory(string path);

    bool IsDirectoryEmpty(string path);

    Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default);

    Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default);

    void DeleteFile(string path);

    void DeleteDirectory(string path, bool recursive);

    IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption);
}
