<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>{{TargetFramework}}</TargetFramework>
    <RootNamespace>{{Namespace}}</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="{{OpenApiVersion}}" />
    <PackageReference Include="Swashbuckle.AspNetCore.SwaggerUI" Version="{{SwaggerUiVersion}}" />
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
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="{{JwtBearerVersion}}" />
    <PackageReference Include="Microsoft.AspNetCore.OData" Version="{{ODataVersion}}" />
    <PackageReference Include="FluentValidation.AspNetCore" Version="{{FluentValidationAspNetCoreVersion}}" />
{{#if Serilog}}    <PackageReference Include="Serilog.AspNetCore" Version="{{SerilogVersion}}" />
{{/if}}{{#if FluentValidation}}    <PackageReference Include="FluentValidation" Version="{{FluentValidationVersion}}" />
    <PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="{{FluentValidationVersion}}" />
{{/if}}{{#if AutoMapper}}    <PackageReference Include="AutoMapper" Version="{{AutoMapperVersion}}" />
{{/if}}{{#if MediatR}}    <PackageReference Include="MediatR" Version="{{MediatRVersion}}" />
{{/if}}{{#if Redis}}    <PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="{{CachingRedisVersion}}" />
    <PackageReference Include="StackExchange.Redis" Version="{{RedisVersion}}" />
{{/if}}  </ItemGroup>

</Project>
