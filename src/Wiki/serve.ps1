param(
    [int]$Port = 4173,
    [string]$Configuration = "Debug",
    [switch]$Build,
    [switch]$BuildLocal,
    [switch]$Publish,
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

$sampleRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent (Split-Path -Parent $sampleRoot)
$configurationWasExplicit = $PSBoundParameters.ContainsKey("Configuration")
$hostProject = Join-Path $sampleRoot "Wiki.csproj"
$hostRoot = $sampleRoot
$webRoot = Join-Path $hostRoot "wwwroot"
$jazorRoot = Join-Path $hostRoot "jazor"
$publishRoot = Join-Path $repoRoot ".tmp\wiki-publish-preview\$Configuration"
$publishShadowJazorRoot = $null
$mainModulePath = Join-Path $jazorRoot "main.mjs"
$componentModulePath = Join-Path $jazorRoot "components\wiki-home.mjs"

$env:DOTNET_CLI_HOME = Join-Path $repoRoot ".dotnet"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"

if ($Publish -and ($Build -or $BuildLocal)) {
    throw "-Publish already performs its own publish build. Do not combine it with -Build or -BuildLocal."
}

if ($Publish -and -not $configurationWasExplicit) {
    $Configuration = "Release"
    $publishRoot = Join-Path $repoRoot ".tmp\wiki-publish-preview\$Configuration"
}

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

function Remove-DirectorySafely {
    param([string]$Path)

    if (-not (Test-Path $Path)) {
        return
    }

    $resolvedRepoRoot = (Resolve-Path $repoRoot).Path
    $resolvedPath = (Resolve-Path $Path).Path
    if (-not $resolvedPath.StartsWith($resolvedRepoRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to delete outside repository root: $resolvedPath"
    }

    Remove-Item -LiteralPath $resolvedPath -Recurse -Force
}

function Assert-EmittedArtifacts {
    $missingPaths = @()

    if (-not (Test-Path $mainModulePath)) {
        $missingPaths += $mainModulePath
    }

    if (-not (Test-Path $componentModulePath)) {
        $missingPaths += $componentModulePath
    }

    if ($missingPaths.Count -gt 0) {
        $missingList = $missingPaths -join "`n - "
        if ($Publish) {
            throw "Missing published Wiki modules:`n - $missingList`nRun '.\src\Wiki\serve.ps1 -Publish' first."
        }

        throw "Missing emitted Wiki modules:`n - $missingList`nRun '.\src\Wiki\serve.ps1 -Build' or '.\src\Wiki\build-local.ps1' first."
    }
}

if ($Publish) {
    Remove-DirectorySafely -Path $publishRoot
    Invoke-DotNet @("publish", $hostProject, "-c", $Configuration, "-o", $publishRoot, "/m:1", "/p:BuildInParallel=false")

    $hostRoot = $publishRoot
    $webRoot = Join-Path $hostRoot "wwwroot"
    $jazorRoot = Join-Path $webRoot "jazor"
    $publishShadowJazorRoot = Join-Path $hostRoot "jazor"
    $mainModulePath = Join-Path $jazorRoot "main.mjs"
    $componentModulePath = Join-Path $jazorRoot "components\wiki-home.mjs"

    if (Test-Path $publishShadowJazorRoot) {
        throw "Unexpected publish shadow directory: $publishShadowJazorRoot. Published preview must serve /jazor only from wwwroot/jazor."
    }
}
elseif ($BuildLocal) {
    $buildScript = Join-Path $sampleRoot "build-local.ps1"
    Invoke-Script -Path $buildScript -Args @("-Configuration", $Configuration)
} elseif ($Build) {
    Invoke-DotNet @("build", $hostProject, "-c", $Configuration, "/m:1", "/p:BuildInParallel=false")
}

Assert-EmittedArtifacts

$url = "http://localhost:$Port/"
$rootUrl = "http://localhost:$Port"
$routeUrls = @(
    $url,
    "http://localhost:$Port/guides/getting-started",
    "http://localhost:$Port/guides/content-model",
    "http://localhost:$Port/guides/navigation-discovery",
    "http://localhost:$Port/guides/information-architecture",
    "http://localhost:$Port/engineering/h-function-authoring",
    "http://localhost:$Port/engineering/compiler-support-boundary",
    "http://localhost:$Port/engineering/route-catalog-contract",
    "http://localhost:$Port/engineering/host-semantic-seams",
    "http://localhost:$Port/engineering/import-emit-contract",
    "http://localhost:$Port/engineering/runtime-catalog",
    "http://localhost:$Port/operations/content-governance",
    "http://localhost:$Port/operations/deployment",
    "http://localhost:$Port/operations/testing-verification"
)
if ($Publish) {
    Write-Host "Serving published jazor.wiki from: $webRoot"
    Write-Host "Serving published Jazor modules from: $jazorRoot"
    Write-Host "Published preview root: $hostRoot"
}
else {
    Write-Host "Serving jazor.wiki from: $webRoot"
    Write-Host "Serving emitted Jazor modules from: $jazorRoot"
}
Write-Host "Open routes:"
foreach ($routeUrl in $routeUrls) {
    Write-Host " - $routeUrl"
}

if ($DryRun) {
    if ($Publish) {
        Write-Host "Dry-run mode: published preview artifacts exist and the published host was not started."
    }
    else {
        Write-Host "Dry-run mode: emitted modules exist and the static server was not started."
    }
    return
}

if ($Publish) {
    Push-Location $hostRoot
    try {
        Invoke-DotNet @("Wiki.dll", "--urls", $rootUrl)
    }
    finally {
        Pop-Location
    }
}
else {
    $runArgs = @(
        "run",
        "--project", $hostProject,
        "--no-launch-profile",
        "-c", $Configuration
    )

    if ($Build -or $BuildLocal) {
        $runArgs += @("--no-build", "--no-restore")
    }

    $env:ASPNETCORE_URLS = $rootUrl
    Invoke-DotNet $runArgs
}
