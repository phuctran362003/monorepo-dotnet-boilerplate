namespace MonorepoDotnet.Core.Distribution;

/// <summary>Identity of the published .NET global tool.</summary>
public static class ToolPackage
{
    public const string Id = "monorepo-dotnet";

    public const string UpdateCommand = "dotnet tool update --global monorepo-dotnet";
}
