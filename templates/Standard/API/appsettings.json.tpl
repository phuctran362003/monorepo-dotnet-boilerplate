{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
{{#if Postgres}}    "DefaultConnection": "Host=localhost;Port={{DatabasePort}};Database={{DockerName}};Username=postgres;Password=",
{{/if}}{{#if SqlServer}}    "DefaultConnection": "Server=localhost,{{DatabasePort}};Database={{DockerName}};User Id=sa;TrustServerCertificate=True;Encrypt=False",
{{/if}}    "Redis": "localhost:6379"
  },
  "AllowedHosts": "*",
  "RateLimiting": {
    "PermitLimit": 100,
    "WindowSeconds": 60,
    "LoginPermitLimit": 5,
    "LoginWindowSeconds": 60
  },
  "JWT": {
    "SecretKey": "change-me-to-a-secret-that-is-at-least-32-characters",
    "Issuer": "{{SolutionName}}",
    "Audience": "{{SolutionName}}",
    "ValidIssuer": "{{SolutionName}}",
    "ValidAudience": "{{SolutionName}}",
    "TokenValidityInMinutes": "60"
  },
  "ObjectStorage": {
    "ServiceUrl": "",
    "BucketName": "",
    "PublicUrl": "",
    "AccessKey": "",
    "SecretKey": "",
    "Region": "us-east-1",
    "StorageQuotaBytes": 10737418240
  }
}
