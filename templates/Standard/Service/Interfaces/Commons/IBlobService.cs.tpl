using {{SolutionName}}.Service.DTOs;
using Microsoft.AspNetCore.Http;

namespace {{SolutionName}}.Service.Interfaces.Commons;

public interface IBlobService
{
    Task<UploadFileResultDto> UploadFileAsync(
        IFormFile file,
        string folder = "uploads",
        CancellationToken cancellationToken = default,
        string? objectFileName = null);

    Task DeleteFileAsync(string key, CancellationToken cancellationToken = default);

    string GetPublicUrl(string key);

    Task<BlobDownloadResult> DownloadFileAsync(string key, CancellationToken cancellationToken = default);

    string GetKeyFromPublicUrl(string url);

    Task<List<BlobObjectDto>> ListImagesAsync(string folder, CancellationToken cancellationToken = default);
}
