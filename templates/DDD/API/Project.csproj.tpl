<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>{{TargetFramework}}</TargetFramework>
    <RootNamespace>{{ApiNamespace}}</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\{{ApplicationProject}}\{{ApplicationProject}}.csproj" />
    <ProjectReference Include="..\{{InfrastructureProject}}\{{InfrastructureProject}}.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="{{OpenApiVersion}}" />
    <PackageReference Include="Swashbuckle.AspNetCore.SwaggerUI" Version="{{SwaggerUiVersion}}" />
    <PackageReference Include="Microsoft.AspNetCore.OData" Version="{{ODataVersion}}" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="{{EfCoreVersion}}">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
{{#if Serilog}}    <PackageReference Include="Serilog.AspNetCore" Version="{{SerilogVersion}}" />
{{/if}}  </ItemGroup>

</Project>
