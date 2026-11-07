using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MonorepoDotnet.CLI;
using MonorepoDotnet.CLI.Prompts;
using MonorepoDotnet.Core.Abstractions;
using MonorepoDotnet.Core.Generation;
using MonorepoDotnet.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "HH:mm:ss ";
});

builder.Services.AddInfrastructure();
builder.Services.AddSingleton<ISolutionGenerator, DddSolutionGenerator>();
builder.Services.AddSingleton<ConfigurationPrompt>();
builder.Services.AddSingleton<CliApplication>();

using var host = builder.Build();

var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("monorepo-dotnet");
using var cancellationSource = new CancellationTokenSource();

Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellationSource.Cancel();
};

try
{
    return await host.Services
        .GetRequiredService<CliApplication>()
        .RunAsync(args, cancellationSource.Token);
}
catch (OperationCanceledException)
{
    logger.LogWarning("Generation cancelled.");
    return 130;
}
catch (Exception ex)
{
    logger.LogError(ex, "Unexpected error.");
    return 1;
}
