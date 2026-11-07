# Project: monorepo-dotnet

You are a Senior .NET Architect and Engineer. Build a maintainable, extensible CLI tool named **monorepo-dotnet** that automatically generates ASP.NET Core solutions following DDD architecture, PostgreSQL, Entity Framework Core Code First, and Docker.

This file is the source of truth for product goals, architecture, phases, coding standards, and definition of done. Read it before changing code. After each phase, update **Current status** below.

## Current status

**Completed:** Phases 1–8.

- Phase 1 — Foundation: three-project solution, DI, `ProcessRunner`, file system, template provider/renderer, environment and name validation.
- Phase 2 — DDD generator: 53 embedded templates, `DddTemplateManifest`, `DddSolutionGenerator`, solution/project creation, restore, build.
- Phase 3 — PostgreSQL/EF Core: `ApplicationDbContext`, entity configuration, repository, design-time factory, migrations folder, Npgsql DI, automatic `InitialCreate`.
- Phase 4 — Docker: multi-stage `Dockerfile`, Compose with PostgreSQL/Redis, healthchecks, volumes, network, `.env`/`.env.example`.
- Phase 5 — Interactive CLI: Spectre.Console prompts, environment report, overwrite guard, summary + confirmation, progress, success/failure panels, exit codes.
- Phase 6 — Distribution: single-file `win-x64` publish verified, embedded templates confirmed inside the published executable, global-tool packaging configured.
- Phase 7 — Architectures and path safety: DDD, Clean Architecture, 1-layer and MVC generators; `SolutionLayouts`; architecture-specific manifests/templates; `OutputPathValidator` for prompt, environment and generator checks.
- Phase 8 — SQL Server: database prompt offers PostgreSQL and SQL Server; per-provider EF Core package, `UseNpgsql`/`UseSqlServer` wiring, connection strings, Compose service (`postgres`/`sqlserver`), `.env` keys and SQL Server password handling via `Database:Password`.

**Verified end to end:** generation for all four architectures with both databases, with all five optional packages and with none (plus Docker on/off), `dotnet build` for every combination, `dotnet ef migrations add` for every combination (DDD/Clean `Persistence/Migrations`, 1-layer/MVC `Data/Migrations`; SQL Server migrations use SQL Server column types), `docker compose config` for both providers, and path validation for invalid characters, reserved names, missing drives, file collisions and non-empty targets. DDD Docker smoke tests (`docker compose up`, `/health`, `GET/POST /api/products`, Redis) remain supported; the original DDD end-to-end scenarios were run against PostgreSQL.

**Not implemented (out of MVP scope):** test project generation, additional databases/frameworks, non-interactive/CI mode.

## 1. Technology Stack

**Generator:**
- .NET 10 / C#
- Spectre.Console for interactive CLI.
- Microsoft.Extensions.DependencyInjection.
- Microsoft.Extensions.Logging.
- System.Diagnostics.Process for executing CLI commands.
- Template-based code generation.

**Generated projects:**
- ASP.NET Core 10 Web API (or MVC).
- Domain-Driven Design (DDD), Clean Architecture, 1-layer or MVC layout.
- Entity Framework Core.
- PostgreSQL (Npgsql) or SQL Server (Microsoft.EntityFrameworkCore.SqlServer).
- Docker and Docker Compose.

Use compatible, stable package versions.

## 2. Generator Architecture

Use a lightweight layered architecture:

```text
monorepo-dotnet/
├── src/
│   ├── MonorepoDotnet.CLI/
│   ├── MonorepoDotnet.Core/
│   └── MonorepoDotnet.Infrastructure/
│
├── templates/
│   ├── DDD/
│   │   ├── API/
│   │   ├── Application/
│   │   ├── Domain/
│   │   ├── Infrastructure/
│   │   └── Docker/
│   ├── Clean/
│   ├── SingleLayer/
│   └── Mvc/
│
├── docs/
├── Directory.Build.props
├── global.json
├── .gitignore
├── README.md
└── MonorepoDotnet.sln
```

Responsibilities:
- **CLI:** Interactive prompts, workflow orchestration and dependency injection.
- **Core:** Models, enums, interfaces and generator use cases.
- **Infrastructure:** File system, templates and external process execution.

Rules:
- Core must not depend on Infrastructure or CLI.
- Infrastructure implements Core interfaces.
- Keep architecture simple; avoid unnecessary abstractions.
- All external commands must use a reusable ProcessRunner.

## 3. Interactive CLI Workflow

Use Spectre.Console to create a professional interactive terminal experience.

The CLI MUST ask the user for the following configuration:

1. Solution name (required, no hardcoded default).
2. Output directory.
3. .NET version (default .NET 10).
4. Architecture (DDD, Clean Architecture, 1-layer or MVC).
5. Database (PostgreSQL or SQL Server for MVP).
6. Docker (default Yes).
7. Generate Unit Tests and Integration Tests (default No).
8. Optional packages:
   - Serilog
   - FluentValidation
   - AutoMapper
   - Redis
   - MediatR

Only display options that are actually implemented.

### Solution Naming Rules

The solution name is the single source of truth for all generated names.

For example, if the user enters `CongThongTin`, generate:

```text
CongThongTin.sln
CongThongTin.API
CongThongTin.Application
CongThongTin.Domain
CongThongTin.Infrastructure
```

Use the solution name consistently in:
- Project names.
- Namespaces.
- Docker service and image names.
- Generated configurations.

Validate names and reject empty or invalid values.

Ask the user to confirm the configuration before generating files. Never silently overwrite existing files.

## 4. Generated DDD Architecture

Generate this structure:

```text
{SolutionName}/
├── src/
│   ├── {SolutionName}.API/
│   ├── {SolutionName}.Application/
│   ├── {SolutionName}.Domain/
│   └── {SolutionName}.Infrastructure/
│
├── docker-compose.yml
├── Dockerfile
├── .dockerignore
├── .gitignore
└── {SolutionName}.sln
```

Project responsibilities:

**Domain**
- Entities
- ValueObjects
- Enums
- DomainEvents
- Domain Exceptions
- Repository interfaces where appropriate

**Application**
- UseCases
- DTOs
- Interfaces
- Application Services
- Dependency Injection

**Infrastructure**
- Persistence
- ApplicationDbContext
- Entity Configurations
- EF Core Migrations
- Repository Implementations
- Dependency Injection

**API**
- Controllers or endpoints
- Middleware
- Extensions
- Configuration
- Program.cs

Project dependencies:

```text
API -> Application, Infrastructure

Infrastructure -> Application, Domain

Application -> Domain

Domain -> No project dependencies
```

Keep business logic inside the appropriate DDD layers.

## 5. PostgreSQL, SQL Server and EF Core

Automatically configure and install:

- Microsoft.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.Design
- Npgsql.EntityFrameworkCore.PostgreSQL (PostgreSQL)
- Microsoft.EntityFrameworkCore.SqlServer (SQL Server)

Generate:
- ApplicationDbContext.
- Connection string for the selected provider.
- Dependency injection configuration.
- Entity configuration structure.
- Migration directory.

SQL Server passwords are supplied through `Database:Password` / the
`Database__Password` environment variable and merged with
`SqlConnectionStringBuilder`; they are never written to `appsettings.json`.

Support EF Core commands:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Use correct project and startup-project paths.

Do not automatically apply migrations unless explicitly requested.

## 6. Docker Integration

Generate:
- Multi-stage Dockerfile.
- docker-compose.yml.
- .dockerignore.
- .env.example.
- PostgreSQL (`postgres`) or SQL Server (`sqlserver`) service.
- Persistent database volume.
- Docker networking.

Ensure the API connects to the database using the Docker Compose service name (`postgres` or `sqlserver`) rather than localhost.

Never hardcode database passwords. Use environment variables.

Docker must be optional.

## 7. Process Runner and Environment Validation

Implement a reusable asynchronous ProcessRunner with:
- ArgumentList instead of unsafe shell command concatenation.
- Standard output and error capture.
- Exit code validation.
- CancellationToken support.
- Useful error reporting.

Validate:
- Installed .NET SDK.
- Compatible SDK version.
- Docker CLI availability.
- Docker Engine availability when needed.
- Output directory permissions.
- Solution name validity.

If Docker is unavailable, allow generating Docker files without starting containers.

## 8. Development Phases

Implement incrementally:

**Phase 1 — Foundation**
- Initialize Generator solution.
- Create CLI, Core and Infrastructure.
- Configure project references and DI.
- Implement ProcessRunner.
- Implement environment validation.

**Phase 2 — DDD Generator**
- Create reusable DDD templates.
- Generate solution and projects.
- Configure project references.
- Generate standard source files.

**Phase 3 — PostgreSQL and EF Core**
- Install NuGet packages.
- Generate DbContext.
- Configure PostgreSQL.
- Configure migrations.

**Phase 4 — Docker**
- Generate Dockerfile.
- Generate Compose.
- Generate environment configuration.

**Phase 5 — CLI**
- Implement interactive workflow.
- Add progress indicators.
- Add configuration confirmation.
- Add error handling.

**Phase 6 — Distribution**
- Publish standalone Windows EXE.
- Ensure templates are included in published output.
- Prepare optional .NET Global Tool support.

**Phase 7 — Architectures and Path Safety**
- Add Clean Architecture, 1-layer and MVC generation options.
- Generalize solution layout, Docker and EF migration handling per architecture.
- Validate the output directory and complete solution path at the prompt and in the generator.

**Phase 8 — SQL Server**
- Offer PostgreSQL and SQL Server in the database prompt.
- Generate the matching EF Core provider, connection string, Compose service and `.env` keys.
- Handle SQL Server authentication without storing the password in `appsettings.json`.

## 9. Coding Standards

- Follow SOLID where appropriate.
- Enable nullable reference types.
- Use implicit usings.
- Use asynchronous I/O.
- Use dependency injection.
- Keep code modular and maintainable.
- Avoid unnecessary dependencies.
- Do not create tests in this MVP.
- Do not leave essential functionality as TODOs.
- Keep documentation synchronized with implementation.

## 10. Definition of Done

The MVP is complete when users can:

1. Run the CLI.
2. Enter a custom solution name.
3. Select an output directory.
4. Generate a DDD ASP.NET Core solution.
5. Configure PostgreSQL and EF Core.
6. Generate Docker configuration.
7. Restore NuGet packages.
8. Build successfully.
9. Run the generated application using Docker Compose.
10. Create and apply EF Core migrations.

## 11. Instructions to AI Agent

First inspect the existing repository.

If empty, initialize the project from scratch.

Work incrementally, starting with Phase 1. Do not implement everything in one operation.

After each phase:
- Build the Generator.
- Fix compilation errors.
- Verify functionality.
- Update documentation.
- Update **Current status** in this file.
- Summarize completed work and remaining tasks.

Do not generate non-functional placeholders or claim success without verification.

Prioritize a working, maintainable MVP over unnecessary complexity.
