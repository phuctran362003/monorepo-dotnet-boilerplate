# Generated solution

Every generated solution is a self-contained ASP.NET Core 10 application.
This page describes the DDD layout in detail; the differences for Clean
Architecture, 1-layer and MVC are summarized below.

## Architecture variants

| Architecture | Projects | Notes |
| --- | --- | --- |
| DDD | `API`, `Application`, `Domain`, `Infrastructure` | Aggregates, value objects and domain events. |
| Clean | `WebApi`, `Application`, `Domain`, `Infrastructure` | Simple entity model; dependencies point inward. |
| 1-Layer | `{SolutionName}` | One web project: `Controllers`, `Products`, `Models`, `Data`. |
| MVC | `{SolutionName}.Web` | One web project with `Controllers`, `Views`, `ViewModels`, `Services`, `Data`. |

All variants share the root files, database/EF Core setup, Docker templates
and optional-package wiring. The sections below use the DDD names
(`{SolutionName}.API`, `Persistence/Migrations`); for Clean use
`{SolutionName}.WebApi`, and for 1-layer/MVC use the single main project with
`Data/Migrations` as the migration output directory.

## Layout

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

## Project dependencies

```text
API ──► Application, Infrastructure
Infrastructure ──► Application, Domain
Application ──► Domain
Domain ──► (no project references)
```

## Domain

No project references. The project file pins EF Core, EF Core Tools, configuration and JWT Bearer. `Microsoft.EntityFrameworkCore.SqlServer` is added only when SQL Server is selected. Package versions follow the chosen target framework.

| Folder | Contents |
| --- | --- |
| `Common/` | `Entity`, `AggregateRoot`, `ValueObject`, `DomainEvent`, `IDomainEvent`. |
| `Entities/` | `Product` aggregate with encapsulated state and behavior. |
| `ValueObjects/` | `ProductName` value object. |
| `Enums/` | `ProductStatus`. |
| `DomainEvents/` | `ProductCreatedDomainEvent`. |
| `Exceptions/` | `DomainException`. |
| `Repositories/` | `IProductRepository` (domain-level persistence contract). |

## Application

Use cases, DTOs and application-level abstractions. References Domain only.
The project file always includes `FluentValidation.AspNetCore`.

| Folder | Contents |
| --- | --- |
| `Products/` | `IProductService`, `ProductService`. |
| `Products/Commands/` | `CreateProductCommand` and handler. |
| `Products/Queries/` | `GetProductsQuery`, `GetProductByIdQuery` and handlers. |
| `Products/Dtos/` | `ProductDto`, `CreateProductRequest`. |
| `Products/Validators/` | `CreateProductRequestValidator` (only with FluentValidation). |
| `Products/MappingProfile.cs` | AutoMapper profile mapping `Product` to `ProductDto` (only with AutoMapper). |
| `Common/Interfaces/` | `IUnitOfWork`. |
| `Common/Caching/` | `ICacheService` (only with Redis). |
| `DependencyInjection.cs` | `AddApplication(IConfiguration)`. |

The sample `Product` slice exists to prove the wiring end to end; replace it
with your own bounded contexts.

## Infrastructure

| Folder | Contents |
| --- | --- |
| `Persistence/` | `ApplicationDbContext`. |
| `Persistence/Configurations/` | `ProductConfiguration` (`IEntityTypeConfiguration<Product>`). |
| `Persistence/Repositories/` | `ProductRepository` implementing `IProductRepository`. |
| `Persistence/Migrations/` | EF Core migration folder (`InitialCreate` when the EF tool is available). |
| `Persistence/ApplicationDbContextFactory.cs` | Design-time factory so `dotnet ef` works without a running database. |
| `Caching/` | `RedisCacheService` (only with Redis). |
| `DependencyInjection.cs` | `AddInfrastructure(IConfiguration)`; registers the DbContext with `UseNpgsql` and retry-on-failure. |

## API

| Folder | Contents |
| --- | --- |
| `Program.cs` | Minimal hosting model, middleware, controllers, `/health`, optional Serilog and optional migration application. |
| Project file | `Microsoft.AspNetCore.OData` in addition to OpenAPI. |
| `Controllers/` | `ProductsController`. |
| `Middleware/` | `ExceptionHandlingMiddleware` mapping domain exceptions to problem details. |
| `Extensions/` | `AddApiServices()`. |
| `Filters/` | `ValidationFilter` (only with FluentValidation). |
| `appsettings.json` | Logging, connection string template, `Database:ApplyMigrations`, optional `AutoMapper:LicenseKey`. |

### Endpoints

| Method | Route | Description |
| --- | --- | --- |
| GET | `/health` | Health probe returning status and service name. |
| GET | `/api/products` | List products. |
| GET | `/api/products/{id}` | Get one product. |
| POST | `/api/products` | Create a product. |

OpenAPI is mapped in Development at `/openapi/v1.json`.

## Optional packages

| Package | What the generator wires |
| --- | --- |
| Serilog | `Serilog.AspNetCore`; `builder.Host.UseSerilog` reading from configuration, request logging, console/file sinks. |
| FluentValidation | `FluentValidation.DependencyInjectionExtensions`; `AddValidatorsFromAssembly`, `CreateProductRequestValidator`, `ValidationFilter` for automatic 400 responses. |
| AutoMapper | `AutoMapper`; `MappingProfile` registered via `AddAutoMapper`, `IMapper` injected into `ProductService`. Reads an optional `AutoMapper:LicenseKey` from configuration. |
| Redis | `StackExchange.Redis` + `Microsoft.Extensions.Caching.StackExchangeRedis`; `ICacheService`/`RedisCacheService`, `ConnectionStrings:Redis`, `redis` compose service. |
| MediatR | `MediatR`; command/query handlers and `AddMediatR` registration. |

Pinned versions live in
[`DotNetVersionCatalog.cs`](../src/MonorepoDotnet.Core/Generation/DotNetVersionCatalog.cs)
(per target framework) and
[`PackageVersions.cs`](../src/MonorepoDotnet.Core/Generation/PackageVersions.cs)
(shared packages) and are substituted into every template, so all generated
projects agree.

## PostgreSQL or SQL Server and EF Core

The connection string is read from `ConnectionStrings:Default`:

```text
# PostgreSQL
Host=localhost;Port=5432;Database={docker-name};Username=postgres;Password=

# SQL Server
Server=localhost,1433;Database={docker-name};User Id=sa;TrustServerCertificate=True;Encrypt=False
```

- In Docker Compose the API receives the service name (`postgres` or
  `sqlserver`) plus credentials from `.env`; `localhost` is never used inside a
  container.
- `appsettings.json` intentionally leaves the password empty. The real password
  is generated once into `.env`, which is git-ignored.
- SQL Server passwords come from `Database:Password` (`Database__Password`
  environment variable) and are merged into the connection string with
  `SqlConnectionStringBuilder`, so the secret never has to live in
  `appsettings.json`.
- `ApplicationDbContextFactory` supplies a placeholder connection string at
  design time (and reads `Database__Password`), so `dotnet ef migrations add`
  does not need a running database.

Commands, run from the solution root:

```bash
dotnet ef migrations add InitialCreate \
  --project src/{SolutionName}.Infrastructure \
  --startup-project src/{SolutionName}.API \
  --output-dir Persistence/Migrations

dotnet ef database update \
  --project src/{SolutionName}.Infrastructure \
  --startup-project src/{SolutionName}.API
```

Migrations are never applied automatically during generation. Inside Docker,
set `APPLY_MIGRATIONS=true` to opt in to `Database.MigrateAsync()` at startup.

## Docker

`Dockerfile` is multi-stage: `sdk:{8|9|10}.0` restore/publish matching the
selected target framework, then `aspnet:{8|9|10}.0` runtime as a non-root user
(`USER $APP_UID`) listening on port 8080.

`docker-compose.yml` defines:

- `api` — built from the local `Dockerfile`, waits for healthy dependencies.
- `postgres` — `postgres:17-alpine`, healthcheck, named volume (PostgreSQL only).
- `sqlserver` — `mcr.microsoft.com/mssql/server:2022-latest`, `Developer` edition,
  SQL login healthcheck, named volume (SQL Server only).
- `redis` — `redis:7-alpine` with AOF, only when Redis is selected.
- A bridge network and named volumes for persistence.
- All secrets and ports come from `.env`.

`.env.example` documents every variable. `.env` is generated with a random
48-character hexadecimal password and is listed in `.gitignore`. SQL Server
passwords get a `Aa!` suffix so they satisfy the SQL Server complexity policy.

The Compose project name is the lower-cased solution name
(`name: {docker-name}`), so containers, networks and volumes are prefixed with
it (`{docker-name}-api-1`, `{docker-name}_{docker-name}-postgres-data`,
`{docker-name}_{docker-name}-sqlserver-data`, …). No `container_name` is set,
which lets you run more than one stack without name clashes.

```bash
docker compose up --build
docker compose exec api sh                              # shell inside the API container
docker compose exec postgres psql -U postgres -d {docker-name}   # PostgreSQL
docker compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa  # SQL Server
docker compose exec redis redis-cli                     # only with Redis
```

The API is exposed at `http://localhost:8080` by default.

## Build and run locally

```bash
dotnet restore
dotnet build
dotnet run --project src/{SolutionName}.API
```

Set `ConnectionStrings__Default` as an environment variable (double underscore
is the configuration hierarchy separator) or edit
`appsettings.Development.json`.
