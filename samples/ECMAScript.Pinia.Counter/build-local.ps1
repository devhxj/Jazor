param(
    [string]$Configuration = "Debug",
    [string]$BaseOutputPath = "",
    [string]$BaseIntermediateOutputPath = "",
    [string]$JazorOutDir = ""
)

$ErrorActionPreference = "Stop"

$sampleRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent (Split-Path -Parent $sampleRoot)
$publishScript = Join-Path $repoRoot "scripts\publish-nuget.ps1"
$packageOutput = Join-Path $repoRoot ".tmp\nupkg-sample"
$hostProject = Join-Path $sampleRoot "Pinia.Counter.Host\Pinia.Counter.Host.csproj"

$env:DOTNET_CLI_HOME = Join-Path $repoRoot ".dotnet"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$baseOutputPathWasExplicit = $PSBoundParameters.ContainsKey("BaseOutputPath")
$baseIntermediateOutputPathWasExplicit = $PSBoundParameters.ContainsKey("BaseIntermediateOutputPath")
$jazorOutDirWasExplicit = $PSBoundParameters.ContainsKey("JazorOutDir")

function Invoke-DotNet {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    $dotnetOutput = @()
    & dotnet @Arguments 2>&1 | Tee-Object -Variable dotnetOutput
    if ($LASTEXITCODE -ne 0) {
        $tail = ($dotnetOutput | ForEach-Object { $_.ToString() } | Select-Object -Last 40) -join [Environment]::NewLine
        if ([string]::IsNullOrWhiteSpace($tail)) {
            throw "dotnet $($Arguments -join ' ') failed with exit code $LASTEXITCODE."
        }

        throw "dotnet $($Arguments -join ' ') failed with exit code $LASTEXITCODE.`nLast output:`n$tail"
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
}

if ($baseOutputPathWasExplicit) {
    $publishArgs["BaseOutputPath"] = $BaseOutputPath
}

if ($baseIntermediateOutputPathWasExplicit) {
    $publishArgs["BaseIntermediateOutputPath"] = $BaseIntermediateOutputPath
}

& $publishScript @publishArgs
if ($LASTEXITCODE -ne 0) {
    throw "publish-nuget.ps1 failed with exit code $LASTEXITCODE."
}

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
    "-c", $Configuration,
    "-t:Rebuild",
    "/m:1",
    "/p:BuildInParallel=false",
    "-p:RestoreSources=$packageOutput",
    "-p:RestorePackagesPath=$restorePackagesPath",
    "-p:RestoreForce=true",
    "-p:JazorPackageVersion=$packageVersion"
)

if ($baseOutputPathWasExplicit) {
    $buildArgs += "-p:JazorIsolatedBaseOutputRoot=$BaseOutputPath"
}

if ($baseIntermediateOutputPathWasExplicit) {
    $buildArgs += "-p:JazorIsolatedBaseIntermediateOutputRoot=$BaseIntermediateOutputPath"
}

if ($jazorOutDirWasExplicit) {
    $buildArgs += "-p:JazorOutDir=$JazorOutDir"
}

$buildArgs += "/nr:false"
$buildArgs += "-p:UseSharedCompilation=false"

Invoke-DotNet $buildArgs
