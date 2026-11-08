# Distribution

The generator is a normal .NET 10 console application. Templates are embedded in
`MonorepoDotnet.Infrastructure` as manifest resources, so a published binary is
fully self-contained: it does not need a `templates/` folder next to it.

## Single-file executable (Windows)

```powershell
dotnet publish src/MonorepoDotnet.CLI `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -o artifacts/win-x64
```

Output: `artifacts/win-x64/monorepo.exe`.

Publish also copies that executable to `%USERPROFILE%\.dotnet\tools\monorepo.exe`,
which is already on PATH after installing the .NET SDK. After that, open any
terminal and run:

```powershell
monorepo
monorepo --version
```

Skip the PATH install with `-p:InstallAfterPublish=false`. New terminals pick up
PATH changes automatically; the current session already sees `.dotnet\tools`.

You can still run the artifact directly:

```powershell
.\artifacts\win-x64\monorepo.exe
```

### Framework-dependent publish

Smaller, but requires the .NET 10 runtime on the target machine:

```powershell
dotnet publish src/MonorepoDotnet.CLI -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o artifacts/win-x64-fd
```

## Other platforms

Change the runtime identifier and drop the Windows-specific native extraction
flag:

```bash
# Linux
dotnet publish src/MonorepoDotnet.CLI -c Release -r linux-x64 --self-contained true \
  -p:PublishSingleFile=true -o artifacts/linux-x64

# macOS (Apple silicon)
dotnet publish src/MonorepoDotnet.CLI -c Release -r osx-arm64 --self-contained true \
  -p:PublishSingleFile=true -o artifacts/osx-arm64
```

Cross-publishing a self-contained app requires the matching runtime packs, which
the SDK restores automatically.

## Why templates survive publishing

`MonorepoDotnet.Infrastructure.csproj` embeds every template under `templates/`
as a resource:

```xml
<EmbeddedResource Include="..\..\templates\**\*"
                  LogicalName="templates/%(RecursiveDir)%(Filename)%(Extension)"
                  WithCulture="false" />
```

`WithCulture="false"` is required: without it MSBuild treats `*.cs.tpl` files as
satellite resources for specific cultures and the build fails. The resource name
is `templates/<architecture>/<relative path>`, which is what `TemplateProvider`
looks up at runtime.

Resolution order in `TemplateProvider`:

1. Embedded manifest resource (always present in published builds).
2. Disk fallback: `templates/<architecture>/...` walking up from the current directory.
3. `MONOREPODOTNET_TEMPLATES` environment variable pointing at a templates root.

The disk fallbacks exist so templates can be edited during development without
rebuilding.

## .NET global tool

The CLI project already declares:

```xml
<PackAsTool>true</PackAsTool>
<ToolCommandName>monorepo</ToolCommandName>
<PackageId>MonorepoDotnet.CLI</PackageId>
```

Pack and install locally:

```bash
dotnet pack src/MonorepoDotnet.CLI -c Release -o artifacts/packages
dotnet tool install --global --add-source artifacts/packages MonorepoDotnet.CLI
monorepo --version
```

Update or remove:

```bash
dotnet tool update --global --add-source artifacts/packages MonorepoDotnet.CLI
dotnet tool uninstall --global MonorepoDotnet.CLI
```

To publish to NuGet.org, set a unique `PackageId` (the current one is a
placeholder), add package metadata (authors, license, repository URL) and push:

```bash
dotnet nuget push artifacts/packages/MonorepoDotnet.CLI.0.1.0.nupkg --api-key $NUGET_API_KEY --source https://api.nuget.org/v3/index.json
```

## Versioning

The version comes from `<Version>` in `MonorepoDotnet.CLI.csproj` and is what
`--version` prints. Bump it before packing or publishing.

## Verifying a published build

```powershell
.\artifacts\win-x64\monorepo.exe --version
.\artifacts\win-x64\monorepo.exe --help
monorepo --version
```

Then run the interactive flow from a directory that does not contain a
`templates` folder. Successful generation proves the embedded templates are
resolved from the assembly rather than from disk.
