using MonorepoDotnet.Core.Models;

namespace MonorepoDotnet.Core.Generation;

/// <summary>
/// Files for the standard backend in <c>docs/backend-structure.md</c>:
/// API → Service → Infrastructure → Domain, plus one Product feature in the
/// documented slots.
/// </summary>
public static class StandardTemplateManifest
{
    public static IReadOnlyList<TemplateFile> All { get; } =
    [
        new("DDD/Root/Directory.Build.props.tpl", "Directory.Build.props"),
        new("DDD/Root/.gitignore.tpl", ".gitignore"),
        new("DDD/Root/.editorconfig.tpl", ".editorconfig"),
        new("Standard/Root/README.md.tpl", "README.md"),

        new("Standard/Domain/Project.csproj.tpl", "src/{{DomainProject}}/{{DomainProject}}.csproj"),
        new("Standard/Domain/DbContext.cs.tpl", "src/{{DomainProject}}/{{SolutionName}}DbContext.cs"),
        new("Standard/Domain/Entities/BaseEntity.cs.tpl", "src/{{DomainProject}}/Entities/BaseEntity.cs"),
        new("Standard/Domain/Entities/Product.cs.tpl", "src/{{DomainProject}}/Entities/Product.cs"),
        new("DDD/Infrastructure/Persistence/Migrations/.gitkeep.tpl", "src/{{DomainProject}}/Migrations/.gitkeep"),

        new("Standard/Infrastructure/Project.csproj.tpl", "src/{{InfrastructureProject}}/{{InfrastructureProject}}.csproj"),
        new("Standard/Infrastructure/UnitOfWork.cs.tpl", "src/{{InfrastructureProject}}/UnitOfWork.cs"),
        new("Standard/Infrastructure/Interfaces/IGenericRepository.cs.tpl", "src/{{InfrastructureProject}}/Interfaces/IGenericRepository.cs"),
        new("Standard/Infrastructure/Interfaces/IUnitOfWork.cs.tpl", "src/{{InfrastructureProject}}/Interfaces/IUnitOfWork.cs"),
        new("Standard/Infrastructure/Interfaces/IClaimsService.cs.tpl", "src/{{InfrastructureProject}}/Interfaces/IClaimsService.cs"),
        new("Standard/Infrastructure/Interfaces/ICurrentTime.cs.tpl", "src/{{InfrastructureProject}}/Interfaces/ICurrentTime.cs"),
        new("Standard/Infrastructure/Repositories/GenericRepository.cs.tpl", "src/{{InfrastructureProject}}/Repositories/GenericRepository.cs"),
        new("Standard/Infrastructure/Commons/ClaimsService.cs.tpl", "src/{{InfrastructureProject}}/Commons/ClaimsService.cs"),
        new("Standard/Infrastructure/Commons/CurrentTime.cs.tpl", "src/{{InfrastructureProject}}/Commons/CurrentTime.cs"),
        new("Standard/Infrastructure/Commons/Pagination.cs.tpl", "src/{{InfrastructureProject}}/Commons/Pagination.cs"),
        new("Standard/Infrastructure/Utils/AuthenTools.cs.tpl", "src/{{InfrastructureProject}}/Utils/AuthenTools.cs"),
        new("Standard/Infrastructure/Utils/TokenTools.cs.tpl", "src/{{InfrastructureProject}}/Utils/TokenTools.cs"),
        new("Standard/Infrastructure/Utils/GenerateJWTToken.cs.tpl", "src/{{InfrastructureProject}}/Utils/GenerateJWTToken.cs"),
        new("Standard/Infrastructure/Utils/HashHelper.cs.tpl", "src/{{InfrastructureProject}}/Utils/HashHelper.cs"),
        new("Standard/Infrastructure/Utils/StringTools.cs.tpl", "src/{{InfrastructureProject}}/Utils/StringTools.cs"),
        new("Standard/Infrastructure/Utils/StringExtensions.cs.tpl", "src/{{InfrastructureProject}}/Utils/StringExtensions.cs"),
        new("Standard/Infrastructure/Utils/ResourceHelper.cs.tpl", "src/{{InfrastructureProject}}/Utils/ResourceHelper.cs"),

        new("Standard/Service/Project.csproj.tpl", "src/{{ServiceProject}}/{{ServiceProject}}.csproj"),
        new("Standard/Service/Interfaces/IProductService.cs.tpl", "src/{{ServiceProject}}/Interfaces/IProductService.cs"),
        new("Standard/Service/Interfaces/Commons/ILoggerService.cs.tpl", "src/{{ServiceProject}}/Interfaces/Commons/ILoggerService.cs"),
        new("Standard/Service/Interfaces/Commons/IBlobService.cs.tpl", "src/{{ServiceProject}}/Interfaces/Commons/IBlobService.cs"),
        new("Standard/Service/Interfaces/Commons/IMapperService.cs.tpl", "src/{{ServiceProject}}/Interfaces/Commons/IMapperService.cs"),
        new("Standard/Service/Services/ProductService.cs.tpl", "src/{{ServiceProject}}/Services/ProductService.cs"),
        new("Standard/Service/Services/Commons/LoggerService.cs.tpl", "src/{{ServiceProject}}/Services/Commons/LoggerService.cs"),
        new("Standard/Service/Services/Commons/BlobService.cs.tpl", "src/{{ServiceProject}}/Services/Commons/BlobService.cs"),
        new("Standard/Service/Services/Commons/MapperService.cs.tpl", "src/{{ServiceProject}}/Services/Commons/MapperService.cs"),
        new("Standard/Service/DTOs/ProductDtos.cs.tpl", "src/{{ServiceProject}}/DTOs/ProductDtos.cs"),
        new("Standard/Service/DTOs/BlobDtos.cs.tpl", "src/{{ServiceProject}}/DTOs/BlobDtos.cs"),
        new("Standard/Service/Mappers/ProductMapper.cs.tpl", "src/{{ServiceProject}}/Mappers/ProductMapper.cs"),
        new("Standard/Service/Utils/ApiResult.cs.tpl", "src/{{ServiceProject}}/Utils/ApiResult.cs"),
        new("Standard/Service/Utils/ErrorHelper.cs.tpl", "src/{{ServiceProject}}/Utils/ErrorHelper.cs"),
        new("Standard/Service/Utils/ExceptionUtils.cs.tpl", "src/{{ServiceProject}}/Utils/ExceptionUtils.cs"),
        new("Standard/Service/Utils/ErrorMessages.cs.tpl", "src/{{ServiceProject}}/Utils/ErrorMessages.cs"),
        new("Standard/Service/Utils/PasswordHasher.cs.tpl", "src/{{ServiceProject}}/Utils/PasswordHasher.cs"),
        new("Standard/Service/Utils/JwtUtils.cs.tpl", "src/{{ServiceProject}}/Utils/JwtUtils.cs"),
        new("Standard/Service/Utils/DateTimeUtc.cs.tpl", "src/{{ServiceProject}}/Utils/DateTimeUtc.cs"),
        new("Standard/Service/Utils/OtpGenerator.cs.tpl", "src/{{ServiceProject}}/Utils/OtpGenerator.cs"),
        new("Standard/Service/Utils/ImageFiles.cs.tpl", "src/{{ServiceProject}}/Utils/ImageFiles.cs"),
        new("Standard/Service/Utils/SharedCacheKeys/ListingSharedCacheKeys.cs.tpl", "src/{{ServiceProject}}/Utils/SharedCacheKeys/ListingSharedCacheKeys.cs"),

        new("Standard/API/Project.csproj.tpl", "src/{{ApiProject}}/{{ApiProject}}.csproj"),
        new("Standard/API/Program.cs.tpl", "src/{{ApiProject}}/Program.cs"),
        new("Standard/API/appsettings.json.tpl", "src/{{ApiProject}}/appsettings.json"),
        new("DDD/Docker/Dockerfile.tpl", "src/{{ApiProject}}/Dockerfile"),
        new("Standard/API/Properties/launchSettings.json.tpl", "src/{{ApiProject}}/Properties/launchSettings.json"),
        new("Standard/API/Architecture/IOCContainer.cs.tpl", "src/{{ApiProject}}/Architecture/IOCContainer.cs"),
        new("Standard/API/Architecture/MigrationExtensions.cs.tpl", "src/{{ApiProject}}/Architecture/MigrationExtensions.cs"),
        new("Standard/API/Middleware/RateLimitingOptions.cs.tpl", "src/{{ApiProject}}/Middleware/RateLimitingOptions.cs"),
        new("Standard/API/Middleware/RateLimitingExtensions.cs.tpl", "src/{{ApiProject}}/Middleware/RateLimitingExtensions.cs"),
        new("Standard/API/Controllers/ProductController.cs.tpl", "src/{{ApiProject}}/Controllers/ProductController.cs"),

        new("Standard/Docker/docker-compose.yml.tpl", "docker-compose.yml", RequiresDocker: true),
        new("DDD/Docker/.dockerignore.tpl", ".dockerignore", RequiresDocker: true),
        new("DDD/Docker/.env.example.tpl", ".env.example", RequiresDocker: true)
    ];

    public static IEnumerable<TemplateFile> ForOptions(GeneratorOptions options) =>
        All.Where(file => file.Matches(options));
}
