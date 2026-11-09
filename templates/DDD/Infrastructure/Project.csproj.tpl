<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>{{TargetFramework}}</TargetFramework>
    <RootNamespace>{{InfrastructureNamespace}}</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\{{ApplicationProject}}\{{ApplicationProject}}.csproj" />
    <ProjectReference Include="..\{{DomainProject}}\{{DomainProject}}.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="{{EfCoreVersion}}" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="{{EfCoreVersion}}">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
{{#if Postgres}}    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="{{NpgsqlVersion}}" />
{{/if}}{{#if SqlServer}}    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="{{EfCoreVersion}}" />
{{/if}}    <PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" Version="{{ExtensionsVersion}}" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="{{ExtensionsVersion}}" />
{{#if Redis}}    <PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="{{CachingRedisVersion}}" />
    <PackageReference Include="StackExchange.Redis" Version="{{RedisVersion}}" />
{{/if}}  </ItemGroup>

</Project>
