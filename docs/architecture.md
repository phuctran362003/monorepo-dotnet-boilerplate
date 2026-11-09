# Architecture

`monorepo-dotnet` is a three-project .NET 10 solution that generates ASP.NET Core
solutions in DDD, Clean Architecture, 1-layer and MVC layouts. The generator
itself follows a lightweight layered architecture.

```text
MonorepoDotnet.CLI ──────────► Core
        │                       ▲
        └──────► Infrastructure ┘
```

| Project | Responsibility |
| --- | --- |
| `MonorepoDotnet.CLI` | Composition root, Spectre.Console prompts, progress reporting, exit codes. |
| `MonorepoDotnet.Core` | Models, enums, abstractions, validation and generator use cases. |
| `MonorepoDotnet.Infrastructure` | File system, embedded templates, template rendering, process execution, environment validation. |

## Dependency rules

- `Core` depends on nothing but the base class library.
- `Infrastructure` depends on `Core` and implements its abstractions.
- `CLI` depends on both and is the only project allowed to know about DI wiring.

`Core` never references `Spectre.Console`, `System.Diagnostics.Process` or
`System.IO.Directory`; it works exclusively through abstractions.

## Key abstractions

| Abstraction | Implementation | Purpose |
| --- | --- | --- |
| `IProcessRunner` | `ProcessRunner` | Runs `dotnet`, `docker`, `dotnet-ef` with `ArgumentList`, captured stdout/stderr, exit code and cancellation support. |
| `IFileSystem` | `FileSystemService` | All file and directory access used during generation. |
| `ITemplateProvider` | `TemplateProvider` | Reads templates from embedded resources (single-file friendly) with a development fallback to `templates/`. |
| `ITemplateRenderer` | `TemplateRenderer` | `{{Token}}` replacement plus `{{#if Flag}}` blocks. Fails fast on unknown tokens and flags. |
| `IEnvironmentValidator` | `EnvironmentValidator` | Compatible .NET SDK for the selected TFM, Docker CLI/Engine, output directory permissions, output path validity, solution path availability, solution name. |
| `IDotNetSdkInstaller` | `DotNetSdkInstaller` | Installs the matching SDK from Microsoft (WinGet, else the official `dotnet-install` script). |
| `ISolutionGenerator` | `SolutionGenerator` | Orchestrates the generation pipeline for every architecture. |

## Generation pipeline

`SolutionGenerator.GenerateAsync` runs eight steps and reports each one
through `IProgress<GenerationProgress>`:

1. **Prepare** — validate options and create `{OutputDirectory}/{SolutionName}`.
   A non-empty existing directory aborts immediately; existing files are never
   overwritten.
2. **Write project files** — render every entry returned by
   `ArchitectureTemplateManifest.For(options)` for the selected architecture
   and options.
3. **Write Docker environment** — generate `.env` with a random 48-character
   password when Docker is enabled.
4. **Create solution** — `dotnet new sln` followed by `dotnet sln add`.
5. **Restore** — `dotnet restore {SolutionName}.sln`.
6. **Build** — `dotnet build --no-restore -c Debug --nologo`.
7. **Initial migration** — `dotnet ef migrations add InitialCreate`, using the
   project/startup project/output directory from `SolutionLayouts`, best effort.
8. **Finalize** — collect warnings and return `GenerationResult`.

Failure semantics: a failing step throws `GenerationException`, the CLI shows a
failure panel, and the root directory is deleted if the generator created it.
A missing `dotnet-ef` tool or a failed migration only adds a warning; the
solution is still usable.

Cancellation (`Ctrl+C`) propagates an `OperationCanceledException`, triggers the
same cleanup and exits with code `130`.

## Configuration flow

`GeneratorOptions` is an immutable record produced by `ConfigurationPrompt`:

- `SolutionName` — single source of truth for project names, namespaces, Docker
  names and the database name.
- `OutputDirectory`, `DotNetVersion`, `Architecture`, `Database`,
  `IncludeDocker`, `Packages` (flags enum), `BuildAfterGeneration`.
- Derived: `RootDirectory`, `DockerName`, project names, `MainProject`,
  `HasPackage(...)`.

The output directory is validated by `OutputPathValidator`:

- `Validate(outputDirectory)` checks invalid path/file-name characters,
  reserved Windows device names, missing drive roots, existing files in the
  path, length and accessibility.
- `ValidateTarget(outputDirectory, solutionName)` additionally rejects an
  existing non-empty `{OutputDirectory}/{SolutionName}` folder.

`ConfigurationPrompt` runs these checks before the environment report, and
`EnvironmentValidator` and `SolutionGenerator` repeat them as a safety net.

`SolutionLayouts.Create(options)` maps an architecture to its projects, main
project, migration project/startup project and migration output directory. The
generator uses that layout for `dotnet sln add`, `dotnet ef` and the Dockerfile
`COPY`/`ENTRYPOINT` lines.

The database choice is also resolved into tokens and flags:
`DatabaseProviderExtensions` supplies display names, `GeneratorOptions` derives
`DatabasePort`, `DatabaseUser` and `DatabaseService`, and the generator adds
`{{Database}}`, `{{DatabasePort}}`, `{{DatabaseUser}}`, `{{DatabaseService}}`
plus the `{{#if Postgres}}` / `{{#if SqlServer}}` flags. Templates therefore emit
the matching EF Core provider, connection string, Compose service and `.env`
keys without separate per-database file trees.

`BuildTemplateValues` converts the options into the token dictionary
(`{{SolutionName}}`, `{{TargetFramework}}`, `{{EfCoreVersion}}`, …) and
`BuildTemplateFlags` into boolean flags (`{{#if Docker}}`, `{{#if Serilog}}`, …).
Framework-specific package and Docker image versions live in
`DotNetVersionCatalog` (via `PackageVersions.For`); shared package versions live
in `PackageVersions.cs`. They are substituted into the generated `.csproj`
files, so there is no `Directory.Packages.props`.

## Templates

Templates live under `templates/` and are embedded into
`MonorepoDotnet.Infrastructure` as resources named
`templates/<architecture>/<relative path>`:

- `templates/DDD/**` — DDD projects plus the shared root and Docker templates.
- `templates/Clean/**` — Clean-specific overrides (simple entity, mappings,
  configuration, README); shared files reuse the DDD templates.
- `templates/SingleLayer/**` — single-project Web API layout.
- `templates/Mvc/**` — single-project MVC layout with Razor views.

```xml
<EmbeddedResource Include="..\..\templates\**\*"
                  LogicalName="templates/%(RecursiveDir)%(Filename)%(Extension)"
                  WithCulture="false" />
```

`WithCulture="false"` is critical: without it MSBuild interprets `*.cs.tpl` as a
culture-specific satellite resource and the build fails.

Because templates are embedded, `dotnet publish -p:PublishSingleFile=true`
produces a self-contained executable that does not need a `templates` folder
next to it. During development, `TemplateProvider` also looks for
`templates/<architecture>/...` walking up from the current directory
(overridable with `MONOREPODOTNET_TEMPLATES`).

### Template syntax

| Syntax | Meaning |
| --- | --- |
| `{{Token}}` | Replaced from the values dictionary. Unknown tokens throw. |
| `{{#if Flag}}...{{/if}}` | Block included when the flag is `true`. |
| `{{#if !Flag}}...{{/if}}` | Block included when the flag is `false`. |

Blocks nest, and every opening tag must have a matching `{{/if}}`.

`DddTemplateManifest` declares each generated file as a `TemplateFile` with its
template path, output path and optional predicate (package or Docker flag), so
adding a file is a single list entry plus a template.

## Generated solution architecture

DDD:

```text
{SolutionName}/
├── src/
│   ├── {SolutionName}.API/
│   ├── {SolutionName}.Application/
│   ├── {SolutionName}.Domain/
│   └── {SolutionName}.Infrastructure/
├── docker-compose.yml
├── Dockerfile
├── .dockerignore
├── .env
├── .env.example
├── .editorconfig
├── .gitignore
├── Directory.Build.props
├── README.md
└── {SolutionName}.sln
```

Clean Architecture uses `{SolutionName}.WebApi` instead of `{SolutionName}.API`
and a simpler domain model. The 1-layer layout uses one project,
`src/{SolutionName}`, with `Controllers`, `Models`, `Data`, `Products` and
related folders. MVC uses one project, `src/{SolutionName}.Web`, with
`Controllers`, `Views`, `ViewModels`, `Services` and `Data`.

DDD/Clean project dependencies:

```text
API/WebApi ──► Application, Infrastructure
Infrastructure ──► Application, Domain
Application ──► Domain
Domain ──► (nothing)
```

Domain has no project references. Its project file still references EF Core,
EF Core Tools, `Microsoft.Extensions.Configuration`,
`Microsoft.Extensions.Configuration.Json` and JWT Bearer, plus the SQL Server
provider when that database is selected. Application references
`FluentValidation.AspNetCore`. API/WebApi references `Microsoft.AspNetCore.OData`.
Versions come from `DotNetVersionCatalog` for the selected target framework.

Domain contains entities, value objects, enums, domain events, domain
exceptions and repository interfaces. Application contains use cases, DTOs,
interfaces and its own DI registration. Infrastructure contains the
`ApplicationDbContext`, entity configurations, repositories, migrations and a
design-time factory. API/WebApi contains controllers, middleware, extensions and
`Program.cs`.

See [generated-solution.md](generated-solution.md) for the full file inventory.

## CLI composition

`Program.cs` builds a `HostApplicationBuilder`, registers Infrastructure, the
generator and the prompt/renderer services, then resolves `CliApplication`.

`CliApplication.RunAsync` handles `--help`/`--version`, renders the banner,
collects options, runs `IEnvironmentValidator`, applies the overwrite guard,
shows the summary, asks for confirmation, renders a Spectre progress bar while
`ISolutionGenerator` runs, and finally prints a success or failure panel. Exit
codes: `0` success, `1` failure/cancel, `130` Ctrl+C.

## Design notes

- The generator depends only on abstractions, so it can be unit-tested later
  without touching the file system or spawning processes.
- `ProcessRunner` always uses `ArgumentList`, never string concatenation, and
  captures stdout/stderr for error reporting.
- Generated secrets never live in source: `.env` is generated, git-ignored, and
  referenced from Compose; `appsettings.json` keeps the password empty.
- Generated solutions target `net10.0`, `net9.0` or `net8.0` (package and Docker
  image versions come from `DotNetVersionCatalog`); PostgreSQL and SQL Server
  are supported and all four architectures are implemented. Other databases and
  frameworks are rejected by `ValidateOptions`.
- When no compatible SDK is installed, the CLI shows Microsoft install commands
  and can run WinGet or the official `dotnet-install` script.
