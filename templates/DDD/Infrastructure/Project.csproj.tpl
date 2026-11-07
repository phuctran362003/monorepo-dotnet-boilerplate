<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>{{TargetFramework}}</TargetFramework>
    <RootNamespace>{{Namespace}}.Infrastructure</RootNamespace>
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
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="{{NpgsqlVersion}}" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" Version="{{ExtensionsVersion}}" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="{{ExtensionsVersion}}" />
{{#if Redis}}    <PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="{{ExtensionsVersion}}" />
    <PackageReference Include="StackExchange.Redis" Version="{{RedisVersion}}" />
{{/if}}  </ItemGroup>

</Project>
