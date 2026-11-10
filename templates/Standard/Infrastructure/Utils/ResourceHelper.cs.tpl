using System.Reflection;
using System.Text;

namespace {{InfrastructureNamespace}}.Utils;

public static class ResourceHelper
{
    public static string ReadResource(string relativePath, Assembly fromAssembly)
    {
        var resourceName = $"{fromAssembly.GetName().Name}.{relativePath.Replace('/', '.').Replace('\\', '.')}";
        using var stream = fromAssembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' was not found.");
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    public static string ReadJsonResource(string relativePath, Assembly fromAssembly, bool stripWhitespace = false)
    {
        var json = ReadResource(relativePath, fromAssembly);
        return stripWhitespace ? json.StripJsonWhitespace() : json;
    }

    public static int DateTimeValidate(DateTime startDate, DateTime endDate) =>
        (endDate.Date - startDate.Date).Days + 1;
}
