# monorepo-dotnet

Interactive CLI that generates production-ready ASP.NET Core solutions using
Domain-Driven Design (DDD), Entity Framework Core, PostgreSQL and Docker.

The generator asks a few questions, then writes a complete solution, restores
packages, builds it and creates the initial EF Core migration.

```text
┌─────────────────────────────────────────────────────────────┐
│  monorepo-dotnet                                            │
│  ASP.NET Core DDD solution generator — PostgreSQL, EF, Docker│
└─────────────────────────────────────────────────────────────┘
  Solution name      CongThongTin
  Output directory   D:\projects
  Target framework   net10.0
  Architecture       DDD (Domain-Driven Design)
  Database           PostgreSQL (EF Core, Code First)
  Docker             Yes
  Optional packages  Serilog, FluentValidation
```

## Requirements

- .NET SDK 10.0.100 or later (`dotnet --list-sdks`)
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

## What gets generated

```text
{SolutionName}/
├── src/
│   ├── {SolutionName}.API/             # Controllers, middleware, Program.cs
│   ├── {SolutionName}.Application/     # Use cases, DTOs, validators
│   ├── {SolutionName}.Domain/          # Entities, value objects, domain events
│   └── {SolutionName}.Infrastructure/  # EF Core, repositories, migrations
├── docker-compose.yml                  # API + PostgreSQL (+ Redis)
├── Dockerfile                          # Multi-stage, non-root runtime
├── .dockerignore
├── .env                                # Generated with a random password (git-ignored)
├── .env.example
├── .gitignore
├── Directory.Build.props
├── README.md
└── {SolutionName}.sln
```

Dependencies: `API → Application + Infrastructure`,
`Infrastructure → Application + Domain`, `Application → Domain`,
`Domain → none`.

## Features

- **Interactive Spectre.Console UI** — validated prompts, environment report,
  configuration summary, confirmation, live progress.
- **Safe by default** — never overwrites existing files; deletes a failed
  generation directory; `Ctrl+C` cancels cleanly.
- **DDD structure** — aggregate root, value objects, domain events, repository
  interfaces, application services and handlers.
- **EF Core + PostgreSQL** — `ApplicationDbContext`, entity configurations,
  design-time factory, `InitialCreate` migration, Npgsql with retry.
- **Docker** — multi-stage `Dockerfile`, Compose with PostgreSQL, persistent
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
├── templates/DDD/                      # Embedded DDD project templates
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
- The generated `.env` contains a random database password and is git-ignored.
- Migrations are never applied automatically; opt in with
  `dotnet ef database update` or `APPLY_MIGRATIONS=true`.
- Test project generation is out of scope for this MVP.
