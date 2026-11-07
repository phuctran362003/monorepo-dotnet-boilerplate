# Usage

`monorepo-dotnet` is an interactive CLI. Run it, answer the prompts, confirm the
summary, and it generates, restores, builds and migrates a complete solution.

## Run from source

```bash
dotnet run --project src/MonorepoDotnet.CLI
```

## Run a published build

See [distribution.md](distribution.md) for producing a single-file executable or
a .NET global tool.

## Command line arguments

| Argument | Behavior |
| --- | --- |
| *(none)* | Starts the interactive workflow. |
| `--help`, `-h`, `/?` | Prints usage and exits with code `0`. |
| `--version`, `-v` | Prints the tool version and exits with code `0`. |

All configuration is collected interactively; there are no flags that skip the
prompts in this version.

## Interactive workflow

1. **Banner** — tool name and purpose.
2. **Solution configuration** prompts (see below).
3. **Environment checks** — a table reports .NET SDK, Docker CLI, Docker Engine,
   output directory permissions and solution name validation. Errors abort the
   run; warnings are informational.
4. **Overwrite guard** — if `{OutputDirectory}/{SolutionName}` exists and is not
   empty, the run stops. Existing files are never overwritten.
5. **Configuration summary** — a table of every choice, followed by a
   confirmation question.
6. **Generation progress** — a live progress bar over the eight pipeline steps.
7. **Result panel** — success with the output path and next steps, or a failure
   panel with the underlying error.

Pressing `Ctrl+C` cancels the run. The exit code is `130`, and a partially
generated directory is removed when the generator created it.

## Prompts

| Prompt | Default | Notes |
| --- | --- | --- |
| Solution name | none | Required. Must match `^[A-Za-z][A-Za-z0-9_]*$`, at most 64 characters. |
| Output directory | current directory | Absolute or relative; normalized with `Path.GetFullPath`. |
| .NET version | `net10.0` | Only `net10.0` is offered. |
| Architecture | DDD | Only DDD is offered. |
| Database | PostgreSQL | EF Core Code First, Npgsql provider. |
| Docker files | Yes | Generates `Dockerfile`, `docker-compose.yml`, `.dockerignore`, `.env.example` and `.env`. |
| Optional packages | none | Multi-select: Serilog, FluentValidation, AutoMapper, Redis, MediatR. |

Unit and integration test generation is **not offered**, because the MVP does
not generate test projects. The CLI only shows options that are implemented.

### Solution name rules

The solution name is the single source of truth for:

- The solution file: `{SolutionName}.sln`
- Project names and namespaces: `{SolutionName}.API`, `.Application`, `.Domain`,
  `.Infrastructure`
- Docker service, container and image names (lower-cased)
- The PostgreSQL database name (lower-cased)

Invalid names (empty, starting with a digit, containing spaces, dots, dashes or
non-ASCII characters, or longer than 64 characters) are rejected at the prompt.

## What happens after confirmation

The generator writes every file from embedded templates, then runs:

1. `dotnet new sln --name {SolutionName} --output {root} --format sln`
2. `dotnet sln {SolutionName}.sln add src/.../*.csproj`
3. `dotnet restore {SolutionName}.sln`
4. `dotnet build {SolutionName}.sln --no-restore -c Debug --nologo`
5. `dotnet ef migrations add InitialCreate` (best effort)

Steps 1–4 are fatal on failure: the newly created directory is deleted and the
error is shown. Step 5 only produces a warning; the solution is still valid and
you can create the migration manually.

No database is created or updated during generation. Migrations are only applied
when you ask for it (`dotnet ef database update`, or `APPLY_MIGRATIONS=true`
with Docker Compose).

## Environment requirements

| Requirement | When it is checked | Severity |
| --- | --- | --- |
| .NET SDK 10.0.100+ | Always | Error |
| Docker CLI | Only when Docker files are requested | Error |
| Docker Engine reachable | Only when Docker files are requested | Warning |
| Output directory writable | Always | Error |
| Valid solution name | Always | Error |

Generating Docker files works with the Docker Engine stopped; only running the
containers needs it.

## Exit codes

| Code | Meaning |
| --- | --- |
| `0` | Success, or `--help` / `--version`. |
| `1` | Validation failure, existing non-empty directory, generation failure, or cancelled at the confirmation prompt. |
| `130` | Cancelled with `Ctrl+C`. |

## Troubleshooting

- **"This tool needs an interactive terminal"** — standard input is redirected
  (for example when piping input or running from a CI script). The prompts need a
  real console window.
- **"The target directory already exists and is not empty"** — choose another
  solution name or output directory. The tool never merges into existing files.
- **Docker Engine warning** — start Docker Desktop (or the daemon). Docker file
  generation still succeeds.
- **Migration warning** — install the tool with
  `dotnet tool install --global dotnet-ef --version 10.0.14`, then run the
  command printed in the generated `README.md`.
- **Build failure** — the full `dotnet build` output is included in the failure
  panel. The partially generated directory is removed so the next attempt starts
  clean.
