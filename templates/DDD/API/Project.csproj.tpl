<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>{{TargetFramework}}</TargetFramework>
    <RootNamespace>{{ApiNamespace}}</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\{{ApplicationProject}}\{{ApplicationProject}}.csproj" />
    <ProjectReference Include="..\{{InfrastructureProject}}\{{InfrastructureProject}}.csproj" />
  </ItemGroup>

</Project>
