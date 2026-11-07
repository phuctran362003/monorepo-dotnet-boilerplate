using System.Text.RegularExpressions;

namespace MonorepoDotnet.Core.Validation;

/// <summary>
/// Validates solution names. The solution name becomes a project name, a root
/// namespace and a Docker image/service name, therefore it is restricted to
/// letters, digits and underscores, starting with a letter.
/// </summary>
public static partial class SolutionNameValidator
{
    public const int MaxLength = 64;

    [GeneratedRegex("^[A-Za-z][A-Za-z0-9_]*$", RegexOptions.CultureInvariant)]
    private static partial Regex NamePattern();

    public static IReadOnlyList<string> Validate(string? solutionName)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(solutionName))
        {
            errors.Add("Solution name is required.");
            return errors;
        }

        var value = solutionName.Trim();

        if (value.Length > MaxLength)
        {
            errors.Add($"Solution name must be at most {MaxLength} characters long.");
        }

        if (!NamePattern().IsMatch(value))
        {
            errors.Add("Solution name must start with a letter and contain only letters, digits and underscores.");
        }

        return errors;
    }

    public static bool IsValid(string? solutionName) => Validate(solutionName).Count == 0;

    public static string Normalize(string solutionName) => solutionName.Trim();
}
