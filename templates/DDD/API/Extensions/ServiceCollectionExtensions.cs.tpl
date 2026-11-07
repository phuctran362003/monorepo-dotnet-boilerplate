{{#if FluentValidation}}using {{ApiNamespace}}.Filters;
{{/if}}using Microsoft.Extensions.DependencyInjection;

namespace {{ApiNamespace}}.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddControllers(options =>
        {
{{#if FluentValidation}}            options.Filters.Add<ValidationFilter>();
{{/if}}        });

        services.AddOpenApi();
        services.AddProblemDetails();

        return services;
    }
}
