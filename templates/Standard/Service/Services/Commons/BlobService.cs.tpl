using Amazon.S3;
using Amazon.S3.Model;
using {{SolutionName}}.Service.DTOs;
using {{SolutionName}}.Service.Interfaces.Commons;
using {{SolutionName}}.Service.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace {{SolutionName}}.Service.Services.Commons;

public class ObjectStorageOptions
{
    public const string SectionName = "ObjectStorage";

    public string ServiceUrl { get; set; } = string.Empty;

    public string BucketName { get; set; } = string.Empty;

    public string PublicUrl { get; set; } = string.Empty;

    public string AccessKey { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;

    public string Region { get; set; } = "us-east-1";

    public long StorageQuotaBytes { get; set; } = 10L * 1024 * 1024 * 1024;
}

public class BlobService : IBlobService
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;
    private const int MaxImageEdgePixels = 8192;
    private const int WebpQuality = 82;

    private readonly IAmazonS3 _s3Client;
    private readonly ObjectStorageOptions _options;
    private readonly ILogger<BlobService> _logger;

    public BlobService(IAmazonS3 s3Client, IOptions<ObjectStorageOptions> options, ILogger<BlobService> logger)
    {
        _s3Client = s3Client;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<UploadFileResultDto> UploadFileAsync(
        IFormFile file,
        string folder = "uploads",
        CancellationToken cancellationToken = default,
        string? objectFileName = null)
    {
        if (file is null || file.Length == 0)
        {
            throw ErrorHelper.BadRequest(ErrorMessages.FileEmpty);
        }

        if (file.Length > MaxFileSizeBytes)
        {
            throw ErrorHelper.BadRequest(ErrorMessages.FileTooLarge);
        }

        await using var source = file.OpenReadStream();
        MemoryStream payload;
        string contentType;
        string extension;

        if (ImageFiles.IsImage(file))
        {
            payload = new MemoryStream();
            using var image = await Image.LoadAsync(source, cancellationToken);
            if (image.Width > MaxImageEdgePixels || image.Height > MaxImageEdgePixels)
            {
                image.Mutate(operation => operation.Resize(new ResizeOptions
                {
                    Size = new Size(MaxImageEdgePixels, MaxImageEdgePixels),
                    Mode = ResizeMode.Max
                }));
            }

            await image.SaveAsWebpAsync(payload, new WebpEncoder { Quality = WebpQuality }, cancellationToken);
            payload.Position = 0;
            contentType = "image/webp";
            extension = ".webp";
        }
        else
        {
            payload = new MemoryStream();
            await source.CopyToAsync(payload, cancellationToken);
            payload.Position = 0;
            contentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType;
            extension = Path.GetExtension(file.FileName);
        }

        var name = string.IsNullOrWhiteSpace(objectFileName) ? Guid.NewGuid().ToString("N") : objectFileName;
        var key = $"{folder.Trim('/')}/{name}{extension}";
        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            InputStream = payload,
            ContentType = contentType,
            DisablePayloadSigning = _options.ServiceUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
        };

        await _s3Client.PutObjectAsync(request, cancellationToken);
        _logger.LogInformation("Uploaded object {Key}.", key);

        return new UploadFileResultDto
        {
            Key = key,
            Url = GetPublicUrl(key),
            ContentType = contentType,
            Size = payload.Length
        };
    }

    public async Task DeleteFileAsync(string key, CancellationToken cancellationToken = default)
    {
        await _s3Client.DeleteObjectAsync(_options.BucketName, key, cancellationToken);
    }

    public string GetPublicUrl(string key)
    {
        var root = _options.PublicUrl.TrimEnd('/');
        return string.IsNullOrEmpty(root) ? key : $"{root}/{key.TrimStart('/')}";
    }

    public async Task<BlobDownloadResult> DownloadFileAsync(string key, CancellationToken cancellationToken = default)
    {
        var response = await _s3Client.GetObjectAsync(_options.BucketName, key, cancellationToken);
        return new BlobDownloadResult
        {
            Content = response.ResponseStream,
            ContentType = response.Headers.ContentType ?? "application/octet-stream",
            FileName = Path.GetFileName(key)
        };
    }

    public string GetKeyFromPublicUrl(string url)
    {
        var root = _options.PublicUrl.TrimEnd('/');
        if (!string.IsNullOrEmpty(root) && url.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            return url[(root.Length + 1)..];
        }

        return url;
    }

    public async Task<List<BlobObjectDto>> ListImagesAsync(string folder, CancellationToken cancellationToken = default)
    {
        var prefix = folder.Trim('/') + "/";
        var response = await _s3Client.ListObjectsV2Async(new ListObjectsV2Request
        {
            BucketName = _options.BucketName,
            Prefix = prefix
        }, cancellationToken);

        return response.S3Objects
            .Where(item => ImageFiles.HasImageExtension(item.Key))
            .Select(item => new BlobObjectDto
            {
                Key = item.Key,
                Url = GetPublicUrl(item.Key),
                Size = item.Size ?? 0
            })
            .ToList();
    }
}
