# Usage

`monorepo-dotnet` is an interactive CLI. Run it, answer the prompts, confirm the
summary, and it generates, restores, builds and migrates a complete solution.

## Run from source

```bash
dotnet run --project src/MonorepoDotnet.CLI
```

## Run a published build

After `dotnet publish` (see [distribution.md](distribution.md)), the command
`monorepo` is installed on PATH. Open any terminal:

```bash
monorepo
```

You can also pack it as a .NET global tool; the command name is the same.

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
   output directory permissions, the complete solution path and solution name
   validation. Errors abort the run; warnings are informational.
4. **Overwrite guard** — if `{OutputDirectory}/{SolutionName}` exists and is not
   empty, the prompt rejects it and the run stops. Existing files are never
   overwritten.
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
| Output directory | current directory | Absolute or relative; validated and normalized with `Path.GetFullPath`. |
| .NET version | `net10.0` | Only `net10.0` is offered. |
| Architecture | DDD | DDD, Clean Architecture, 1-Layer (single project) or MVC. |
| Database | PostgreSQL | PostgreSQL or SQL Server, both EF Core Code First. |
| Docker files | Yes | Generates `Dockerfile`, `docker-compose.yml`, `.dockerignore`, `.env.example` and `.env`. |
| Optional packages | none | Multi-select: Serilog, FluentValidation, AutoMapper, Redis, MediatR. |

Unit and integration test generation is **not offered**, because the MVP does
not generate test projects. The CLI only shows options that are implemented.

### Architectures

| Choice | Layout |
| --- | --- |
| DDD | `Domain`, `Application`, `Infrastructure`, `API` with aggregate roots, value objects and domain events. |
| Clean Architecture | `Domain`, `Application`, `Infrastructure`, `WebApi`; a simpler entity model with the same inward dependency rule. |
| 1-Layer | Single `{SolutionName}` web project with `Controllers`, `Models`, `Data`, `Products` and `Services`. |
| MVC | Single `{SolutionName}.Web` project with `Controllers`, `Views`, `ViewModels`, `Services` and `Data`. |

All four options support PostgreSQL, SQL Server, Docker and the optional
packages. The sample `Product` slice is generated in every layout.

### Databases

| Choice | Provider | Docker service | Port | `.env` keys |
| --- | --- | --- | --- | --- |
| PostgreSQL | `Npgsql.EntityFrameworkCore.PostgreSQL` | `postgres` | 5432 | `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_PORT` |
| SQL Server | `Microsoft.EntityFrameworkCore.SqlServer` | `sqlserver` | 1433 | `MSSQL_DB`, `MSSQL_USER`, `MSSQL_SA_PASSWORD`, `MSSQL_PORT` |

SQL Server uses the `sa` login and stores the password in
`Database__Password` / the `Database:Password` configuration key, not in the
connection string. The SQL Server container starts empty; the application
database is created by the first migration (`dotnet ef database update` or
`APPLY_MIGRATIONS=true`).

### Solution name rules

The solution name is the single source of truth for:

- The solution file: `{SolutionName}.sln`
- Project names and namespaces: DDD `{SolutionName}.API`; Clean
  `{SolutionName}.WebApi`; 1-layer `{SolutionName}`; MVC `{SolutionName}.Web`.
  All multi-project layouts also use `.Application`, `.Domain` and
  `.Infrastructure`.
- Docker service, container and image names (lower-cased)
- The database name (lower-cased) and the Docker database service (`postgres`
  or `sqlserver`)

Invalid names (empty, starting with a digit, containing spaces, dots, dashes or
non-ASCII characters, or longer than 64 characters) are rejected at the prompt.

### Output path rules

The output directory is validated twice: immediately at the prompt (before the
environment report) and again by the generator before any file is written.
The complete target is `{OutputDirectory}/{SolutionName}`.

Rejected paths:

- Empty output directory.
- Characters that are invalid in a path, or `* ? : " < > |` in any folder name.
- Reserved Windows device names such as `CON`, `PRN`, `AUX`, `NUL`, `COM1` and
  `LPT1`.
- A missing or inaccessible drive root (for example an unmapped `Z:\`).
- A path that points to an existing file, or that runs through an existing file.
- A path longer than 260 characters, including the solution name.
- An existing `{OutputDirectory}/{SolutionName}` directory that is not empty.

Relative paths are resolved against the current working directory. An existing
empty target directory is allowed; existing files are never overwritten.

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
with Docker Compose). With SQL Server the database itself is created by that
first migration.

## Environment requirements

| Requirement | When it is checked | Severity |
| --- | --- | --- |
| .NET SDK 10.0.100+ | Always | Error |
| Docker CLI | Only when Docker files are requested | Error |
| Docker Engine reachable | Only when Docker files are requested | Warning |
| Output directory writable | Always | Error |
| Output path valid | Always | Error |
| Target solution path available | Always | Error |
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
- **Path rejected at the output prompt** — fix the invalid character, reserved
  name, drive root, file collision or length reported by the prompt. Nothing is
  written until the path is valid.
- **Docker Engine warning** — start Docker Desktop (or the daemon). Docker file
  generation still succeeds.
- **Migration warning** — install the tool with
  `dotnet tool install --global dotnet-ef --version 10.0.14`, then run the
  command printed in the generated `README.md`.
- **Build failure** — the full `dotnet build` output is included in the failure
  panel. The partially generated directory is removed so the next attempt starts
  clean.
