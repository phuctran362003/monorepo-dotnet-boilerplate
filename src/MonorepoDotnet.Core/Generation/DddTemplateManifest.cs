using MonorepoDotnet.Core.Models;

namespace MonorepoDotnet.Core.Generation;

/// <summary>
/// Declarative list of every file produced by the DDD generator. Keeping this
/// in one place makes the generated layout easy to review.
/// </summary>
public static class DddTemplateManifest
{
    public static IReadOnlyList<TemplateFile> All { get; } =
    [
        // Root
        new("DDD/Root/Directory.Build.props.tpl", "Directory.Build.props"),
        new("DDD/Root/.gitignore.tpl", ".gitignore"),
        new("DDD/Root/.editorconfig.tpl", ".editorconfig"),
        new("DDD/Root/README.md.tpl", "README.md"),

        // Domain
        new("DDD/Domain/Project.csproj.tpl", "src/{{DomainProject}}/{{DomainProject}}.csproj"),
        new("DDD/Domain/Common/Entity.cs.tpl", "src/{{DomainProject}}/Common/Entity.cs"),
        new("DDD/Domain/Common/AggregateRoot.cs.tpl", "src/{{DomainProject}}/Common/AggregateRoot.cs"),
        new("DDD/Domain/Common/ValueObject.cs.tpl", "src/{{DomainProject}}/Common/ValueObject.cs"),
        new("DDD/Domain/Common/IDomainEvent.cs.tpl", "src/{{DomainProject}}/Common/IDomainEvent.cs"),
        new("DDD/Domain/Common/DomainEvent.cs.tpl", "src/{{DomainProject}}/Common/DomainEvent.cs"),
        new("DDD/Domain/Exceptions/DomainException.cs.tpl", "src/{{DomainProject}}/Exceptions/DomainException.cs"),
        new("DDD/Domain/Enums/ProductStatus.cs.tpl", "src/{{DomainProject}}/Enums/ProductStatus.cs"),
        new("DDD/Domain/Entities/Product.cs.tpl", "src/{{DomainProject}}/Entities/Product.cs"),
        new("Persistence/BaseEntity.cs.tpl", "src/{{DomainProject}}/Entities/BaseEntity.cs"),
        new("DDD/Domain/ValueObjects/ProductName.cs.tpl", "src/{{DomainProject}}/ValueObjects/ProductName.cs"),
        new("DDD/Domain/DomainEvents/ProductCreatedDomainEvent.cs.tpl", "src/{{DomainProject}}/DomainEvents/ProductCreatedDomainEvent.cs"),
        new("Persistence/IGenericRepository.cs.tpl", "src/{{DomainProject}}/Repositories/IGenericRepository.cs"),

        // Application
        new("DDD/Application/Project.csproj.tpl", "src/{{ApplicationProject}}/{{ApplicationProject}}.csproj"),
        new("DDD/Application/DependencyInjection.cs.tpl", "src/{{ApplicationProject}}/DependencyInjection.cs"),
        new("Persistence/IUnitOfWork.cs.tpl", "src/{{ApplicationProject}}/Common/Interfaces/IUnitOfWork.cs"),
        new("DDD/Application/Products/Dtos/ProductDto.cs.tpl", "src/{{ApplicationProject}}/Products/Dtos/ProductDto.cs"),
        new("DDD/Application/Products/Dtos/CreateProductRequest.cs.tpl", "src/{{ApplicationProject}}/Products/Dtos/CreateProductRequest.cs"),
        new("DDD/Application/Products/IProductService.cs.tpl", "src/{{ApplicationProject}}/Products/IProductService.cs"),
        new("DDD/Application/Products/ProductService.cs.tpl", "src/{{ApplicationProject}}/Products/ProductService.cs"),
        new("DDD/Application/Products/MappingProfile.cs.tpl", "src/{{ApplicationProject}}/Products/MappingProfile.cs", OptionalPackage.AutoMapper),
        new("DDD/Application/Products/Validators/CreateProductRequestValidator.cs.tpl", "src/{{ApplicationProject}}/Products/Validators/CreateProductRequestValidator.cs", OptionalPackage.FluentValidation),
        new("DDD/Application/Products/Commands/CreateProductCommand.cs.tpl", "src/{{ApplicationProject}}/Products/Commands/CreateProductCommand.cs", OptionalPackage.MediatR),
        new("DDD/Application/Products/Commands/CreateProductCommandHandler.cs.tpl", "src/{{ApplicationProject}}/Products/Commands/CreateProductCommandHandler.cs", OptionalPackage.MediatR),
        new("DDD/Application/Products/Queries/GetProductsQuery.cs.tpl", "src/{{ApplicationProject}}/Products/Queries/GetProductsQuery.cs", OptionalPackage.MediatR),
        new("DDD/Application/Products/Queries/GetProductsQueryHandler.cs.tpl", "src/{{ApplicationProject}}/Products/Queries/GetProductsQueryHandler.cs", OptionalPackage.MediatR),
        new("DDD/Application/Products/Queries/GetProductByIdQuery.cs.tpl", "src/{{ApplicationProject}}/Products/Queries/GetProductByIdQuery.cs", OptionalPackage.MediatR),
        new("DDD/Application/Products/Queries/GetProductByIdQueryHandler.cs.tpl", "src/{{ApplicationProject}}/Products/Queries/GetProductByIdQueryHandler.cs", OptionalPackage.MediatR),
        new("DDD/Application/Common/Caching/ICacheService.cs.tpl", "src/{{ApplicationProject}}/Common/Caching/ICacheService.cs", OptionalPackage.Redis),

        // Infrastructure
        new("DDD/Infrastructure/Project.csproj.tpl", "src/{{InfrastructureProject}}/{{InfrastructureProject}}.csproj"),
        new("DDD/Infrastructure/DependencyInjection.cs.tpl", "src/{{InfrastructureProject}}/DependencyInjection.cs"),
        new("DDD/Infrastructure/Persistence/ApplicationDbContext.cs.tpl", "src/{{InfrastructureProject}}/Persistence/ApplicationDbContext.cs"),
        new("DDD/Infrastructure/Persistence/Configurations/ProductConfiguration.cs.tpl", "src/{{InfrastructureProject}}/Persistence/Configurations/ProductConfiguration.cs"),
        new("Persistence/GenericRepository.cs.tpl", "src/{{InfrastructureProject}}/Persistence/Repositories/GenericRepository.cs"),
        new("Persistence/UnitOfWork.cs.tpl", "src/{{InfrastructureProject}}/Persistence/UnitOfWork.cs"),
        new("DDD/Infrastructure/Persistence/ApplicationDbContextFactory.cs.tpl", "src/{{InfrastructureProject}}/Persistence/ApplicationDbContextFactory.cs"),
        new("DDD/Infrastructure/Persistence/Migrations/.gitkeep.tpl", "src/{{InfrastructureProject}}/Persistence/Migrations/.gitkeep"),
        new("DDD/Infrastructure/Caching/RedisCacheService.cs.tpl", "src/{{InfrastructureProject}}/Caching/RedisCacheService.cs", OptionalPackage.Redis),

        // API
        new("DDD/API/Project.csproj.tpl", "src/{{ApiProject}}/{{ApiProject}}.csproj", Host: ProjectKind.WebApi),
        new("DDD/API/Program.cs.tpl", "src/{{ApiProject}}/Program.cs", Host: ProjectKind.WebApi),
        new("DDD/API/Controllers/ProductsController.cs.tpl", "src/{{ApiProject}}/Controllers/ProductsController.cs", Host: ProjectKind.WebApi),
        new("DDD/API/Middleware/ExceptionHandlingMiddleware.cs.tpl", "src/{{ApiProject}}/Middleware/ExceptionHandlingMiddleware.cs", Host: ProjectKind.WebApi),
        new("DDD/API/Extensions/ServiceCollectionExtensions.cs.tpl", "src/{{ApiProject}}/Extensions/ServiceCollectionExtensions.cs", Host: ProjectKind.WebApi),
        new("DDD/API/Filters/ValidationFilter.cs.tpl", "src/{{ApiProject}}/Filters/ValidationFilter.cs", OptionalPackage.FluentValidation, Host: ProjectKind.WebApi),
        new("DDD/API/appsettings.json.tpl", "src/{{ApiProject}}/appsettings.json"),
        new("DDD/API/appsettings.Development.json.tpl", "src/{{ApiProject}}/appsettings.Development.json"),
        new("DDD/API/Properties/launchSettings.json.tpl", "src/{{ApiProject}}/Properties/launchSettings.json", Host: ProjectKind.WebApi),

        // Console host
        new("Console/Host/Project.csproj.tpl", "src/{{ApiProject}}/{{ApiProject}}.csproj", Host: ProjectKind.Console),
        new("Console/Host/Program.cs.tpl", "src/{{ApiProject}}/Program.cs", Host: ProjectKind.Console),
        new("Console/Host/Properties/launchSettings.json.tpl", "src/{{ApiProject}}/Properties/launchSettings.json", Host: ProjectKind.Console),

        // Docker
        new("DDD/Docker/Dockerfile.tpl", "Dockerfile", RequiresDocker: true),
        new("DDD/Docker/docker-compose.yml.tpl", "docker-compose.yml", RequiresDocker: true),
        new("DDD/Docker/.dockerignore.tpl", ".dockerignore", RequiresDocker: true),
        new("DDD/Docker/.env.example.tpl", ".env.example", RequiresDocker: true)
    ];

    public static IEnumerable<TemplateFile> ForOptions(GeneratorOptions options) =>
        All.Where(file => file.Matches(options));
}
