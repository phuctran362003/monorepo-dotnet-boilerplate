{{#if Redis}}using {{Namespace}}.Caching;
{{/if}}using {{Namespace}}.Common;
using {{Namespace}}.Data;
using {{Namespace}}.Data.Repositories;
{{#if FluentValidation}}using {{Namespace}}.Filters;
{{/if}}using {{Namespace}}.Middleware;
using {{Namespace}}.Products;
{{#if Serilog}}using Serilog;
{{/if}}{{#if SqlServer}}using Microsoft.Data.SqlClient;
{{/if}}using Microsoft.EntityFrameworkCore;
{{#if FluentValidation}}using FluentValidation;
{{/if}}{{#if AutoMapper}}using AutoMapper;
{{/if}}{{#if MediatR}}using MediatR;
{{/if}}using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

{{#if Serilog}}builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

{{/if}}builder.Services.AddControllers(options =>
{
{{#if FluentValidation}}    options.Filters.Add<ValidationFilter>();
{{/if}}});
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

var connectionString = builder.Configuration.GetConnectionString("Default");

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
var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
{{#if Serilog}}app.UseSerilogRequestLogging();
{{/if}}
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "{{SolutionName}} v1");
    });
}

app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "{{SolutionName}}" }));

if (builder.Configuration.GetValue<bool>("Database:ApplyMigrations"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();
