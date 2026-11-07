# Architecture

`monorepo-dotnet` is a three-project .NET 10 solution that generates ASP.NET Core
DDD solutions. The generator itself follows a lightweight layered architecture.

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
| `IEnvironmentValidator` | `EnvironmentValidator` | .NET SDK version, Docker CLI/Engine, output directory permissions, solution name. |
| `ISolutionGenerator` | `DddSolutionGenerator` | Orchestrates the generation pipeline. |

## Generation pipeline

`DddSolutionGenerator.GenerateAsync` runs eight steps and reports each one
through `IProgress<GenerationProgress>`:

1. **Prepare** — validate options and create `{OutputDirectory}/{SolutionName}`.
   A non-empty existing directory aborts immediately; existing files are never
   overwritten.
2. **Write project files** — render every entry of `DddTemplateManifest` for the
   selected options (53 templates at most, 42 files for the default options).
3. **Write Docker environment** — generate `.env` with a random 48-character
   password when Docker is enabled.
4. **Create solution** — `dotnet new sln` followed by `dotnet sln add`.
5. **Restore** — `dotnet restore {SolutionName}.sln`.
6. **Build** — `dotnet build --no-restore -c Debug --nologo`.
7. **Initial migration** — `dotnet ef migrations add InitialCreate`, best effort.
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
- Derived: `RootDirectory`, `DockerName`, project names, `HasPackage(...)`.

`BuildTemplateValues` converts the options into the token dictionary
(`{{SolutionName}}`, `{{TargetFramework}}`, `{{EfCoreVersion}}`, …) and
`BuildTemplateFlags` into boolean flags (`{{#if Docker}}`, `{{#if Serilog}}`, …).
Package and image versions live in `PackageVersions.cs` and are substituted into
the generated `.csproj` files, so there is no `Directory.Packages.props` and
upgrading is a one-file change.

## Templates

Templates live in `templates/DDD/**` and are embedded into
`MonorepoDotnet.Infrastructure` as resources named
`templates/DDD/<relative path>`:

```xml
<EmbeddedResource Include="..\..\templates\DDD\**\*"
                  LogicalName="templates/DDD/%(RecursiveDir)%(Filename)%(Extension)"
                  WithCulture="false" />
```

`WithCulture="false"` is critical: without it MSBuild interprets `*.cs.tpl` as a
culture-specific satellite resource and the build fails.

Because templates are embedded, `dotnet publish -p:PublishSingleFile=true`
produces a self-contained executable that does not need a `templates` folder
next to it. During development, `TemplateProvider` also looks for
`templates/DDD/...` walking up from the current directory (overridable with
`MONOREPODOTNET_TEMPLATES`).

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

Project dependencies:

```text
API ──► Application, Infrastructure
Infrastructure ──► Application, Domain
Application ──► Domain
Domain ──► (nothing)
```

Domain contains entities, value objects, enums, domain events, domain
exceptions and repository interfaces. Application contains use cases, DTOs,
interfaces and its own DI registration. Infrastructure contains the
`ApplicationDbContext`, entity configurations, repositories, migrations and a
design-time factory. API contains controllers, middleware, extensions and
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
- Generated solutions target `net10.0` only; unsupported architectures,
  databases and frameworks are rejected by `ValidateOptions`.
