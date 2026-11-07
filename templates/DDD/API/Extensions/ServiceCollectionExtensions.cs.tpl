{{#if FluentValidation}}using {{Namespace}}.API.Filters;
{{/if}}using Microsoft.Extensions.DependencyInjection;

namespace {{Namespace}}.API.Extensions;

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
