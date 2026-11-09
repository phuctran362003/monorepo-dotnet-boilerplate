# monorepo-dotnet

Interactive CLI that generates an ASP.NET Core solution, restores packages, builds it, and creates the initial EF Core migration.

## Quick start

```bash
dotnet tool install --global monorepo-dotnet
monorepo
```

The CLI asks for the solution name, output directory, .NET version, architecture, database, Docker, and optional packages. It never overwrites a non-empty folder. When generation finishes:

```bash
cd <output-directory>/<SolutionName>
docker compose up --build
curl http://localhost:8080/health
```

Run from source:

```bash
dotnet run --project src/MonorepoDotnet.CLI
```

Needs a .NET SDK for the target you pick (8, 9, or 10). If it is missing, the CLI prints the Microsoft install command and can run it. Docker is only required to start the generated containers. `dotnet-ef` is optional; without it the CLI still generates the solution and tells you how to add the migration later.

## Features

- **.NET 8, 9, or 10.** Package versions and Docker image tags follow the selected target framework.
- **Four architectures.** DDD, Clean Architecture, a single-project API, or MVC. DDD and Clean use Domain, Application, Infrastructure, and API/WebApi. The other two are one web project.
- **EF Core Code First.** PostgreSQL or SQL Server, `ApplicationDbContext`, design-time factory, and an `InitialCreate` migration. Passwords stay in `.env`, not in `appsettings.json`.
- **Docker.** Optional multi-stage Dockerfile and Compose with the selected database, a volume, a healthcheck, and a service-name host (`postgres` or `sqlserver`).
- **Baseline packages.** Domain gets EF Core, EF Core Tools, Configuration, and JWT Bearer (SQL Server provider only when SQL Server is selected). Application gets FluentValidation.AspNetCore. API gets OData. 1-layer and MVC put the same packages on the single project.
- **Optional packages, fully wired.** Serilog, FluentValidation, AutoMapper, Redis, and MediatR.
- **Sample Product API.** List, get, and create, so the generated app runs end to end. MVC also includes Razor views.
- **Safe prompts.** Names, paths, and SDKs are checked before any file is written. A failed run deletes the directory it created.

## Docs

| Document | Contents |
| --- | --- |
| [docs/usage.md](docs/usage.md) | Prompts, validation, exit codes, troubleshooting. |
| [docs/generated-solution.md](docs/generated-solution.md) | What each architecture produces. |
| [docs/architecture.md](docs/architecture.md) | How the generator is structured. |
| [docs/distribution.md](docs/distribution.md) | Single-file publish and the NuGet global tool. |
