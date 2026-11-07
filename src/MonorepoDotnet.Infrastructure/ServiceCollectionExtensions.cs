using Microsoft.Extensions.DependencyInjection;
using MonorepoDotnet.Core.Abstractions;
using MonorepoDotnet.Infrastructure.Environment;
using MonorepoDotnet.Infrastructure.FileSystem;
using MonorepoDotnet.Infrastructure.Processes;
using MonorepoDotnet.Infrastructure.Templates;

namespace MonorepoDotnet.Infrastructure;

public static class ServiceCollectionExtensions
{
    /// <summary>Registers infrastructure implementations of Core abstractions.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IProcessRunner, ProcessRunner>();
        services.AddSingleton<IFileSystem, FileSystemService>();
        services.AddSingleton<ITemplateProvider, TemplateProvider>();
        services.AddSingleton<ITemplateRenderer, TemplateRenderer>();
        services.AddSingleton<IEnvironmentValidator, EnvironmentValidator>();

        return services;
    }
}
