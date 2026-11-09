# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:{{DotNetImageTag}} AS build
WORKDIR /src

COPY Directory.Build.props ./
{{DockerProjectCopies}}
RUN dotnet restore "{{MainProjectPath}}"

COPY . .
RUN dotnet publish "{{MainProjectPath}}" -c Release -o /app/publish /p:UseAppHost=false

{{#if Console}}FROM mcr.microsoft.com/dotnet/runtime:{{DotNetImageTag}} AS final
WORKDIR /app
{{/if}}{{#if !Console}}FROM mcr.microsoft.com/dotnet/aspnet:{{DotNetImageTag}} AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
{{/if}}
{{#if AutoMapper}}
# AutoMapper's license check performs an HTTP request and the .NET HTTP stack
# probes GSSAPI on Linux. Installing the Kerberos runtime avoids a noisy
# "cannot open shared object file: libgssapi_krb5.so.2" line in the logs.
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*
{{/if}}
{{#if !Console}}EXPOSE 8080
{{/if}}COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "{{MainProject}}.dll"]
