# monorepo-dotnet

Interactive CLI that generates production-ready ASP.NET Core solutions using
DDD, Clean Architecture, a single-project (1-layer) layout or MVC, with Entity
Framework Core, PostgreSQL or SQL Server, and Docker.

The generator asks a few questions, then writes a complete solution, restores
packages, builds it and creates the initial EF Core migration.

```text
┌──────────────────────────────────────────────────────────────────────────────┐
│  monorepo-dotnet                                                             │
│  ASP.NET Core solution generator — DDD, Clean, 1-Layer, MVC — PostgreSQL, EF │
└──────────────────────────────────────────────────────────────────────────────┘
  Solution name      CongThongTin
  Output directory   D:\projects
  Target framework   net10.0
  Architecture       Clean Architecture (Domain, Application, Infrastructure, WebApi)
  Database           PostgreSQL (EF Core, Code First)
  Docker             Yes
  Optional packages  Serilog, FluentValidation
```

## Requirements

- .NET SDK 10.0.100 or later (`dotnet --list-sdks`)
- PostgreSQL 14+ or SQL Server 2019+ — local install or Docker
- Docker CLI + Docker Engine — optional, only needed to run the generated
  containers
- EF Core CLI for migrations:
  `dotnet tool install --global dotnet-ef --version 10.0.14` — optional, the
  generator only warns if it is missing

## Quick start

```bash
git clone <repository>
cd monorepo-dotnet
dotnet run --project src/MonorepoDotnet.CLI
```

Then answer the prompts. When it finishes:

```bash
cd <output-directory>/<SolutionName>
docker compose up --build
curl http://localhost:8080/health
```

## Databases

The database prompt offers two providers, both with EF Core Code First:

| Choice | EF Core provider | Docker service | Default port | `.env` variables |
| --- | --- | --- | --- | --- |
| PostgreSQL | `Npgsql.EntityFrameworkCore.PostgreSQL` | `postgres` (`postgres:17-alpine`) | 5432 | `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_PORT` |
| SQL Server | `Microsoft.EntityFrameworkCore.SqlServer` | `sqlserver` (`mcr.microsoft.com/mssql/server:2022-latest`) | 1433 | `MSSQL_DB`, `MSSQL_USER`, `MSSQL_SA_PASSWORD`, `MSSQL_PORT` |

For SQL Server the login is `sa`; the generated password is placed in
`Database__Password` (configuration) or the `Database__Password` environment
variable, never inside the connection string. The SQL Server container starts
without the application database: it is created by the first migration
(`dotnet ef database update`, or `APPLY_MIGRATIONS=true`).

## What gets generated

Every architecture produces `{SolutionName}.sln`, the shared root files
(`Directory.Build.props`, `.editorconfig`, `.gitignore`, `README.md`), an
initial EF Core migration when `dotnet-ef` is installed, and optional Docker
files (`.env`, `.env.example`, `Dockerfile`, `docker-compose.yml`,
`.dockerignore`).

| Architecture | Project layout | Entry point |
| --- | --- | --- |
| DDD | `Domain`, `Application`, `Infrastructure`, `API` | `{SolutionName}.API` |
| Clean | `Domain`, `Application`, `Infrastructure`, `WebApi` | `{SolutionName}.WebApi` |
| 1-Layer | one web project with folders for controllers, models, data and services | `{SolutionName}` |
| MVC | one web project with `Controllers`, `Views`, `ViewModels`, `Services`, `Data` | `{SolutionName}.Web` |

The DDD and Clean layouts keep dependencies pointing inward:

```text
API/WebApi ──► Application + Infrastructure
Infrastructure ──► Application + Domain
Application ──► Domain
Domain ──► none
```

The 1-layer and MVC layouts deliberately use a single project. They are a
pragmatic starting point for small applications and can be split later.

All architectures include the same sample `Product` slice (list, get, create)
so the wiring — controllers, validation, mapping, caching, logging, EF Core —
can be verified end to end. MVC additionally ships Razor views and a static
stylesheet.

## Features

- **Interactive Spectre.Console UI** — validated prompts, environment report,
  configuration summary, confirmation, live progress.
- **Safe by default** — verifies input and solution paths before writing, never
  overwrites existing files, deletes a failed generation directory; `Ctrl+C`
  cancels cleanly.
- **Four architectures** — DDD, Clean Architecture, 1-layer and MVC, all using
  the same PostgreSQL/EF Core, Docker and optional-package wiring.
- **PostgreSQL or SQL Server** — the database prompt selects the EF Core
  provider, connection string, Docker service and Compose environment.
- **DDD structure** — aggregate root, value objects, domain events, repository
  interfaces, application services and handlers.
- **Clean structure** — a simpler domain model with Domain, Application,
  Infrastructure and WebApi projects and dependencies pointing inward.
- **EF Core + PostgreSQL or SQL Server** — `ApplicationDbContext`, entity
  configurations, design-time factory, `InitialCreate` migration, retry on
  failure for both providers.
- **Docker** — multi-stage `Dockerfile`, Compose with PostgreSQL or SQL Server, persistent
  volumes, healthchecks, bridge network, environment-based secrets, service-name
  host (`postgres`, never `localhost`).
- **Optional packages** — Serilog, FluentValidation, AutoMapper, Redis, MediatR,
  each fully wired (not just added to the `.csproj`).
- **Single-file distribution** — templates are embedded resources, so the
  published executable needs no external files.

## Documentation

| Document | Contents |
| --- | --- |
| [docs/usage.md](docs/usage.md) | Prompts, validation rules, pipeline, exit codes, troubleshooting. |
| [docs/architecture.md](docs/architecture.md) | Generator layering, abstractions, templates, pipeline. |
| [docs/generated-solution.md](docs/generated-solution.md) | Everything the generator produces, layer by layer. |
| [docs/distribution.md](docs/distribution.md) | Single-file publish, cross-platform builds, global tool packaging. |

## Layout

```text
monorepo-dotnet/
├── src/
│   ├── MonorepoDotnet.CLI/             # Interactive prompts + composition root
│   ├── MonorepoDotnet.Core/            # Models, interfaces and generator use cases
│   └── MonorepoDotnet.Infrastructure/  # File system, templates, process runner
├── templates/
│   ├── DDD/                            # DDD project templates + shared Docker/root files
│   ├── Clean/                          # Clean-specific overrides (domain, mappings, README)
│   ├── SingleLayer/                    # 1-layer templates
│   └── Mvc/                            # MVC templates (controllers, views, view models)
├── docs/
├── Directory.Build.props
├── global.json
├── MonorepoDotnet.sln
└── README.md
```

`Core` has no dependency on `Infrastructure` or `CLI`; `Infrastructure`
implements `Core` abstractions; `CLI` is the only composition root.

## Build

```bash
dotnet build MonorepoDotnet.sln
```

## Publish

```powershell
dotnet publish src/MonorepoDotnet.CLI -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o artifacts/win-x64
```

See [docs/distribution.md](docs/distribution.md) for other platforms and global
tool packaging.

## Notes

- Solution names must match `^[A-Za-z][A-Za-z0-9_]*$` (max 64 characters).
- The output directory and the complete `{OutputDirectory}/{SolutionName}`
  path are validated at the prompt: invalid characters, reserved Windows names,
  missing drive roots, file collisions, length limits and non-empty targets are
  rejected before generation starts.
- The generated `.env` contains a random database password and is git-ignored.
  SQL Server passwords are generated to satisfy its complexity policy.
- Migrations are never applied automatically; opt in with
  `dotnet ef database update` or `APPLY_MIGRATIONS=true`.
- Test project generation is out of scope for this MVP.
