@echo off
setlocal EnableExtensions
cd /d "%~dp0"

set "BUMP=%~1"
if "%BUMP%"=="" set "BUMP=patch"

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "$p = Get-Content -LiteralPath '%~f0' -Raw; $i = $p.IndexOf('### POWERSHELL'); if ($i -lt 0) { throw 'Missing PowerShell section.' }; Invoke-Expression $p.Substring($i)"
set "EXITCODE=%ERRORLEVEL%"

echo.
if not "%EXITCODE%"=="0" (
  echo Publish failed.
  pause
  exit /b %EXITCODE%
)

echo Done.
pause
exit /b 0

### POWERSHELL
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

$root = (Get-Location).Path
Set-Location $root

$bump = "$env:BUMP".Trim().ToLowerInvariant()
if ([string]::IsNullOrWhiteSpace($bump)) { $bump = 'patch' }
if ($bump -notin @('patch', 'minor', 'major')) {
    throw "Unknown bump '$bump'. Use patch, minor, or major."
}

$csproj = Join-Path $root 'src\MonorepoDotnet.CLI\MonorepoDotnet.CLI.csproj'
if (-not (Test-Path -LiteralPath $csproj)) {
    throw "CLI project not found: $csproj"
}

$content = Get-Content -LiteralPath $csproj -Raw
$match = [regex]::Match($content, '<Version>([^<]+)</Version>')
if (-not $match.Success) {
    throw "Could not find <Version> in $csproj"
}

$current = $match.Groups[1].Value.Trim()
$core = ($current -split '-', 2)[0]
$bits = @($core.Split('.'))
while ($bits.Count -lt 3) { $bits += '0' }

$major = [int]$bits[0]
$minor = [int]$bits[1]
$patch = [int]$bits[2]

switch ($bump) {
    'major' { $major++; $minor = 0; $patch = 0 }
    'minor' { $minor++; $patch = 0 }
    default { $patch++ }
}

$newVersion = "$major.$minor.$patch"
Write-Host "Version: $current -> $newVersion ($bump)"

$script:replaced = $false
$updated = [regex]::Replace($content, '<Version>[^<]+</Version>', {
    param($m)
    if (-not $script:replaced) {
        $script:replaced = $true
        return "<Version>$newVersion</Version>"
    }
    return $m.Value
})
$utf8 = New-Object System.Text.UTF8Encoding $false
[System.IO.File]::WriteAllText($csproj, $updated, $utf8)

$apiKey = $env:NUGET_API_KEY
$keyFile = Join-Path $root '.nuget-api-key'
if ([string]::IsNullOrWhiteSpace($apiKey) -and (Test-Path -LiteralPath $keyFile)) {
    $apiKey = (Get-Content -LiteralPath $keyFile -Raw).Trim()
}
if ([string]::IsNullOrWhiteSpace($apiKey)) {
    throw "NuGet API key is missing. Set NUGET_API_KEY, or put the key in .nuget-api-key (gitignored). Create a key at https://www.nuget.org/account/apikeys"
}

$packagesDir = Join-Path $root 'artifacts\packages'
New-Item -ItemType Directory -Force -Path $packagesDir | Out-Null

Write-Host "Packing global tool..."
dotnet pack (Join-Path $root 'src\MonorepoDotnet.CLI') -c Release -o $packagesDir
if ($LASTEXITCODE -ne 0) { throw "dotnet pack failed." }

$nupkg = Join-Path $packagesDir "monorepo-dotnet.$newVersion.nupkg"
if (-not (Test-Path -LiteralPath $nupkg)) {
    throw "Package not found: $nupkg"
}

Write-Host "Pushing $nupkg to nuget.org..."
dotnet nuget push $nupkg --api-key $apiKey --source https://api.nuget.org/v3/index.json --skip-duplicate
if ($LASTEXITCODE -ne 0) { throw "dotnet nuget push failed." }

Write-Host "Updating local global tool from the packed nupkg..."
$listed = dotnet tool list --global
if ($LASTEXITCODE -eq 0 -and ($listed | Select-String -SimpleMatch 'monorepo-dotnet')) {
    dotnet tool update --global --add-source $packagesDir monorepo-dotnet
} else {
    dotnet tool install --global --add-source $packagesDir monorepo-dotnet
}
if ($LASTEXITCODE -ne 0) { throw "Local global tool install/update failed." }

Write-Host ""
Write-Host "nuget.org: https://www.nuget.org/packages/monorepo-dotnet/$newVersion"
Write-Host "Indexing can take a few minutes. This machine already has $newVersion from artifacts\packages."
Write-Host "Others: dotnet tool update --global monorepo-dotnet"
