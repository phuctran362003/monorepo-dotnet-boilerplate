# Changelog

All notable changes to `monorepo-dotnet` are recorded here. The version in
`src/MonorepoDotnet.CLI/MonorepoDotnet.CLI.csproj` is the released version and
must match the git tag (`v<version>`) and the package on nuget.org.

## 0.1.10 — unreleased

### Fixed

- NuGet package metadata (`Authors`, `Copyright`) was double-encoded mojibake in
  `MonorepoDotnet.CLI.csproj`. The corrupted strings shipped to nuget.org, so the
  package page showed hundreds of garbage characters instead of the author name.
  Both fields are now plain UTF-8. `0.1.9` on nuget.org remains affected; it
  cannot be edited after publication.

### Added

- CI workflow (`.github/workflows/ci.yml`): restore, build, full test suite,
  `dotnet pack`, and a guard that fails when the packed `.nuspec` contains
  mojibake. Runs on every push and pull request to `main`.
- This changelog.

## 0.1.9

- .NET 10 only for generated solutions; Standard (`API` → `Service` →
  `Infrastructure` → `Domain`) default layout; Swagger UI in Development.
- Shipped with corrupted `Authors`/`Copyright` metadata (see 0.1.10).

## Earlier versions

`0.1.0` – `0.1.8` introduced the DDD generator, PostgreSQL/SQL Server, Docker
Compose, Clean Architecture, 1-layer and MVC layouts, and the interactive
Spectre.Console workflow. See git history for details.
