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

Source: [phuctran362003/monorepo-dotnet-boilerplate](https://github.com/phuctran362003/monorepo-dotnet-boilerplate).

The CLI project is packed as a global tool:

| Property | Value |
| --- | --- |
| `PackageId` | `monorepo-dotnet` (what people pass to `dotnet tool install`) |
| `ToolCommandName` | `monorepo` (what they type afterwards) |
| License | MIT |
| Repository | https://github.com/phuctran362003/monorepo-dotnet-boilerplate |

A global tool is framework-dependent: installers need the .NET 10 SDK (the
generator needs the SDK anyway). Do **not** pack with `-r win-x64` or
`PublishSingleFile`; `dotnet pack` builds a portable tool.

`PackageId` is frozen at first NuGet.org push. Do not rename it after that.

### Pack and install locally (dry run)

Do this before the first NuGet.org push.

```powershell
dotnet pack src/MonorepoDotnet.CLI -c Release -o artifacts/packages
dotnet tool uninstall --global monorepo-dotnet   # ignore errors if not installed
dotnet tool install --global --add-source artifacts/packages monorepo-dotnet
monorepo --version
```

Update or remove a local install:

```powershell
dotnet tool update --global --add-source artifacts/packages monorepo-dotnet
dotnet tool uninstall --global monorepo-dotnet
```

### Publish from GitHub Actions (Trusted Publishing)

This is the path that uses the nuget.org policy form. It does **not** need a
long-lived API key. The policy is bound to a specific workflow file: if that
file is missing, the policy never issues a key.

Repo already has `.github/workflows/publish.yml`. Fill the policy as:

| Field | Value |
| --- | --- |
| Policy Name | `monorepo-dotnet-release` |
| Package Owner | `devphuctrann` (nuget.org profile name, not email) |
| CI/CD Provider | GitHub Actions |
| Repository Owner | `phuctran362003` |
| Repository | `monorepo-dotnet-boilerplate` (the GitHub repo name, not the local folder) |
| Workflow File | `publish.yml` (file name only, no `.github/workflows/`) |
| Environment | `production` |

On GitHub, create the environment **Settings → Environments → `production`**.
The job uses `environment: production`; if the policy has that field filled,
the names must match. Leave Environment empty on nuget.org only if you also
remove `environment:` from the workflow.

After the policy is saved and this workflow is on the default branch:

1. Bump `<Version>` and push.
2. Actions → **publish** → Run workflow, or publish a GitHub Release.

Local `dotnet nuget push` still needs a classic API key (below). Trusted
Publishing only applies to that GitHub Actions job.

### Publish to nuget.org (Trusted Publishing only)

This repo publishes exclusively through `.github/workflows/publish.yml`, which
uses **nuget.org Trusted Publishing (OIDC)**. There is no long-lived API key and
no local publish script in the repository.

1. Bump `<Version>` in `src/MonorepoDotnet.CLI/MonorepoDotnet.CLI.csproj` and
   push to `main` (or tag a release). CI verifies the version is not already
   published.
2. Trigger `.github/workflows/publish.yml` (Actions → publish → Run workflow,
   or publish a GitHub Release). The job uses `environment: production` and the
   OIDC identity, so no secret lives in the repo.

The nuget.org policy must match `publish.yml` (owner `devphuctrann`, repository
`phuctran362003/monorepo-dotnet-boilerplate`, workflow file `publish.yml`,
environment `production`). Create the environment in
**Settings → Environments → `production`** on GitHub.

You cannot replace a version that already exists on nuget.org — always bump
`<Version>` before pushing. Since `publish.yml` no longer passes
`--skip-duplicate`, a duplicate version fails the push loudly instead of
silently succeeding.

Users install from nuget.org:

```powershell
dotnet tool install --global monorepo-dotnet
monorepo
dotnet tool update --global monorepo-dotnet
```

### Pack and install locally (dry run)

Do this before the first NuGet.org push to verify the tool end-to-end:

```powershell
dotnet pack src/MonorepoDotnet.CLI -c Release -o artifacts/packages
dotnet tool uninstall --global monorepo-dotnet   # ignore errors if not installed
dotnet tool install --global --add-source artifacts/packages monorepo-dotnet
monorepo --version
```

Update or remove a local install:

```powershell
dotnet tool update --global --add-source artifacts/packages monorepo-dotnet
dotnet tool uninstall --global monorepo-dotnet
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
