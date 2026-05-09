param(
    [string]$Configuration = "Debug",
    [switch]$Bundle
)

$ErrorActionPreference = "Stop"

$sampleRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent (Split-Path -Parent $sampleRoot)
$packageProject = Join-Path $repoRoot "src\Jazor\Jazor.csproj"
$packageOutput = Join-Path $repoRoot ".tmp\nupkg-sample"
$hostProject = Join-Path $sampleRoot "Sample.Host\Sample.Host.csproj"
$runtimeProject = Join-Path $repoRoot "src\ECMAScript\ECMAScript.csproj"
$analyzerProject = Join-Path $repoRoot "src\Jazor.Analyzer\Jazor.Analyzer.csproj"
$emitProject = Join-Path $repoRoot "src\Jazor.Emit\Jazor.Emit.csproj"
$emitPublishDir = Join-Path $repoRoot "src\Jazor.Emit\bin\$Configuration\net10.0\publish"

$env:DOTNET_CLI_HOME = Join-Path $repoRoot ".dotnet"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"

dotnet build $runtimeProject -c $Configuration /m:1 /p:BuildInParallel=false
dotnet build $analyzerProject -c $Configuration /m:1 /p:BuildInParallel=false
dotnet publish $emitProject -c $Configuration -o $emitPublishDir /m:1 /p:BuildInParallel=false
dotnet pack $packageProject -c $Configuration --no-build -o $packageOutput

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

if ($Bundle) {
    $buildArgs += "-p:JazorBundle=true"
}

dotnet @buildArgs
