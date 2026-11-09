<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>{{TargetFramework}}</TargetFramework>
    <RootNamespace>{{ApiNamespace}}</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\{{ApplicationProject}}\{{ApplicationProject}}.csproj" />
    <ProjectReference Include="..\{{InfrastructureProject}}\{{InfrastructureProject}}.csproj" />
  </ItemGroup>

  <ItemGroup>
    <None Update="appsettings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <None Update="appsettings.Development.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>

</Project>
