# monorepo-dotnet

`monorepo-dotnet` is an interactive CLI for quickly creating ASP.NET Core
solutions with a clean architecture, database, Docker, and commonly used
packages already configured.

Instead of creating projects, wiring references, installing packages, writing a
`DbContext`, and configuring Docker by hand, answer a few prompts. The tool
generates the solution, restores its packages, builds it, and creates the first
EF Core migration.

## Quick start

```bash
dotnet tool install --global monorepo-dotnet
monorepo
```

Remove the global tool:

```bash
dotnet tool uninstall --global monorepo-dotnet
```

Choose the solution name, .NET version, architecture, database, and packages.
When generation finishes:

```bash
cd <output-directory>/<SolutionName>
docker compose up --build
curl http://localhost:8080/health
```

Run from source:

```bash
dotnet run --project src/MonorepoDotnet.CLI
```

Publish a self-contained Windows executable to `artifacts/win-x64`:

```powershell
dotnet publish src/MonorepoDotnet.CLI `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -o artifacts/win-x64
```

You need a .NET 8, 9, or 10 SDK matching the selected target. If it is missing,
the CLI shows the official Microsoft installation command and can run it for
you. Docker is only required to run the containers. `dotnet-ef` is optional;
without it, the solution is still generated and the CLI explains how to create
the migration later.

## What problem does it solve?

- Cuts down the boilerplate setup time for a new ASP.NET Core project.
- Prevents mismatched project references, package versions, and layer
  configuration.
- Gives the whole team a consistent solution structure.
- Creates a ready-to-use local Docker environment without hand-written Compose
  configuration.
- Includes a working sample for testing the complete API-to-database flow.

## Key features

- **.NET 8, 9, and 10** — package versions and Docker images automatically
  match the selected target framework.
- **Four architectures** — DDD, Clean Architecture, 1-Layer API, and MVC.
- **EF Core Code First** — PostgreSQL or SQL Server with
  `ApplicationDbContext` and a design-time factory. Migrations are not created
  during generation.
- **Optional Docker setup** — generates a multi-stage Dockerfile and Docker
  Compose configuration with a database, volume, health check, and environment
  variables.
- **Packages on Domain** — DDD and Clean Architecture put every package
  reference on the Domain project: EF Core, EF Tools, Configuration, JWT
  Bearer, OpenAPI, Swagger UI, OData, and FluentValidation.AspNetCore.
  1-layer and MVC keep those packages on the single project.
- **Fully wired optional packages** — Serilog, FluentValidation, AutoMapper,
  Redis, and MediatR, not just package references in the `.csproj`.
- **Sample Product API** — list, get, and create endpoints for an end-to-end
  smoke test; the MVC template also includes Razor Views.
- **Safe generation** — validates the solution name, output path, and SDK before
  writing files, and never overwrites a non-empty directory.
- **Secret protection** — database passwords stay in `.env`, not
  `appsettings.json`.

## Documentation

| Document | Description |
| --- | --- |
| [Usage guide](docs/usage.md) | Prompts, validation, exit codes, and troubleshooting. |
| [Generated solution](docs/generated-solution.md) | Structure and contents of each architecture. |
| [Generator architecture](docs/architecture.md) | How the tool is organized and works. |
| [Distribution](docs/distribution.md) | Single-file publishing and the NuGet global tool. |
