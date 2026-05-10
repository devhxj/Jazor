param(
    [string]$Configuration = "Debug",
    [string]$BaseOutputPath = "",
    [string]$BaseIntermediateOutputPath = "",
    [switch]$Bundle
)

$ErrorActionPreference = "Stop"

$sampleRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent (Split-Path -Parent $sampleRoot)
$hostProject = Join-Path $sampleRoot "Wiki.csproj"
$jazorRoot = Join-Path $sampleRoot "jazor"
$mainModulePath = Join-Path $jazorRoot "main.mjs"
$componentModulePath = Join-Path $jazorRoot "components\wiki-home.mjs"

$env:DOTNET_CLI_HOME = Join-Path $repoRoot ".dotnet"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$baseOutputPathWasExplicit = $PSBoundParameters.ContainsKey("BaseOutputPath")
$baseIntermediateOutputPathWasExplicit = $PSBoundParameters.ContainsKey("BaseIntermediateOutputPath")

function Get-IsolatedBuildRoot {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $resolvedPath = $Path
    if (-not $resolvedPath.Contains('$(', [StringComparison]::Ordinal)) {
        if (-not [System.IO.Path]::IsPathRooted($resolvedPath)) {
            $resolvedPath = Join-Path $repoRoot $resolvedPath
        }

        $resolvedPath = [System.IO.Path]::GetFullPath($resolvedPath)
    }

    if (-not $resolvedPath.EndsWith('\', [StringComparison]::Ordinal)) {
        $resolvedPath += '\'
    }

    return $resolvedPath
}

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

if ($baseOutputPathWasExplicit) {
    $buildArgs += "-p:JazorIsolatedBaseOutputRoot=$(Get-IsolatedBuildRoot -Path $BaseOutputPath)"
}

if ($baseIntermediateOutputPathWasExplicit) {
    $buildArgs += "-p:JazorIsolatedBaseIntermediateOutputRoot=$(Get-IsolatedBuildRoot -Path $BaseIntermediateOutputPath)"
}

$buildArgs += "/nr:false"
$buildArgs += "-p:UseSharedCompilation=false"

Invoke-DotNet $buildArgs

Assert-PathExists -Path $mainModulePath -Description "emitted main module"
Assert-PathExists -Path $componentModulePath -Description "emitted wiki component module"
