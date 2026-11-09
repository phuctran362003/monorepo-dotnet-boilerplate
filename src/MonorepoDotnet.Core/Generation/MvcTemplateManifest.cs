using MonorepoDotnet.Core.Models;

namespace MonorepoDotnet.Core.Generation;

/// <summary>ASP.NET Core MVC layout with Razor views in a single web project.</summary>
public static class MvcTemplateManifest
{
    public static IReadOnlyList<TemplateFile> All { get; } =
    [
        // Root
        new("DDD/Root/Directory.Build.props.tpl", "Directory.Build.props"),
        new("DDD/Root/.gitignore.tpl", ".gitignore"),
        new("DDD/Root/.editorconfig.tpl", ".editorconfig"),
        new("Mvc/Root/README.md.tpl", "README.md"),

        // Project
        new("Mvc/Project.csproj.tpl", "src/{{MvcProject}}/{{MvcProject}}.csproj"),
        new("Mvc/Program.cs.tpl", "src/{{MvcProject}}/Program.cs"),
        new("Persistence/BaseEntity.cs.tpl", "src/{{MvcProject}}/Entities/BaseEntity.cs"),
        new("Persistence/IUnitOfWork.cs.tpl", "src/{{MvcProject}}/Common/IUnitOfWork.cs"),
        new("Mvc/Exceptions/DomainException.cs.tpl", "src/{{MvcProject}}/Exceptions/DomainException.cs"),

        // Models and product slice
        new("Mvc/Models/Product.cs.tpl", "src/{{MvcProject}}/Models/Product.cs"),
        new("Mvc/ViewModels/ProductViewModel.cs.tpl", "src/{{MvcProject}}/ViewModels/ProductViewModel.cs"),
        new("Mvc/ViewModels/CreateProductViewModel.cs.tpl", "src/{{MvcProject}}/ViewModels/CreateProductViewModel.cs"),
        new("Mvc/Services/IProductService.cs.tpl", "src/{{MvcProject}}/Services/IProductService.cs"),
        new("Mvc/Services/ProductService.cs.tpl", "src/{{MvcProject}}/Services/ProductService.cs"),
        new("Mvc/Services/MappingProfile.cs.tpl", "src/{{MvcProject}}/Services/MappingProfile.cs", OptionalPackage.AutoMapper),
        new("Mvc/Services/Validators/CreateProductViewModelValidator.cs.tpl", "src/{{MvcProject}}/Services/Validators/CreateProductViewModelValidator.cs", OptionalPackage.FluentValidation),
        new("Mvc/Products/Commands/CreateProductCommand.cs.tpl", "src/{{MvcProject}}/Products/Commands/CreateProductCommand.cs", OptionalPackage.MediatR),
        new("Mvc/Products/Commands/CreateProductCommandHandler.cs.tpl", "src/{{MvcProject}}/Products/Commands/CreateProductCommandHandler.cs", OptionalPackage.MediatR),
        new("Mvc/Products/Queries/GetProductsQuery.cs.tpl", "src/{{MvcProject}}/Products/Queries/GetProductsQuery.cs", OptionalPackage.MediatR),
        new("Mvc/Products/Queries/GetProductsQueryHandler.cs.tpl", "src/{{MvcProject}}/Products/Queries/GetProductsQueryHandler.cs", OptionalPackage.MediatR),
        new("Mvc/Products/Queries/GetProductByIdQuery.cs.tpl", "src/{{MvcProject}}/Products/Queries/GetProductByIdQuery.cs", OptionalPackage.MediatR),
        new("Mvc/Products/Queries/GetProductByIdQueryHandler.cs.tpl", "src/{{MvcProject}}/Products/Queries/GetProductByIdQueryHandler.cs", OptionalPackage.MediatR),

        // Persistence
        new("Mvc/Data/ApplicationDbContext.cs.tpl", "src/{{MvcProject}}/Data/ApplicationDbContext.cs"),
        new("Persistence/IGenericRepository.cs.tpl", "src/{{MvcProject}}/Data/IGenericRepository.cs"),
        new("Mvc/Data/Configurations/ProductConfiguration.cs.tpl", "src/{{MvcProject}}/Data/Configurations/ProductConfiguration.cs"),
        new("Persistence/GenericRepository.cs.tpl", "src/{{MvcProject}}/Data/Repositories/GenericRepository.cs"),
        new("Persistence/UnitOfWork.cs.tpl", "src/{{MvcProject}}/Data/UnitOfWork.cs"),
        new("Mvc/Data/ApplicationDbContextFactory.cs.tpl", "src/{{MvcProject}}/Data/ApplicationDbContextFactory.cs"),
        new("Mvc/Data/Migrations/.gitkeep.tpl", "src/{{MvcProject}}/Data/Migrations/.gitkeep"),
        new("Mvc/Data/RedisCacheService.cs.tpl", "src/{{MvcProject}}/Data/RedisCacheService.cs", OptionalPackage.Redis),
        new("Mvc/Caching/ICacheService.cs.tpl", "src/{{MvcProject}}/Caching/ICacheService.cs", OptionalPackage.Redis),

        // MVC
        new("Mvc/Controllers/ProductsController.cs.tpl", "src/{{MvcProject}}/Controllers/ProductsController.cs"),
        new("Mvc/Controllers/HomeController.cs.tpl", "src/{{MvcProject}}/Controllers/HomeController.cs"),
        new("Mvc/Middleware/ExceptionHandlingMiddleware.cs.tpl", "src/{{MvcProject}}/Middleware/ExceptionHandlingMiddleware.cs"),
        new("Mvc/Filters/ValidationFilter.cs.tpl", "src/{{MvcProject}}/Filters/ValidationFilter.cs", OptionalPackage.FluentValidation),
        new("Mvc/Views/_ViewImports.cshtml", "src/{{MvcProject}}/Views/_ViewImports.cshtml"),
        new("Mvc/Views/_ViewStart.cshtml", "src/{{MvcProject}}/Views/_ViewStart.cshtml"),
        new("Mvc/Views/Shared/_Layout.cshtml", "src/{{MvcProject}}/Views/Shared/_Layout.cshtml"),
        new("Mvc/Views/Products/Index.cshtml", "src/{{MvcProject}}/Views/Products/Index.cshtml"),
        new("Mvc/Views/Products/Create.cshtml", "src/{{MvcProject}}/Views/Products/Create.cshtml"),
        new("Mvc/Views/Products/Details.cshtml", "src/{{MvcProject}}/Views/Products/Details.cshtml"),
        new("Mvc/Views/Home/Index.cshtml", "src/{{MvcProject}}/Views/Home/Index.cshtml"),
        new("Mvc/wwwroot/site.css", "src/{{MvcProject}}/wwwroot/site.css"),
        new("Mvc/appsettings.json.tpl", "src/{{MvcProject}}/appsettings.json"),
        new("Mvc/appsettings.Development.json.tpl", "src/{{MvcProject}}/appsettings.Development.json"),
        new("Mvc/Properties/launchSettings.json.tpl", "src/{{MvcProject}}/Properties/launchSettings.json"),

        // Docker (shared)
        new("DDD/Docker/Dockerfile.tpl", "Dockerfile", RequiresDocker: true),
        new("DDD/Docker/docker-compose.yml.tpl", "docker-compose.yml", RequiresDocker: true),
        new("DDD/Docker/.dockerignore.tpl", ".dockerignore", RequiresDocker: true),
        new("DDD/Docker/.env.example.tpl", ".env.example", RequiresDocker: true)
    ];

    public static IEnumerable<TemplateFile> ForOptions(GeneratorOptions options) =>
        All.Where(file => file.Matches(options));
}
