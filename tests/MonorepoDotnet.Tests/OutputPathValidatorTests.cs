using MonorepoDotnet.Core.Validation;

namespace MonorepoDotnet.Tests;

public class OutputPathValidatorTests
{
    private static string TempRoot(string name) =>
        Path.Combine(Path.GetTempPath(), $"monorepo-outputpath-{Guid.NewGuid():N}-{name}");

    [Fact]
    public void Validate_NullOrEmpty_ReturnsError()
    {
        foreach (var value in new string?[] { null, "", "   " })
        {
            var errors = OutputPathValidator.Validate(value);
            Assert.NotEmpty(errors);
        }
    }

    [Fact]
    public void Validate_InvalidCharacters_ReturnsError()
    {
        var invalid = Path.GetInvalidPathChars().First(c => c != '\0');
        var errors = OutputPathValidator.Validate(Path.Combine(TempRoot("invalid"), $"bad{invalid}name"));
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("not valid in a path", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_PathIsAnExistingFile_ReturnsError()
    {
        var file = Path.Combine(TempRoot("file-as-dir"), "a.txt");
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        File.WriteAllText(file, "x");

        var errors = OutputPathValidator.Validate(file);
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("points to a file", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_TooLongPath_ReturnsError()
    {
        var errors = OutputPathValidator.Validate(new string('X', 300));
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("too long", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_ExistingDirectory_ReturnsNoErrors()
    {
        var dir = TempRoot("valid");
        Directory.CreateDirectory(dir);
        try
        {
            var errors = OutputPathValidator.Validate(dir);
            Assert.Empty(errors);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void ValidateTarget_NonEmptyTarget_ReturnsError()
    {
        var root = TempRoot("nonempty-target");
        var target = Path.Combine(root, "MyApp");
        Directory.CreateDirectory(target);
        File.WriteAllText(Path.Combine(target, "existing.txt"), "x");
        try
        {
            var errors = OutputPathValidator.ValidateTarget(root, "MyApp");
            Assert.NotEmpty(errors);
            Assert.Contains(errors, e => e.Contains("not empty", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ValidateTarget_AvailableTarget_ReturnsNoErrors()
    {
        var root = TempRoot("available-target");
        Directory.CreateDirectory(root);
        try
        {
            var errors = OutputPathValidator.ValidateTarget(root, "MyApp");
            Assert.Empty(errors);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ValidateTarget_InvalidName_ReturnsError()
    {
        var root = TempRoot("invalid-name-target");
        Directory.CreateDirectory(root);
        try
        {
            var errors = OutputPathValidator.ValidateTarget(root, "123Bad");
            Assert.NotEmpty(errors);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Normalize_ReturnsFullPath()
    {
        var dir = TempRoot("normalize");
        Directory.CreateDirectory(dir);
        try
        {
            var normalized = OutputPathValidator.Normalize(dir);
            Assert.Equal(Path.GetFullPath(dir), normalized);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void Normalize_UnresolvablePath_ReturnsTrimmedInput()
    {
        var result = OutputPathValidator.Normalize("   C:\\   ");
        Assert.Equal("C:\\", result);
    }

    [Fact]
    public void ReservedWindowsName_IsRejected_OnWindows()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        // Tên thiết bị Windows dưới một phân đoạn thư mục bị cấm.
        var errors = OutputPathValidator.Validate(@"C:\CON");
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("reserved", StringComparison.OrdinalIgnoreCase));
    }
}
