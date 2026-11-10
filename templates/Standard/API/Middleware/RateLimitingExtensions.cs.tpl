using System.Threading.RateLimiting;

namespace {{ApiNamespace}}.Middleware;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection("RateLimiting").Get<RateLimitingOptions>() ?? new RateLimitingOptions();

        services.AddRateLimiter(limiter =>
        {
            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            limiter.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.ContentType = "application/json";
                await context.HttpContext.Response.WriteAsJsonAsync(
                    ApiResult.Failure("429", "Quá nhiều yêu cầu."),
                    cancellationToken);
            };

            limiter.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                if (!httpContext.Request.Path.StartsWithSegments("/api"))
                {
                    return RateLimitPartition.GetNoLimiter("unlimited");
                }

                var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetSlidingWindowLimiter(ip, _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = options.PermitLimit,
                    Window = TimeSpan.FromSeconds(options.WindowSeconds),
                    SegmentsPerWindow = 4,
                    QueueLimit = 0
                });
            });

            limiter.AddPolicy(RateLimitingOptions.LoginPolicy, httpContext =>
            {
                var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetSlidingWindowLimiter($"login:{ip}", _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = options.LoginPermitLimit,
                    Window = TimeSpan.FromSeconds(options.LoginWindowSeconds),
                    SegmentsPerWindow = 4,
                    QueueLimit = 0
                });
            });
        });

        return services;
    }
}
