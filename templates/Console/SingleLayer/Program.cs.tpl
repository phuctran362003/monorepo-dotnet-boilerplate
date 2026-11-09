{{#if Redis}}using {{Namespace}}.Caching;
{{/if}}using {{Namespace}}.Common;
using {{Namespace}}.Data;
using {{Namespace}}.Data.Repositories;
using {{Namespace}}.Products;
using {{Namespace}}.Products.Dtos;
{{#if Serilog}}using Serilog;
{{/if}}{{#if SqlServer}}using Microsoft.Data.SqlClient;
{{/if}}using Microsoft.EntityFrameworkCore;
{{#if FluentValidation}}using FluentValidation;
{{/if}}{{#if AutoMapper}}using AutoMapper;
{{/if}}{{#if MediatR}}using MediatR;
{{/if}}using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

{{#if Serilog}}builder.Services.AddSerilog((services, configuration) => configuration
    .ReadFrom.Configuration(builder.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

{{/if}}var connectionString = builder.Configuration.GetConnectionString("Default");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'Default' is not configured. Set the " +
        "ConnectionStrings__Default environment variable or appsettings.json.");
}
{{#if SqlServer}}
var databasePassword = builder.Configuration["Database:Password"];

if (!string.IsNullOrWhiteSpace(databasePassword))
{
    var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString) { Password = databasePassword };
    connectionString = connectionStringBuilder.ConnectionString;
}
{{/if}}builder.Services.AddDbContext<ApplicationDbContext>(options =>
{{#if Postgres}}    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
        npgsql.EnableRetryOnFailure(3);
    }));
{{/if}}{{#if SqlServer}}    options.UseSqlServer(connectionString, sqlServer =>
    {
        sqlServer.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
        sqlServer.EnableRetryOnFailure(3);
    }));
{{/if}}
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IProductService, ProductService>();
{{#if FluentValidation}}
builder.Services.AddValidatorsFromAssemblyContaining<ProductService>();
{{/if}}{{#if AutoMapper}}
var licenseKey = builder.Configuration["AutoMapper:LicenseKey"];
builder.Services.AddAutoMapper(
    config =>
    {
        if (!string.IsNullOrWhiteSpace(licenseKey))
        {
            config.LicenseKey = licenseKey;
        }
    },
    typeof(ProductService).Assembly);
{{/if}}{{#if MediatR}}
builder.Services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<ProductService>());
{{/if}}{{#if Redis}}
var redisConnection = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = "{{DatabaseName}}:";
});

builder.Services.AddSingleton<ICacheService, RedisCacheService>();
{{/if}}
using var host = builder.Build();

if (builder.Configuration.GetValue<bool>("Database:ApplyMigrations"))
{
    using var migrationScope = host.Services.CreateScope();
    var dbContext = migrationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}

using var scope = host.Services.CreateScope();
var products = scope.ServiceProvider.GetRequiredService<IProductService>();
var request = new CreateProductRequest("Sample product", 9.99m);
{{#if FluentValidation}}
var validator = scope.ServiceProvider.GetRequiredService<IValidator<CreateProductRequest>>();
var validation = await validator.ValidateAsync(request);
if (!validation.IsValid)
{
    foreach (var error in validation.Errors)
    {
        Console.Error.WriteLine(error.ErrorMessage);
    }

    return;
}
{{/if}}
var created = await products.CreateAsync(request);
Console.WriteLine($"Created {created.Id} {created.Name} {created.Price}");

foreach (var product in await products.GetAllAsync())
{
    Console.WriteLine($"{product.Id} {product.Name} {product.Price}");
}
