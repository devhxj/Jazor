param(
    [string]$Configuration = "Debug",
    [switch]$Bundle
)

$ErrorActionPreference = "Stop"

$sampleRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent (Split-Path -Parent $sampleRoot)
$hostProject = Join-Path $sampleRoot "Wiki.csproj"
$webRoot = Join-Path $sampleRoot "wwwroot"
$mainModulePath = Join-Path $webRoot "jazor\main.mjs"
$componentModulePath = Join-Path $webRoot "jazor\components\wiki-home.mjs"

$env:DOTNET_CLI_HOME = Join-Path $repoRoot ".dotnet"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"

function Invoke-DotNet {
    param([string[]]$DotNetArgs)

    dotnet @DotNetArgs
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet command failed with exit code ${LASTEXITCODE}: dotnet $($DotNetArgs -join ' ')"
    }
}

function Assert-PathExists {
    param(
        [string]$Path,
        [string]$Description
    )

    if (-not (Test-Path $Path)) {
        throw "Missing ${Description}: $Path"
    }
}

$buildArgs = @(
    "build",
    $hostProject,
    "-c", $Configuration,
    "/m:1",
    "/p:BuildInParallel=false"
)

if ($Bundle) {
    $buildArgs += "-p:JazorBundle=true"
}

Invoke-DotNet $buildArgs

Assert-PathExists -Path $mainModulePath -Description "emitted main module"
Assert-PathExists -Path $componentModulePath -Description "emitted wiki component module"
