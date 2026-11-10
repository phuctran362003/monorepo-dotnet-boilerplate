namespace {{SolutionName}}.Service.DTOs;

public class UploadFileResultDto
{
    public string Key { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long Size { get; set; }
}

public class BlobObjectDto
{
    public string Key { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public long Size { get; set; }
}

public class BlobDownloadResult
{
    public Stream Content { get; set; } = Stream.Null;

    public string ContentType { get; set; } = "application/octet-stream";

    public string FileName { get; set; } = string.Empty;
}
