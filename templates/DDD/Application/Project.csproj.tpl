<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>{{TargetFramework}}</TargetFramework>
    <RootNamespace>{{ApplicationNamespace}}</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\{{DomainProject}}\{{DomainProject}}.csproj" />
  </ItemGroup>

</Project>
