using {{Namespace}}.API.Extensions;
using {{Namespace}}.API.Middleware;
using {{Namespace}}.Application;
using {{Namespace}}.Infrastructure;
using {{Namespace}}.Infrastructure.Persistence;
{{#if Serilog}}using Serilog;
{{/if}}using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

{{#if Serilog}}builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

{{/if}}builder.Services.AddApiServices();
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
{{#if Serilog}}app.UseSerilogRequestLogging();
{{/if}}
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
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
