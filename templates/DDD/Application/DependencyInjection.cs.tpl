using {{Namespace}}.Application.Products;
{{#if AutoMapper}}using AutoMapper;
{{/if}}{{#if FluentValidation}}using FluentValidation;
{{/if}}{{#if MediatR}}using MediatR;
{{/if}}using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace {{Namespace}}.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddScoped<IProductService, ProductService>();
{{#if FluentValidation}}
        services.AddValidatorsFromAssemblyContaining<ProductService>();
{{/if}}{{#if AutoMapper}}
        var licenseKey = configuration["AutoMapper:LicenseKey"];
        services.AddAutoMapper(
            config =>
            {
                if (!string.IsNullOrWhiteSpace(licenseKey))
                {
                    config.LicenseKey = licenseKey;
                }
            },
            typeof(ProductService).Assembly);
{{/if}}{{#if MediatR}}
        services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<ProductService>());
{{/if}}
        return services;
    }
}
