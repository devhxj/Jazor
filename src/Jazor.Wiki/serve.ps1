param(
    [int]$Port = 4173,
    [string]$Configuration = "Debug",
    [switch]$Build,
    [switch]$BuildLocal,
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

$sampleRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent (Split-Path -Parent $sampleRoot)
$hostProject = Join-Path $sampleRoot "Jazor.Wiki.csproj"
$hostRoot = $sampleRoot
$webRoot = Join-Path $hostRoot "wwwroot"
$modulePath = Join-Path $webRoot "jazor\app\main.mjs"

$env:DOTNET_CLI_HOME = Join-Path $repoRoot ".dotnet"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"

function Invoke-DotNet {
    param([string[]]$DotNetArgs)

    dotnet @DotNetArgs
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet command failed with exit code ${LASTEXITCODE}: dotnet $($DotNetArgs -join ' ')"
    }
}

function Invoke-Script {
    param(
        [string]$Path,
        [string[]]$Args
    )

    & $Path @Args
    if ($LASTEXITCODE -ne 0) {
        throw "script failed with exit code ${LASTEXITCODE}: $Path $($Args -join ' ')"
    }
}

if ($BuildLocal) {
    $buildScript = Join-Path $sampleRoot "build-local.ps1"
    Invoke-Script -Path $buildScript -Args @("-Configuration", $Configuration)
} elseif ($Build) {
    Invoke-DotNet @("build", $hostProject, "-c", $Configuration, "/m:1", "/p:BuildInParallel=false")
}

if (-not (Test-Path $modulePath)) {
    throw "Missing emitted module: $modulePath. Run '.\src\Jazor.Wiki\serve.ps1 -Build' or '.\src\Jazor.Wiki\build-local.ps1' first."
}

$url = "http://localhost:$Port/index.html"
$rootUrl = "http://localhost:$Port"
Write-Host "Serving jazor.wiki from: $webRoot"
Write-Host "Open: $url"

if ($DryRun) {
    Write-Host "Dry-run mode: static server was not started."
    return
}

$runArgs = @(
    "run",
    "--project", $hostProject,
    "-c", $Configuration
)

if ($Build -or $BuildLocal) {
    $runArgs += @("--no-build", "--no-restore")
}

$env:ASPNETCORE_URLS = $rootUrl
Invoke-DotNet $runArgs
