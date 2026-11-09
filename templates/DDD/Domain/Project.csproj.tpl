<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>{{TargetFramework}}</TargetFramework>
    <RootNamespace>{{DomainNamespace}}</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="{{EfCoreVersion}}" />
{{#if SqlServer}}    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="{{EfCoreVersion}}" />
{{/if}}    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="{{EfCoreVersion}}">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.Extensions.Configuration" Version="{{ExtensionsVersion}}" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="{{ExtensionsVersion}}" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="{{JwtBearerVersion}}" />
  </ItemGroup>

</Project>
