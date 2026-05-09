param(
    [string]$Configuration = "Debug",
    [string]$BaseOutputPath = "",
    [string]$BaseIntermediateOutputPath = "",
    [string]$JazorOutDir = "",
    [switch]$Bundle
)

$ErrorActionPreference = "Stop"

$sampleRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent (Split-Path -Parent $sampleRoot)
$publishScript = Join-Path $repoRoot "scripts\publish-nuget.ps1"
$vuetifyProject = Join-Path $repoRoot "src\ECMAScript.Vuetify\ECMAScript.Vuetify.csproj"
$packageOutput = Join-Path $repoRoot ".tmp\nupkg-sample"
$hostProject = Join-Path $sampleRoot "Todo.Host\Todo.Host.csproj"

$env:DOTNET_CLI_HOME = Join-Path $repoRoot ".dotnet"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$baseOutputPathWasExplicit = $PSBoundParameters.ContainsKey("BaseOutputPath")
$baseIntermediateOutputPathWasExplicit = $PSBoundParameters.ContainsKey("BaseIntermediateOutputPath")
$jazorOutDirWasExplicit = $PSBoundParameters.ContainsKey("JazorOutDir")

if (-not $baseOutputPathWasExplicit) {
    $BaseOutputPath = Join-Path $repoRoot ".tmp\razorvue-todolist-out"
}

if (-not $baseIntermediateOutputPathWasExplicit) {
    $BaseIntermediateOutputPath = Join-Path $repoRoot ".tmp\razorvue-todolist-obj"
}

function Invoke-DotNet {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($Arguments -join ' ') failed with exit code $LASTEXITCODE."
    }
}

if (Test-Path $packageOutput) {
    Remove-Item -LiteralPath $packageOutput -Recurse -Force
}

if ($jazorOutDirWasExplicit -and -not [string]::IsNullOrWhiteSpace($JazorOutDir)) {
    $resolvedRepoRoot = [System.IO.Path]::GetFullPath($repoRoot)
    $resolvedJazorOutDir = [System.IO.Path]::GetFullPath($JazorOutDir)

    if (-not $resolvedJazorOutDir.StartsWith($resolvedRepoRoot, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Explicit JazorOutDir must stay within the repository root: $resolvedJazorOutDir"
    }

    if (Test-Path -LiteralPath $resolvedJazorOutDir) {
        Remove-Item -LiteralPath $resolvedJazorOutDir -Recurse -Force
    }
}

$publishArgs = @{
    Configuration = $Configuration
    OutputDirectory = $packageOutput
    SkipPush = $true
    BaseOutputPath = $BaseOutputPath
    BaseIntermediateOutputPath = $BaseIntermediateOutputPath
}

& $publishScript @publishArgs
if ($LASTEXITCODE -ne 0) {
    throw "publish-nuget.ps1 failed with exit code $LASTEXITCODE."
}

Invoke-DotNet @(
    "pack",
    $vuetifyProject,
    "-c", $Configuration,
    "--no-build",
    "-o", $packageOutput,
    "-p:JazorIsolatedBaseOutputRoot=$BaseOutputPath",
    "-p:JazorIsolatedBaseIntermediateOutputRoot=$BaseIntermediateOutputPath",
    "/nr:false",
    "-p:UseSharedCompilation=false"
)

$nupkg = Get-ChildItem -Path $packageOutput -Filter "Jazor.*.nupkg" -File |
    Where-Object { $_.Name -notlike "*.snupkg" } |
    Sort-Object LastWriteTimeUtc -Descending |
    Select-Object -First 1
if (-not $nupkg) { throw "Packed Jazor package not found under '$packageOutput'." }
$packageVersion = $nupkg.BaseName -replace '^Jazor\.', ''
$packageStamp = $nupkg.LastWriteTimeUtc.ToString("yyyyMMddHHmmssffff")
$restorePackagesPath = Join-Path $repoRoot ".tmp\nuget-sample-packages\$packageVersion-$packageStamp"

$buildArgs = @(
    "build",
    $hostProject,
    "-t:Rebuild",
    "/m:1",
    "/p:BuildInParallel=false",
    "-p:RestoreSources=$packageOutput",
    "-p:RestorePackagesPath=$restorePackagesPath",
    "-p:RestoreForce=true",
    "-p:JazorPackageVersion=$packageVersion"
)

$buildArgs += "-p:JazorIsolatedBaseOutputRoot=$BaseOutputPath"
$buildArgs += "-p:JazorIsolatedBaseIntermediateOutputRoot=$BaseIntermediateOutputPath"

if ($jazorOutDirWasExplicit) {
    $buildArgs += "-p:JazorOutDir=$JazorOutDir"
}

if ($Bundle) {
    $buildArgs += "-p:JazorBundle=true"
}

$buildArgs += "/nr:false"
$buildArgs += "-p:UseSharedCompilation=false"

Invoke-DotNet $buildArgs
