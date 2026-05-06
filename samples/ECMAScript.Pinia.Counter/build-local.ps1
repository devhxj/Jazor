param(
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"

$sampleRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent (Split-Path -Parent $sampleRoot)
$jazorProject = Join-Path $repoRoot "src\Jazor\Jazor.csproj"
$packageOutput = Join-Path $repoRoot ".tmp\nupkg-sample"
$hostProject = Join-Path $sampleRoot "Pinia.Counter.Host\Pinia.Counter.Host.csproj"
$runtimeProject = Join-Path $repoRoot "src\ECMAScript\ECMAScript.csproj"
$contractProject = Join-Path $repoRoot "src\ECMAScript.Contract\ECMAScript.Contract.csproj"
$vue3Project = Join-Path $repoRoot "src\ECMAScript.Vue3\ECMAScript.Vue3.csproj"
$piniaProject = Join-Path $repoRoot "src\ECMAScript.Pinia\ECMAScript.Pinia.csproj"
$analyzerProject = Join-Path $repoRoot "src\Jazor.Analyzer\Jazor.Analyzer.csproj"
$emitProject = Join-Path $repoRoot "src\Jazor.Emit\Jazor.Emit.csproj"
$emitPublishDir = Join-Path $repoRoot "src\Jazor.Emit\bin\$Configuration\net10.0\publish"

$env:DOTNET_CLI_HOME = Join-Path $repoRoot ".dotnet"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"

[xml]$sdkProject = Get-Content $jazorProject
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

Invoke-DotNet @("build", $runtimeProject, "-c", $Configuration, "/m:1", "/p:BuildInParallel=false")
Invoke-DotNet @("build", $contractProject, "-c", $Configuration, "/m:1", "/p:BuildInParallel=false")
Invoke-DotNet @("build", $vue3Project, "-c", $Configuration, "/m:1", "/p:BuildInParallel=false")
Invoke-DotNet @("build", $piniaProject, "-c", $Configuration, "/m:1", "/p:BuildInParallel=false")
Invoke-DotNet @("build", $analyzerProject, "-c", $Configuration, "/m:1", "/p:BuildInParallel=false")
Invoke-DotNet @("publish", $emitProject, "-c", $Configuration, "-o", $emitPublishDir, "/m:1", "/p:BuildInParallel=false")
Invoke-DotNet @("pack", $jazorProject, "-c", $Configuration, "--no-build", "-o", $packageOutput)

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
