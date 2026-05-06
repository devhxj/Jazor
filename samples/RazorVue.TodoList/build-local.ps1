param(
    [string]$Configuration = "Debug",
    [switch]$Bundle
)

$ErrorActionPreference = "Stop"

$sampleRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent (Split-Path -Parent $sampleRoot)
$jazorProject = Join-Path $repoRoot "src\Jazor\Jazor.csproj"
$vuetifyProject = Join-Path $repoRoot "src\ECMAScript.Vuetify\ECMAScript.Vuetify.csproj"
$packageOutput = Join-Path $repoRoot ".tmp\nupkg-sample"
$hostProject = Join-Path $sampleRoot "Todo.Host\Todo.Host.csproj"
$runtimeProject = Join-Path $repoRoot "src\ECMAScript\ECMAScript.csproj"
$contractProject = Join-Path $repoRoot "src\ECMAScript.Contract\ECMAScript.Contract.csproj"
$vue3Project = Join-Path $repoRoot "src\ECMAScript.Vue3\ECMAScript.Vue3.csproj"
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
Invoke-DotNet @("build", $vuetifyProject, "-c", $Configuration, "/m:1", "/p:BuildInParallel=false")
Invoke-DotNet @("build", $analyzerProject, "-c", $Configuration, "/m:1", "/p:BuildInParallel=false")
Invoke-DotNet @("publish", $emitProject, "-c", $Configuration, "-o", $emitPublishDir, "/m:1", "/p:BuildInParallel=false")
Invoke-DotNet @("pack", $jazorProject, "-c", $Configuration, "--no-build", "-o", $packageOutput)
Invoke-DotNet @("pack", $vuetifyProject, "-c", $Configuration, "--no-build", "-o", $packageOutput)

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

if ($Bundle) {
    $buildArgs += "-p:JazorBundle=true"
}

Invoke-DotNet $buildArgs
