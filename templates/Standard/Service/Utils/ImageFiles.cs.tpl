using Microsoft.AspNetCore.Http;

namespace {{SolutionName}}.Service.Utils;

public static class ImageFiles
{
    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".jfif", ".png", ".gif", ".webp", ".bmp", ".tif", ".tiff", ".tga", ".pbm", ".pgm", ".ppm", ".qoi"
    };

    public static bool IsImage(IFormFile file)
    {
        if (file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return HasImageExtension(file.FileName);
    }

    public static bool HasImageExtension(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        return Extensions.Contains(Path.GetExtension(key));
    }
}
