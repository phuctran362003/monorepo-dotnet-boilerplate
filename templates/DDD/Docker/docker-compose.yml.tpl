name: {{DockerName}}

services:
  {{ComposeService}}:
    build:
      context: .
      dockerfile: Dockerfile
    image: {{DockerName}}-{{ComposeService}}:latest
    environment:
{{#if Console}}      DOTNET_ENVIRONMENT: Development
{{/if}}{{#if !Console}}      ASPNETCORE_ENVIRONMENT: Development
      ASPNETCORE_URLS: http://+:8080
{{/if}}{{#if Postgres}}      ConnectionStrings__Default: "Host={{DatabaseService}};Port={{DatabasePort}};Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}"
{{/if}}{{#if SqlServer}}      ConnectionStrings__Default: "Server={{DatabaseService}},{{DatabasePort}};Database=${MSSQL_DB};User Id=${MSSQL_USER};TrustServerCertificate=True;Encrypt=False"
      Database__Password: ${MSSQL_SA_PASSWORD}
{{/if}}      Database__ApplyMigrations: ${APPLY_MIGRATIONS:-false}
{{#if Redis}}      ConnectionStrings__Redis: "redis:6379"
{{/if}}{{#if !Console}}    ports:
      - "${API_HTTP_PORT:-{{ApiHttpPort}}}:8080"
{{/if}}
    depends_on:
{{#if Postgres}}      postgres:
        condition: service_healthy
{{/if}}{{#if SqlServer}}      sqlserver:
        condition: service_healthy
{{/if}}{{#if Redis}}      redis:
        condition: service_healthy
{{/if}}    networks:
      - {{DockerName}}-network
    restart: unless-stopped
{{#if Postgres}}
  postgres:
    image: postgres:{{PostgresImage}}
    environment:
      POSTGRES_DB: ${POSTGRES_DB}
      POSTGRES_USER: ${POSTGRES_USER}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
    ports:
      - "${POSTGRES_PORT:-{{DatabasePort}}}:5432"
    volumes:
      - {{DockerName}}-postgres-data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USER} -d ${POSTGRES_DB}"]
      interval: 5s
      timeout: 5s
      retries: 10
    networks:
      - {{DockerName}}-network
    restart: unless-stopped
{{/if}}{{#if SqlServer}}
  sqlserver:
    image: mcr.microsoft.com/mssql/server:{{SqlServerImage}}
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_PID: Developer
      MSSQL_SA_PASSWORD: ${MSSQL_SA_PASSWORD}
    ports:
      - "${MSSQL_PORT:-{{DatabasePort}}}:1433"
    volumes:
      - {{DockerName}}-sqlserver-data:/var/opt/mssql
    healthcheck:
      test: ["CMD-SHELL", "/opt/mssql-tools18/bin/sqlcmd -C -S localhost -U ${MSSQL_USER} -P \"$$MSSQL_SA_PASSWORD\" -Q \"SELECT 1\" -b -o /dev/null || /opt/mssql-tools/bin/sqlcmd -C -S localhost -U ${MSSQL_USER} -P \"$$MSSQL_SA_PASSWORD\" -Q \"SELECT 1\" -b -o /dev/null"]
      interval: 10s
      timeout: 5s
      retries: 10
      start_period: 20s
    networks:
      - {{DockerName}}-network
    restart: unless-stopped
{{/if}}{{#if Redis}}
  redis:
    image: redis:{{RedisImage}}
    command: ["redis-server", "--appendonly", "yes"]
    volumes:
      - {{DockerName}}-redis-data:/data
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 5s
      timeout: 3s
      retries: 10
    networks:
      - {{DockerName}}-network
    restart: unless-stopped
{{/if}}
volumes:
{{#if Postgres}}  {{DockerName}}-postgres-data:
{{/if}}{{#if SqlServer}}  {{DockerName}}-sqlserver-data:
{{/if}}{{#if Redis}}  {{DockerName}}-redis-data:
{{/if}}
networks:
  {{DockerName}}-network:
    driver: bridge
