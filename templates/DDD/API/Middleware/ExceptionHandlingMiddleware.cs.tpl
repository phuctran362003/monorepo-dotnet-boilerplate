using System.Net;
using {{DomainNamespace}}.Exceptions;
{{#if FluentValidation}}using FluentValidation;
{{/if}}using Microsoft.AspNetCore.Mvc;

namespace {{ApiNamespace}}.Middleware;

/// <summary>Converts unhandled exceptions into RFC 7807 problem details responses.</summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException exception)
        {
            await WriteProblemAsync(context, HttpStatusCode.BadRequest, "Business rule violated", exception.Message);
        }
{{#if FluentValidation}}        catch (ValidationException exception)
        {
            var errors = exception.Errors
                .GroupBy(failure => failure.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(failure => failure.ErrorMessage).ToArray());

            await WriteProblemAsync(
                context,
                HttpStatusCode.BadRequest,
                "Validation failed",
                "One or more validation errors occurred.",
                errors);
        }
{{/if}}        catch (KeyNotFoundException exception)
        {
            await WriteProblemAsync(context, HttpStatusCode.NotFound, "Resource not found", exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unhandled exception while processing {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await WriteProblemAsync(
                context,
                HttpStatusCode.InternalServerError,
                "Unexpected error",
                "An unexpected error occurred.");
        }
    }

    private static async Task WriteProblemAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string title,
        string detail,
        IDictionary<string, string[]>? errors = null)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        if (errors is not null)
        {
            problem.Extensions["errors"] = errors;
        }

        await context.Response.WriteAsJsonAsync(problem);
    }
}
