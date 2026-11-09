using MonorepoDotnet.Core.Models;

namespace MonorepoDotnet.Core.Generation;

/// <summary>
/// A template file to render into the generated solution.
/// </summary>
/// <param name="TemplatePath">Path relative to the templates root.</param>
/// <param name="OutputPath">Destination path relative to the solution root. Tokens are allowed.</param>
/// <param name="RequiredPackage">Only generate when this optional package is selected.</param>
/// <param name="RequiresDocker">Only generate when Docker support is enabled.</param>
/// <param name="Host">Only generate for this application kind. Null means every kind.</param>
public sealed record TemplateFile(
    string TemplatePath,
    string OutputPath,
    OptionalPackage RequiredPackage = OptionalPackage.None,
    bool RequiresDocker = false,
    ProjectKind? Host = null)
{
    public bool Matches(GeneratorOptions options) =>
        (RequiredPackage == OptionalPackage.None || options.HasPackage(RequiredPackage)) &&
        (!RequiresDocker || options.IncludeDocker) &&
        (Host is null || Host == options.ProjectKind);
}
