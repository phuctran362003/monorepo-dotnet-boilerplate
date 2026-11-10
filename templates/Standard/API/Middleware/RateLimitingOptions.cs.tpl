namespace {{ApiNamespace}}.Middleware;

public class RateLimitingOptions
{
    public const string LoginPolicy = "login";

    public int PermitLimit { get; set; } = 100;

    public int WindowSeconds { get; set; } = 60;

    public int LoginPermitLimit { get; set; } = 5;

    public int LoginWindowSeconds { get; set; } = 60;
}
