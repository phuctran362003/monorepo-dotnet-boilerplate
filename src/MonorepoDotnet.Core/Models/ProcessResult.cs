namespace MonorepoDotnet.Core.Models;

/// <summary>Result of an external process execution.</summary>
public sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError)
{
    public bool Succeeded => ExitCode == 0;

    public string CombinedOutput =>
        string.Join(Environment.NewLine,
            new[] { StandardOutput, StandardError }.Where(s => !string.IsNullOrWhiteSpace(s)));

    public string ToDiagnosticString(string command) =>
        $"Command '{command}' exited with code {ExitCode}.{Environment.NewLine}" +
        $"stdout: {StandardOutput}{Environment.NewLine}" +
        $"stderr: {StandardError}";
}
