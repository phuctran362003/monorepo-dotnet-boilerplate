#Requires -Version 5.1
[CmdletBinding()]
param(
    [ValidateSet('patch', 'minor', 'major')]
    [string]$Bump = 'patch'
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
try { [Console]::OutputEncoding = [System.Text.Encoding]::UTF8 } catch { }

$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

function Invoke-WithLoading {
    param(
        [Parameter(Mandatory)][string]$Message,
        [Parameter(Mandatory)][scriptblock]$Action
    )

    $sync = [hashtable]::Synchronized(@{ Stop = $false })
    $runspace = [runspacefactory]::CreateRunspace()
    $runspace.Open()
    $runspace.SessionStateProxy.SetVariable('sync', $sync)
    $runspace.SessionStateProxy.SetVariable('Message', $Message)

    $worker = [powershell]::Create()
    $worker.Runspace = $runspace
    [void]$worker.AddScript({
        $frames = @('|', '/', '-', '\')
        $n = 0
        $started = Get-Date
        try {
            [Console]::CursorVisible = $false
        } catch { }
        try {
            while (-not $sync.Stop) {
                $elapsed = [int]((Get-Date) - $started).TotalSeconds
                $frame = $frames[$n % 4]
                [Console]::Write(("`r  {0}  {1}  ({2}s)          " -f $frame, $Message, $elapsed))
                Start-Sleep -Milliseconds 90
                $n++
            }
        } finally {
            try { [Console]::CursorVisible = $true } catch { }
        }
    })

    $handle = $worker.BeginInvoke()
    $failed = $false
    try {
        return & $Action
    } catch {
        $failed = $true
        throw
    } finally {
        $sync.Stop = $true
        Start-Sleep -Milliseconds 120
        try { $worker.EndInvoke($handle) } catch { }
        $worker.Dispose()
        $runspace.Dispose()

        $clear = ' ' * 72
        if ($failed) {
            Write-Host ("`r  [X]  {0}{1}" -f $Message, $clear) -ForegroundColor Red
        } else {
            Write-Host ("`r  [OK] {0}{1}" -f $Message, $clear) -ForegroundColor Green
        }
    }
}

function Invoke-DotNetLogged {
    param(
        [Parameter(Mandatory)][string[]]$Arguments,
        [switch]$AllowFailure
    )

    $previous = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try {
        $lines = & dotnet @Arguments 2>&1 | ForEach-Object {
            if ($_ -is [System.Management.Automation.ErrorRecord]) {
                $_.ToString()
            } else {
                "$_"
            }
        }
    } finally {
        $ErrorActionPreference = $previous
    }

    $output = ($lines -join [Environment]::NewLine).Trim()
    if ($LASTEXITCODE -ne 0 -and -not $AllowFailure) {
        if ([string]::IsNullOrWhiteSpace($output)) {
            $output = "dotnet $($Arguments -join ' ') failed (exit $LASTEXITCODE)."
        }
        throw $output
    }

    return $output
}

function Save-UserNuGetApiKey {
    param([string]$ApiKey)

    [Environment]::SetEnvironmentVariable('NUGET_API_KEY', $ApiKey, 'User')
    $env:NUGET_API_KEY = $ApiKey
}

function Get-NuGetApiKey {
    param([string]$RepoRoot)

    foreach ($candidate in @(
            $env:NUGET_API_KEY
            [Environment]::GetEnvironmentVariable('NUGET_API_KEY', 'User')
        )) {
        if (-not [string]::IsNullOrWhiteSpace($candidate)) {
            $trimmed = $candidate.Trim()
            Save-UserNuGetApiKey -ApiKey $trimmed
            return $trimmed
        }
    }

    $keyFile = Join-Path $RepoRoot '.nuget-api-key'
    if (Test-Path -LiteralPath $keyFile) {
        $fromFile = (Get-Content -LiteralPath $keyFile -Raw).Trim()
        if (-not [string]::IsNullOrWhiteSpace($fromFile)) {
            Save-UserNuGetApiKey -ApiKey $fromFile
            Write-Host "Saved NUGET_API_KEY to this Windows user environment."
            return $fromFile
        }
    }

    Write-Host ""
    Write-Host "NUGET_API_KEY is not set on this machine."
    $entered = Read-Host 'Paste the NuGet API key'
    if ([string]::IsNullOrWhiteSpace($entered)) {
        throw "Cancelled: no API key was entered."
    }

    $entered = $entered.Trim()
    Save-UserNuGetApiKey -ApiKey $entered
    Write-Host "Saved NUGET_API_KEY to this Windows user environment. Later publishes will not ask again."
    return $entered
}

$csproj = Join-Path $root 'src\MonorepoDotnet.CLI\MonorepoDotnet.CLI.csproj'
if (-not (Test-Path -LiteralPath $csproj)) {
    throw "CLI project not found: $csproj"
}

$apiKey = Get-NuGetApiKey -RepoRoot $root

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

switch ($Bump) {
    'major' { $major++; $minor = 0; $patch = 0 }
    'minor' { $minor++; $patch = 0 }
    default { $patch++ }
}

$newVersion = "$major.$minor.$patch"
Write-Host ""
Write-Host "  Version  $current  ->  $newVersion  ($Bump)" -ForegroundColor Cyan
Write-Host ""

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

$packagesDir = Join-Path $root 'artifacts\packages'
New-Item -ItemType Directory -Force -Path $packagesDir | Out-Null
$nupkg = Join-Path $packagesDir "monorepo-dotnet.$newVersion.nupkg"
$cliProject = Join-Path $root 'src\MonorepoDotnet.CLI'

Invoke-WithLoading -Message "Packing global tool" -Action {
    Invoke-DotNetLogged -Arguments @('pack', $cliProject, '-c', 'Release', '-o', $packagesDir)
    if (-not (Test-Path -LiteralPath $nupkg)) {
        throw "Package not found: $nupkg"
    }
}

Invoke-WithLoading -Message "Pushing monorepo-dotnet.$newVersion to nuget.org" -Action {
    Invoke-DotNetLogged -Arguments @(
        'nuget', 'push', $nupkg,
        '--api-key', $apiKey,
        '--source', 'https://api.nuget.org/v3/index.json',
        '--skip-duplicate'
    )
}

Write-Host ""
Write-Host "  nuget.org   [OK]  https://www.nuget.org/packages/monorepo-dotnet/$newVersion" -ForegroundColor Green
Write-Host "  Others: dotnet tool update --global monorepo-dotnet"
Write-Host ""
