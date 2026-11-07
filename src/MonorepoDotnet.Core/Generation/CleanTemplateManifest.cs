using MonorepoDotnet.Core.Models;

namespace MonorepoDotnet.Core.Generation;

/// <summary>
/// Clean Architecture layout: Domain, Application, Infrastructure and WebApi
/// with dependencies pointing inward.
/// </summary>
public static class CleanTemplateManifest
{
    public static IReadOnlyList<TemplateFile> All { get; } =
    [
        // Root
        new("DDD/Root/Directory.Build.props.tpl", "Directory.Build.props"),
        new("DDD/Root/.gitignore.tpl", ".gitignore"),
        new("DDD/Root/.editorconfig.tpl", ".editorconfig"),
        new("Clean/Root/README.md.tpl", "README.md"),

        // Domain
        new("DDD/Domain/Project.csproj.tpl", "src/{{DomainProject}}/{{DomainProject}}.csproj"),
        new("DDD/Domain/Exceptions/DomainException.cs.tpl", "src/{{DomainProject}}/Exceptions/DomainException.cs"),
        new("Clean/Domain/Entities/Product.cs.tpl", "src/{{DomainProject}}/Entities/Product.cs"),
        new("DDD/Domain/Repositories/IProductRepository.cs.tpl", "src/{{DomainProject}}/Repositories/IProductRepository.cs"),

        // Application
        new("DDD/Application/Project.csproj.tpl", "src/{{ApplicationProject}}/{{ApplicationProject}}.csproj"),
        new("DDD/Application/DependencyInjection.cs.tpl", "src/{{ApplicationProject}}/DependencyInjection.cs"),
        new("DDD/Application/Common/Interfaces/IUnitOfWork.cs.tpl", "src/{{ApplicationProject}}/Common/Interfaces/IUnitOfWork.cs"),
        new("Clean/Application/Products/Dtos/ProductDto.cs.tpl", "src/{{ApplicationProject}}/Products/Dtos/ProductDto.cs"),
        new("DDD/Application/Products/Dtos/CreateProductRequest.cs.tpl", "src/{{ApplicationProject}}/Products/Dtos/CreateProductRequest.cs"),
        new("DDD/Application/Products/IProductService.cs.tpl", "src/{{ApplicationProject}}/Products/IProductService.cs"),
        new("Clean/Application/Products/ProductService.cs.tpl", "src/{{ApplicationProject}}/Products/ProductService.cs"),
        new("Clean/Application/Products/MappingProfile.cs.tpl", "src/{{ApplicationProject}}/Products/MappingProfile.cs", OptionalPackage.AutoMapper),
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
        new("Clean/Infrastructure/Persistence/Configurations/ProductConfiguration.cs.tpl", "src/{{InfrastructureProject}}/Persistence/Configurations/ProductConfiguration.cs"),
        new("DDD/Infrastructure/Persistence/Repositories/ProductRepository.cs.tpl", "src/{{InfrastructureProject}}/Persistence/Repositories/ProductRepository.cs"),
        new("DDD/Infrastructure/Persistence/ApplicationDbContextFactory.cs.tpl", "src/{{InfrastructureProject}}/Persistence/ApplicationDbContextFactory.cs"),
        new("DDD/Infrastructure/Persistence/Migrations/.gitkeep.tpl", "src/{{InfrastructureProject}}/Persistence/Migrations/.gitkeep"),
        new("DDD/Infrastructure/Caching/RedisCacheService.cs.tpl", "src/{{InfrastructureProject}}/Caching/RedisCacheService.cs", OptionalPackage.Redis),

        // WebApi
        new("DDD/API/Project.csproj.tpl", "src/{{ApiProject}}/{{ApiProject}}.csproj"),
        new("DDD/API/Program.cs.tpl", "src/{{ApiProject}}/Program.cs"),
        new("DDD/API/Controllers/ProductsController.cs.tpl", "src/{{ApiProject}}/Controllers/ProductsController.cs"),
        new("DDD/API/Middleware/ExceptionHandlingMiddleware.cs.tpl", "src/{{ApiProject}}/Middleware/ExceptionHandlingMiddleware.cs"),
        new("DDD/API/Extensions/ServiceCollectionExtensions.cs.tpl", "src/{{ApiProject}}/Extensions/ServiceCollectionExtensions.cs"),
        new("DDD/API/Filters/ValidationFilter.cs.tpl", "src/{{ApiProject}}/Filters/ValidationFilter.cs", OptionalPackage.FluentValidation),
        new("DDD/API/appsettings.json.tpl", "src/{{ApiProject}}/appsettings.json"),
        new("DDD/API/appsettings.Development.json.tpl", "src/{{ApiProject}}/appsettings.Development.json"),
        new("DDD/API/Properties/launchSettings.json.tpl", "src/{{ApiProject}}/Properties/launchSettings.json"),

        // Docker (shared templates, one project per copy line is generated from the layout)
        new("DDD/Docker/Dockerfile.tpl", "Dockerfile", RequiresDocker: true),
        new("DDD/Docker/docker-compose.yml.tpl", "docker-compose.yml", RequiresDocker: true),
        new("DDD/Docker/.dockerignore.tpl", ".dockerignore", RequiresDocker: true),
        new("DDD/Docker/.env.example.tpl", ".env.example", RequiresDocker: true)
    ];

    public static IEnumerable<TemplateFile> ForOptions(GeneratorOptions options) =>
        All.Where(file =>
            (file.RequiredPackage == OptionalPackage.None || options.HasPackage(file.RequiredPackage)) &&
            (!file.RequiresDocker || options.IncludeDocker));
}
