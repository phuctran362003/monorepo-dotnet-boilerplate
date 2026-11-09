<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>{{TargetFramework}}</TargetFramework>
    <RootNamespace>{{InfrastructureNamespace}}</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\{{ApplicationProject}}\{{ApplicationProject}}.csproj" />
    <ProjectReference Include="..\{{DomainProject}}\{{DomainProject}}.csproj" />
  </ItemGroup>

</Project>
