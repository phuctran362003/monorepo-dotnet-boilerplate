{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port={{PostgresPort}};Database={{DatabaseName}};Username={{PostgresUser}};Password="{{#if Redis}},
    "Redis": "localhost:6379"{{/if}}
  },
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
