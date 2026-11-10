# {{SolutionName}}

Standard backend from `docs/backend-structure.md`.

```
{{ApiProject}}
        │
        ▼
{{ServiceProject}}
        │
        ▼
{{InfrastructureProject}}
        │
        ▼
{{DomainProject}}
```

Solution file: `{{SolutionName}}.slnx`. Target framework: `{{TargetFramework}}`. Database: {{Database}}.

A request goes `ProductController` → `IProductService` → `IUnitOfWork` → `{{SolutionName}}DbContext` → {{Database}}.

The API applies EF Core migrations on startup. Add one from the solution root when the model changes:

```bash
dotnet ef migrations add InitialCreate --project src/{{DomainProject}} --startup-project src/{{ApiProject}} --output-dir Migrations
```

{{#if Docker}}
```bash
docker compose up --build
```
{{/if}}
Swagger UI is served at `/`.
