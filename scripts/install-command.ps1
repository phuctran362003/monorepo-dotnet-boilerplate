param(
    [Parameter(Mandatory = $true)]
    [string] $ExePath
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $ExePath)) {
    throw "Published executable not found: $ExePath"
}

$resolved = (Resolve-Path -LiteralPath $ExePath).Path
$commandName = 'monorepo.exe'
$toolsDir = Join-Path $env:USERPROFILE '.dotnet\tools'

if (-not (Test-Path -LiteralPath $toolsDir)) {
    New-Item -ItemType Directory -Path $toolsDir -Force | Out-Null
}

$destination = Join-Path $toolsDir $commandName
Copy-Item -LiteralPath $resolved -Destination $destination -Force

$userPath = [Environment]::GetEnvironmentVariable('Path', 'User')
if ([string]::IsNullOrEmpty($userPath)) {
    $userPath = ''
}

$normalizedTools = $toolsDir.TrimEnd('\')
$alreadyOnUserPath = $userPath.Split(';', [System.StringSplitOptions]::RemoveEmptyEntries) |
    Where-Object { $_.TrimEnd('\') -ieq $normalizedTools }

if (-not $alreadyOnUserPath) {
    $newPath = if ([string]::IsNullOrWhiteSpace($userPath)) { $toolsDir } else { "$userPath;$toolsDir" }
    [Environment]::SetEnvironmentVariable('Path', $newPath, 'User')
}

$env:Path = "$toolsDir;$env:Path"

Write-Host "Installed command: monorepo"
Write-Host "  $destination"
Write-Host "Open any terminal and run: monorepo"
