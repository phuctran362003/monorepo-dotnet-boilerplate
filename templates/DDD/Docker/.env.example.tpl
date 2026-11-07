# Copy this file to .env and set strong, unique values.
# Never commit the real .env file.
{{#if Postgres}}POSTGRES_DB={{DatabaseName}}
POSTGRES_USER={{DatabaseUser}}
POSTGRES_PASSWORD=change-me-strong-password
POSTGRES_PORT={{DatabasePort}}
{{/if}}{{#if SqlServer}}MSSQL_DB={{DatabaseName}}
MSSQL_USER={{DatabaseUser}}
MSSQL_SA_PASSWORD=Change-me-Strong0Password
MSSQL_PORT={{DatabasePort}}
{{/if}}API_HTTP_PORT={{ApiHttpPort}}
APPLY_MIGRATIONS=false
