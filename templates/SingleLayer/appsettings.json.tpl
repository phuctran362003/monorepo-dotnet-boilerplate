{
  "ConnectionStrings": {
{{#if Postgres}}    "Default": "Host=localhost;Port={{DatabasePort}};Database={{DatabaseName}};Username={{DatabaseUser}};Password="{{#if Redis}},{{/if}}
{{/if}}{{#if SqlServer}}    "Default": "Server=localhost,{{DatabasePort}};Database={{DatabaseName}};User Id={{DatabaseUser}};TrustServerCertificate=True;Encrypt=False"{{#if Redis}},{{/if}}
{{/if}}{{#if Redis}}    "Redis": "localhost:6379"
{{/if}}  },
  "Database": {
    "ApplyMigrations": false
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"{{#if Serilog}},
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" }
    ],
    "Enrich": [ "FromLogContext" ]
  }{{/if}}
}
