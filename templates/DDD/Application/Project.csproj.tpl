<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>{{TargetFramework}}</TargetFramework>
    <RootNamespace>{{ApplicationNamespace}}</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\{{DomainProject}}\{{DomainProject}}.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="FluentValidation.AspNetCore" Version="{{FluentValidationAspNetCoreVersion}}" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="{{ExtensionsVersion}}" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" Version="{{ExtensionsVersion}}" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="{{ExtensionsVersion}}" />
{{#if FluentValidation}}    <PackageReference Include="FluentValidation" Version="{{FluentValidationVersion}}" />
    <PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="{{FluentValidationVersion}}" />
{{/if}}{{#if AutoMapper}}    <PackageReference Include="AutoMapper" Version="{{AutoMapperVersion}}" />
{{/if}}{{#if MediatR}}    <PackageReference Include="MediatR" Version="{{MediatRVersion}}" />
{{/if}}{{#if Redis}}    <PackageReference Include="Microsoft.Extensions.Caching.Abstractions" Version="{{ExtensionsVersion}}" />
{{/if}}  </ItemGroup>

</Project>
