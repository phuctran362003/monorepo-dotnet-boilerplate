using System.Text;
using Amazon.S3;
using {{DomainNamespace}};
using {{InfrastructureNamespace}}.Commons;
using {{InfrastructureNamespace}}.Interfaces;
using {{SolutionName}}.Service.Interfaces;
using {{SolutionName}}.Service.Interfaces.Commons;
using {{SolutionName}}.Service.Services;
using {{SolutionName}}.Service.Services.Commons;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace {{ApiNamespace}}.Architecture;

public static class IocContainer
{
    public static IServiceCollection SetupIocContainer(this IServiceCollection services, IConfiguration configuration)
    {
        SetupDbContext(services, configuration);
        SetupBusinessServicesLayer(services);
        SetupJwt(services, configuration);
        SetupObjectStorage(services, configuration);
        return services;
    }

    private static void SetupDbContext(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<{{SolutionName}}DbContext>(options =>
        {
{{#if Postgres}}            options.UseNpgsql(connectionString, provider =>
            {
                provider.MigrationsAssembly(typeof({{SolutionName}}DbContext).Assembly.FullName);
                provider.CommandTimeout(300);
                provider.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            });
{{/if}}{{#if SqlServer}}            options.UseSqlServer(connectionString, provider =>
            {
                provider.MigrationsAssembly(typeof({{SolutionName}}DbContext).Assembly.FullName);
                provider.CommandTimeout(300);
                provider.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            });
{{/if}}        });
    }

    private static void SetupBusinessServicesLayer(IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IBlobService, BlobService>();
        services.AddScoped<IClaimsService, ClaimsService>();
        services.AddHttpContextAccessor();
        services.AddScoped<ILoggerService, LoggerService>();
        services.AddScoped<IMapperService, MapperService>();
        services.AddScoped<ICurrentTime, CurrentTime>();

        // Redis connection string is available at ConnectionStrings:Redis.
        // services.AddStackExchangeRedisCache(options =>
        //     options.Configuration = configuration.GetConnectionString("Redis"));
    }

    private static void SetupJwt(IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["JWT:Issuer"],
                    ValidAudience = configuration["JWT:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["JWT:SecretKey"] ?? string.Empty)),
                    NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier,
                    RoleClaimType = System.Security.Claims.ClaimTypes.Role
                };
            });
    }

    private static void SetupObjectStorage(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ObjectStorageOptions>(configuration.GetSection(ObjectStorageOptions.SectionName));
        services.AddSingleton<IAmazonS3>(provider =>
        {
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<ObjectStorageOptions>>().Value;
            if (string.IsNullOrWhiteSpace(options.ServiceUrl))
            {
                return new AmazonS3Client(options.AccessKey, options.SecretKey, Amazon.RegionEndpoint.USEast1);
            }

            var config = new AmazonS3Config
            {
                ServiceURL = options.ServiceUrl,
                ForcePathStyle = true,
                AuthenticationRegion = string.IsNullOrWhiteSpace(options.Region) ? "us-east-1" : options.Region
            };
            return new AmazonS3Client(options.AccessKey, options.SecretKey, config);
        });
        services.AddHostedService<ObjectStorageBootstrap>();
    }
}

public sealed class ObjectStorageBootstrap : IHostedService
{
    private readonly IAmazonS3 _s3Client;
    private readonly ObjectStorageOptions _options;

    public ObjectStorageBootstrap(IAmazonS3 s3Client, Microsoft.Extensions.Options.IOptions<ObjectStorageOptions> options)
    {
        _s3Client = s3Client;
        _options = options.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ServiceUrl) || string.IsNullOrWhiteSpace(_options.BucketName))
        {
            return;
        }

        try
        {
            await _s3Client.PutBucketAsync(new Amazon.S3.Model.PutBucketRequest
            {
                BucketName = _options.BucketName
            }, cancellationToken);
        }
        catch (AmazonS3Exception exception) when (exception.ErrorCode is "BucketAlreadyOwnedByYou" or "BucketAlreadyExists")
        {
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
