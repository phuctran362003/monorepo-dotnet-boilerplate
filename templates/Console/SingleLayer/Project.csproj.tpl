<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>{{TargetFramework}}</TargetFramework>
    <RootNamespace>{{Namespace}}</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="{{ExtensionsVersion}}" />
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="{{EfCoreVersion}}" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="{{EfCoreVersion}}">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
{{#if Postgres}}    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="{{NpgsqlVersion}}" />
{{/if}}{{#if SqlServer}}    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="{{EfCoreVersion}}" />
{{/if}}    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="{{EfCoreVersion}}">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.Extensions.Configuration" Version="{{ExtensionsVersion}}" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="{{ExtensionsVersion}}" />
    <PackageReference Include="FluentValidation.AspNetCore" Version="{{FluentValidationAspNetCoreVersion}}" />
{{#if Serilog}}    <PackageReference Include="Serilog.AspNetCore" Version="{{SerilogVersion}}" />
{{/if}}{{#if FluentValidation}}    <PackageReference Include="FluentValidation" Version="{{FluentValidationVersion}}" />
    <PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="{{FluentValidationVersion}}" />
{{/if}}{{#if AutoMapper}}    <PackageReference Include="AutoMapper" Version="{{AutoMapperVersion}}" />
{{/if}}{{#if MediatR}}    <PackageReference Include="MediatR" Version="{{MediatRVersion}}" />
{{/if}}{{#if Redis}}    <PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="{{CachingRedisVersion}}" />
    <PackageReference Include="StackExchange.Redis" Version="{{RedisVersion}}" />
{{/if}}  </ItemGroup>

  <ItemGroup>
    <None Update="appsettings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <None Update="appsettings.Development.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>

</Project>
