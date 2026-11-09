using MonorepoDotnet.Core.Models;

namespace MonorepoDotnet.Core.Generation;

/// <summary>Single-project (1-layer) ASP.NET Core Web API layout.</summary>
public static class SingleLayerTemplateManifest
{
    public static IReadOnlyList<TemplateFile> All { get; } =
    [
        // Root
        new("DDD/Root/Directory.Build.props.tpl", "Directory.Build.props"),
        new("DDD/Root/.gitignore.tpl", ".gitignore"),
        new("DDD/Root/.editorconfig.tpl", ".editorconfig"),
        new("SingleLayer/Root/README.md.tpl", "README.md"),

        // Project
        new("SingleLayer/Project.csproj.tpl", "src/{{SingleLayerProject}}/{{SingleLayerProject}}.csproj", Host: ProjectKind.WebApi),
        new("SingleLayer/Program.cs.tpl", "src/{{SingleLayerProject}}/Program.cs", Host: ProjectKind.WebApi),
        new("Console/SingleLayer/Project.csproj.tpl", "src/{{SingleLayerProject}}/{{SingleLayerProject}}.csproj", Host: ProjectKind.Console),
        new("Console/SingleLayer/Program.cs.tpl", "src/{{SingleLayerProject}}/Program.cs", Host: ProjectKind.Console),
        new("Console/Host/Properties/launchSettings.json.tpl", "src/{{SingleLayerProject}}/Properties/launchSettings.json", Host: ProjectKind.Console),
        new("Persistence/BaseEntity.cs.tpl", "src/{{SingleLayerProject}}/Entities/BaseEntity.cs"),
        new("Persistence/IUnitOfWork.cs.tpl", "src/{{SingleLayerProject}}/Common/IUnitOfWork.cs"),
        new("SingleLayer/Exceptions/DomainException.cs.tpl", "src/{{SingleLayerProject}}/Exceptions/DomainException.cs"),

        // Models and product slice
        new("SingleLayer/Models/Product.cs.tpl", "src/{{SingleLayerProject}}/Models/Product.cs"),
        new("SingleLayer/Products/Dtos/ProductDto.cs.tpl", "src/{{SingleLayerProject}}/Products/Dtos/ProductDto.cs"),
        new("SingleLayer/Products/Dtos/CreateProductRequest.cs.tpl", "src/{{SingleLayerProject}}/Products/Dtos/CreateProductRequest.cs"),
        new("SingleLayer/Products/IProductService.cs.tpl", "src/{{SingleLayerProject}}/Products/IProductService.cs"),
        new("SingleLayer/Products/ProductService.cs.tpl", "src/{{SingleLayerProject}}/Products/ProductService.cs"),
        new("SingleLayer/Products/MappingProfile.cs.tpl", "src/{{SingleLayerProject}}/Products/MappingProfile.cs", OptionalPackage.AutoMapper),
        new("SingleLayer/Products/Validators/CreateProductRequestValidator.cs.tpl", "src/{{SingleLayerProject}}/Products/Validators/CreateProductRequestValidator.cs", OptionalPackage.FluentValidation),
        new("SingleLayer/Products/Commands/CreateProductCommand.cs.tpl", "src/{{SingleLayerProject}}/Products/Commands/CreateProductCommand.cs", OptionalPackage.MediatR),
        new("SingleLayer/Products/Commands/CreateProductCommandHandler.cs.tpl", "src/{{SingleLayerProject}}/Products/Commands/CreateProductCommandHandler.cs", OptionalPackage.MediatR),
        new("SingleLayer/Products/Queries/GetProductsQuery.cs.tpl", "src/{{SingleLayerProject}}/Products/Queries/GetProductsQuery.cs", OptionalPackage.MediatR),
        new("SingleLayer/Products/Queries/GetProductsQueryHandler.cs.tpl", "src/{{SingleLayerProject}}/Products/Queries/GetProductsQueryHandler.cs", OptionalPackage.MediatR),
        new("SingleLayer/Products/Queries/GetProductByIdQuery.cs.tpl", "src/{{SingleLayerProject}}/Products/Queries/GetProductByIdQuery.cs", OptionalPackage.MediatR),
        new("SingleLayer/Products/Queries/GetProductByIdQueryHandler.cs.tpl", "src/{{SingleLayerProject}}/Products/Queries/GetProductByIdQueryHandler.cs", OptionalPackage.MediatR),

        // Persistence
        new("SingleLayer/Data/ApplicationDbContext.cs.tpl", "src/{{SingleLayerProject}}/Data/ApplicationDbContext.cs"),
        new("Persistence/IGenericRepository.cs.tpl", "src/{{SingleLayerProject}}/Data/IGenericRepository.cs"),
        new("SingleLayer/Data/Configurations/ProductConfiguration.cs.tpl", "src/{{SingleLayerProject}}/Data/Configurations/ProductConfiguration.cs"),
        new("Persistence/GenericRepository.cs.tpl", "src/{{SingleLayerProject}}/Data/Repositories/GenericRepository.cs"),
        new("Persistence/UnitOfWork.cs.tpl", "src/{{SingleLayerProject}}/Data/UnitOfWork.cs"),
        new("SingleLayer/Data/ApplicationDbContextFactory.cs.tpl", "src/{{SingleLayerProject}}/Data/ApplicationDbContextFactory.cs"),
        new("SingleLayer/Data/Migrations/.gitkeep.tpl", "src/{{SingleLayerProject}}/Data/Migrations/.gitkeep"),
        new("SingleLayer/Data/RedisCacheService.cs.tpl", "src/{{SingleLayerProject}}/Data/RedisCacheService.cs", OptionalPackage.Redis),

        // Caching abstraction
        new("SingleLayer/Caching/ICacheService.cs.tpl", "src/{{SingleLayerProject}}/Caching/ICacheService.cs", OptionalPackage.Redis),

        // Web layer
        new("SingleLayer/Controllers/ProductsController.cs.tpl", "src/{{SingleLayerProject}}/Controllers/ProductsController.cs", Host: ProjectKind.WebApi),
        new("SingleLayer/Middleware/ExceptionHandlingMiddleware.cs.tpl", "src/{{SingleLayerProject}}/Middleware/ExceptionHandlingMiddleware.cs", Host: ProjectKind.WebApi),
        new("SingleLayer/Filters/ValidationFilter.cs.tpl", "src/{{SingleLayerProject}}/Filters/ValidationFilter.cs", OptionalPackage.FluentValidation, Host: ProjectKind.WebApi),
        new("SingleLayer/appsettings.json.tpl", "src/{{SingleLayerProject}}/appsettings.json"),
        new("SingleLayer/appsettings.Development.json.tpl", "src/{{SingleLayerProject}}/appsettings.Development.json"),
        new("SingleLayer/Properties/launchSettings.json.tpl", "src/{{SingleLayerProject}}/Properties/launchSettings.json", Host: ProjectKind.WebApi),

        // Docker (shared)
        new("DDD/Docker/Dockerfile.tpl", "Dockerfile", RequiresDocker: true),
        new("DDD/Docker/docker-compose.yml.tpl", "docker-compose.yml", RequiresDocker: true),
        new("DDD/Docker/.dockerignore.tpl", ".dockerignore", RequiresDocker: true),
        new("DDD/Docker/.env.example.tpl", ".env.example", RequiresDocker: true)
    ];

    public static IEnumerable<TemplateFile> ForOptions(GeneratorOptions options) =>
        All.Where(file => file.Matches(options));
}
