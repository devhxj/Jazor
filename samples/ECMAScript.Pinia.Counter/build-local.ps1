param(
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"

$sampleRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent (Split-Path -Parent $sampleRoot)
$publishScript = Join-Path $repoRoot "scripts\publish-nuget.ps1"
$packageOutput = Join-Path $repoRoot ".tmp\nupkg-sample"
$hostProject = Join-Path $sampleRoot "Pinia.Counter.Host\Pinia.Counter.Host.csproj"

$env:DOTNET_CLI_HOME = Join-Path $repoRoot ".dotnet"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"

[xml]$sdkProject = Get-Content (Join-Path $repoRoot "src\Jazor\Jazor.csproj")
$packageVersion = $sdkProject.Project.PropertyGroup.Version

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

& $publishScript -Configuration $Configuration -OutputDirectory $packageOutput -SkipPush
if ($LASTEXITCODE -ne 0) {
    throw "publish-nuget.ps1 failed with exit code $LASTEXITCODE."
}

$packagePath = Join-Path $packageOutput "Jazor.$packageVersion.nupkg"
$packageStamp = (Get-Item $packagePath).LastWriteTimeUtc.ToString("yyyyMMddHHmmssffff")
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

Invoke-DotNet $buildArgs
