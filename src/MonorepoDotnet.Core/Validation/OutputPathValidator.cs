using System.Text.RegularExpressions;

namespace MonorepoDotnet.Core.Validation;

/// <summary>
/// Validates the directory the user enters as the solution location. Bad paths
/// are rejected at the prompt, before any environment probe or file is written.
/// </summary>
public static partial class OutputPathValidator
{
    /// <summary>
    /// Long path segments plus the generated project tree must stay manageable.
    /// The generated solution adds at least <c>src/{SolutionName}/</c> and the
    /// project/source file names below the selected output directory.
    /// </summary>
    public const int MaxPathLength = 260;

    private static readonly string[] ReservedWindowsNames =
    [
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
    ];

    [GeneratedRegex(@"^COM[1-9]$|^LPT[1-9]$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex WindowsDevicePattern();

    /// <summary>Validates the output directory on its own.</summary>
    public static IReadOnlyList<string> Validate(string? outputDirectory)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            errors.Add("Output directory is required.");
            return errors;
        }

        var value = outputDirectory.Trim();

        if (value.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            errors.Add("Output directory contains characters that are not valid in a path.");
            return errors;
        }

        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(value);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            errors.Add($"Output directory is not a valid path: {ex.Message}");
            return errors;
        }

        if (fullPath.Length > MaxPathLength)
        {
            errors.Add(
                $"Output directory is too long ({fullPath.Length} characters). " +
                $"Use a path of at most {MaxPathLength} characters.");
        }

        if (File.Exists(fullPath))
        {
            errors.Add($"Output directory '{fullPath}' points to a file, not a directory.");
            return errors;
        }

        var root = Path.GetPathRoot(fullPath);
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
        {
            errors.Add($"The root of the output directory ('{root}') does not exist or is not accessible.");
        }

        if (HasExistingFileAncestor(fullPath))
        {
            errors.Add($"Output directory '{fullPath}' runs through a path that is an existing file.");
        }

        if (OperatingSystem.IsWindows())
        {
            var reserved = FindReservedWindowsName(fullPath);
            if (reserved is not null)
            {
                errors.Add(
                    $"Output directory uses the reserved Windows name '{reserved}'. Choose another directory.");
            }
        }

        var invalidNameCharacter = FindInvalidFileNameCharacter(fullPath);
        if (invalidNameCharacter is not null)
        {
            errors.Add(
                $"Output directory contains the invalid character '{invalidNameCharacter}' in a folder name.");
        }

        return errors;
    }

    /// <summary>
    /// Validates the complete solution path: output directory + solution name,
    /// including the non-empty target directory guard.
    /// </summary>
    public static IReadOnlyList<string> ValidateTarget(string? outputDirectory, string? solutionName)
    {
        var errors = new List<string>(Validate(outputDirectory));

        if (errors.Count > 0 || string.IsNullOrWhiteSpace(solutionName))
        {
            return errors;
        }

        var nameErrors = SolutionNameValidator.Validate(solutionName);
        if (nameErrors.Count > 0)
        {
            errors.AddRange(nameErrors);
            return errors;
        }

        var targetPath = Path.Combine(Path.GetFullPath(outputDirectory!.Trim()), solutionName.Trim());

        if (targetPath.Length > MaxPathLength)
        {
            errors.Add(
                $"The solution path is too long ({targetPath.Length} characters). " +
                $"Use a shorter output directory or solution name (maximum {MaxPathLength}).");
        }

        if (File.Exists(targetPath))
        {
            errors.Add($"The solution path '{targetPath}' is a file. Choose another output directory or name.");
            return errors;
        }

        try
        {
            if (Directory.Exists(targetPath) && Directory.EnumerateFileSystemEntries(targetPath).Any())
            {
                errors.Add(
                    $"The solution path '{targetPath}' already exists and is not empty. " +
                    "Choose another output directory or solution name.");
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            errors.Add($"The solution path '{targetPath}' cannot be inspected: {ex.Message}");
        }

        return errors;
    }

    /// <summary>Returns the absolute, normalized path, or the input when it cannot be resolved.</summary>
    public static string Normalize(string outputDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);

        try
        {
            return Path.GetFullPath(outputDirectory.Trim());
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return outputDirectory.Trim();
        }
    }

    private static bool HasExistingFileAncestor(string fullPath)
    {
        var current = fullPath;

        while (!string.IsNullOrEmpty(current))
        {
            if (File.Exists(current))
            {
                return true;
            }

            current = Path.GetDirectoryName(current);
        }

        return false;
    }

    private static string? FindReservedWindowsName(string fullPath)
    {
        var segments = fullPath.Split(
            [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
            StringSplitOptions.RemoveEmptyEntries);

        foreach (var segment in segments)
        {
            // Ignore drive roots such as "C:".
            if (segment.EndsWith(':'))
            {
                continue;
            }

            var name = segment.Split('.')[0].TrimEnd(' ', '.').ToUpperInvariant();

            if (ReservedWindowsNames.Contains(name) || WindowsDevicePattern().IsMatch(name))
            {
                return segment;
            }
        }

        return null;
    }

    private static char? FindInvalidFileNameCharacter(string fullPath)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var segments = fullPath.Split(
            [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
            StringSplitOptions.RemoveEmptyEntries);

        foreach (var segment in segments)
        {
            // Ignore drive roots such as "C:".
            if (segment.EndsWith(':'))
            {
                continue;
            }

            var index = segment.IndexOfAny(invalidChars);
            if (index >= 0)
            {
                return segment[index];
            }
        }

        return null;
    }
}
