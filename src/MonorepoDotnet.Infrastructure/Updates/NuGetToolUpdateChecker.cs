using System.Net.Http.Json;
using System.Text.Json.Serialization;
using MonorepoDotnet.Core.Abstractions;
using MonorepoDotnet.Core.Distribution;

namespace MonorepoDotnet.Infrastructure.Updates;

/// <summary>
/// Reads the NuGet flat-container version list for <see cref="ToolPackage.Id"/>.
/// </summary>
public sealed class NuGetToolUpdateChecker : IToolUpdateChecker
{
    private static readonly Uri VersionIndexUri = new($"https://api.nuget.org/v3-flatcontainer/{ToolPackage.Id}/index.json");

    public async Task<string?> FindNewerVersionAsync(string currentVersion, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd(ToolPackage.Id);

            var index = await client.GetFromJsonAsync<NuGetVersionIndex>(VersionIndexUri, cancellationToken).ConfigureAwait(false);
            if (index?.Versions is null)
            {
                return null;
            }

            return NuGetVersionSet.NewerStable(currentVersion, index.Versions);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        {
            return null;
        }
    }

    private sealed class NuGetVersionIndex
    {
        [JsonPropertyName("versions")]
        public string[]? Versions { get; set; }
    }
}
