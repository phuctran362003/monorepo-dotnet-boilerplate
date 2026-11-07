name: {{DockerName}}

services:
  api:
    build:
      context: .
      dockerfile: Dockerfile
    image: {{DockerName}}-api:latest
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ASPNETCORE_URLS: http://+:8080
      ConnectionStrings__Default: "Host=postgres;Port={{PostgresPort}};Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}"
      Database__ApplyMigrations: ${APPLY_MIGRATIONS:-false}
{{#if Redis}}      ConnectionStrings__Redis: "redis:6379"
{{/if}}    ports:
      - "${API_HTTP_PORT:-{{ApiHttpPort}}}:8080"
    depends_on:
      postgres:
        condition: service_healthy
{{#if Redis}}      redis:
        condition: service_healthy
{{/if}}    networks:
      - {{DockerName}}-network
    restart: unless-stopped

  postgres:
    image: postgres:{{PostgresImage}}
    environment:
      POSTGRES_DB: ${POSTGRES_DB}
      POSTGRES_USER: ${POSTGRES_USER}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
    ports:
      - "${POSTGRES_PORT:-{{PostgresPort}}}:5432"
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
{{#if Redis}}
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
  {{DockerName}}-postgres-data:
{{#if Redis}}  {{DockerName}}-redis-data:
{{/if}}
networks:
  {{DockerName}}-network:
    driver: bridge
