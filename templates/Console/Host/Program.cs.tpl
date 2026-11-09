using {{ApplicationNamespace}};
using {{ApplicationNamespace}}.Products;
using {{ApplicationNamespace}}.Products.Dtos;
using {{InfrastructureNamespace}};
using {{InfrastructureNamespace}}.Persistence;
{{#if Serilog}}using Serilog;
{{/if}}{{#if FluentValidation}}using FluentValidation;
{{/if}}using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

{{/if}}builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

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
