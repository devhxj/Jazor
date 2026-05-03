param(
    [int]$Port = 4173,
    [string]$Configuration = "Debug",
    [switch]$Build,
    [switch]$BuildLocal,
    [int]$StartupTimeoutSeconds = 30
)

$ErrorActionPreference = "Stop"

$sampleRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent (Split-Path -Parent $sampleRoot)
$hostProject = Join-Path $sampleRoot "Wiki.csproj"
$webRoot = Join-Path $sampleRoot "wwwroot"
$mainModulePath = Join-Path $webRoot "jazor\main.mjs"
$componentModulePath = Join-Path $webRoot "jazor\components\wiki-home.mjs"
$manifestPath = Join-Path $webRoot "jazor\jazor-manifest.json"
$moduleTextPath = Join-Path $webRoot "jazor\components\wiki-home.mjs"
$indexPath = Join-Path $webRoot "index.html"
$stdoutLog = Join-Path $sampleRoot ".wiki-smoke-$PID.stdout.log"
$stderrLog = Join-Path $sampleRoot ".wiki-smoke-$PID.stderr.log"
$rootUrl = "http://localhost:$Port"
$healthUrl = "$rootUrl/health"
$homeUrl = "$rootUrl/"
$docsRoutes = @(
    @{ Url = $homeUrl; Path = "/" },
    @{ Url = "$rootUrl/guides/getting-started"; Path = "/guides/getting-started" },
    @{ Url = "$rootUrl/guides/content-model"; Path = "/guides/content-model" },
    @{ Url = "$rootUrl/engineering/h-function-authoring"; Path = "/engineering/h-function-authoring" },
    @{ Url = "$rootUrl/operations/deployment"; Path = "/operations/deployment" }
)
$unknownRoute = @{ Url = "$rootUrl/guides/missing-page"; Path = "/guides/missing-page" }
$emittedRouteMarkers = @(
    "/guides/getting-started",
    "/guides/content-model",
    "/engineering/h-function-authoring",
    "/operations/deployment"
)
$emittedNavigationMarkers = @(
    "window.history.replaceState",
    "window.history.pushState",
    "window.onpopstate = onPopState",
    "mouseEvent.preventDefault()",
    "window.scrollTo(0, 0)"
)
$emittedDiscoveryMarkers = @(
    "Search docs pages",
    "nav-search-input",
    "nav-search-empty"
)
$emittedSectionNavigationMarkers = @(
    "window.onhashchange = onHashChange",
    "scrollIntoView(true)",
    "toc-link-active",
    "doc-section-active"
)
$emittedPermalinkMarkers = @(
    "window.navigator.clipboard",
    "clipboard.writeText",
    "Copy link",
    "Copied",
    "Link ready",
    "section-permalink-copied",
    "section-permalink-ready"
)

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

function Assert-PathExists {
    param(
        [string]$Path,
        [string]$Description
    )

    if (-not (Test-Path $Path)) {
        throw "Missing ${Description}: $Path"
    }
}

function Assert-Contains {
    param(
        [string]$Text,
        [string]$Snippet,
        [string]$Description
    )

    if (-not $Text.Contains($Snippet)) {
        throw "Missing ${Description}: expected to find '$Snippet'."
    }
}

function Remove-FileWithRetry {
    param(
        [string]$Path,
        [int]$Attempts = 6,
        [int]$DelayMilliseconds = 250
    )

    for ($attempt = 0; $attempt -lt $Attempts; $attempt++) {
        if (-not (Test-Path $Path)) {
            return
        }

        try {
            Remove-Item -LiteralPath $Path -Force
            return
        }
        catch {
            if ($attempt -ge ($Attempts - 1)) {
                throw
            }

            Start-Sleep -Milliseconds $DelayMilliseconds
        }
    }
}

function Wait-ForHttpOk {
    param(
        [string]$Url,
        [System.Diagnostics.Process]$Process,
        [int]$TimeoutSeconds
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        if ($Process.HasExited) {
            throw "Wiki host exited before responding. See logs: $stdoutLog ; $stderrLog"
        }

        try {
            $response = Invoke-WebRequest -Uri $Url -TimeoutSec 3
            if ($response.StatusCode -eq 200) {
                return $response
            }
        }
        catch {
        }

        Start-Sleep -Milliseconds 500
    } while ((Get-Date) -lt $deadline)

    throw "Timed out waiting for $Url. See logs: $stdoutLog ; $stderrLog"
}

if ($BuildLocal) {
    $buildScript = Join-Path $sampleRoot "build-local.ps1"
    Invoke-Script -Path $buildScript -Args @("-Configuration", $Configuration)
}
elseif ($Build) {
    Invoke-DotNet @("build", $hostProject, "-c", $Configuration, "/m:1", "/p:BuildInParallel=false")
}

Assert-PathExists -Path $mainModulePath -Description "emitted main module"
Assert-PathExists -Path $componentModulePath -Description "emitted wiki component module"
Assert-PathExists -Path $manifestPath -Description "emit manifest"
Assert-PathExists -Path $indexPath -Description "static index page"
Assert-PathExists -Path $moduleTextPath -Description "emitted docs shell module"

$indexContent = Get-Content $indexPath -Raw
Assert-Contains -Text $indexContent -Snippet 'id="app"' -Description "Vue mount root in index.html"
Assert-Contains -Text $indexContent -Snippet './jazor/main.mjs' -Description "main module entry in index.html"

$moduleContent = Get-Content $moduleTextPath -Raw
Assert-Contains -Text $moduleContent -Snippet 'Production Docs Built with Vue 3 H Functions' -Description "docs shell title in emitted module"
Assert-Contains -Text $moduleContent -Snippet 'Page Not Found' -Description "not-found title in emitted module"
Assert-Contains -Text $moduleContent -Snippet 'requested-route' -Description "not-found section id in emitted module"
foreach ($routeMarker in $emittedRouteMarkers) {
    Assert-Contains -Text $moduleContent -Snippet $routeMarker -Description "real docs route marker in emitted module"
}
foreach ($navigationMarker in $emittedNavigationMarkers) {
    Assert-Contains -Text $moduleContent -Snippet $navigationMarker -Description "client-side navigation marker in emitted module"
}
foreach ($discoveryMarker in $emittedDiscoveryMarkers) {
    Assert-Contains -Text $moduleContent -Snippet $discoveryMarker -Description "page-discovery filter marker in emitted module"
}
foreach ($sectionNavigationMarker in $emittedSectionNavigationMarkers) {
    Assert-Contains -Text $moduleContent -Snippet $sectionNavigationMarker -Description "section navigation marker in emitted module"
}
foreach ($permalinkMarker in $emittedPermalinkMarkers) {
    Assert-Contains -Text $moduleContent -Snippet $permalinkMarker -Description "section permalink marker in emitted module"
}

$previousAspNetCoreUrls = $env:ASPNETCORE_URLS
$env:ASPNETCORE_URLS = $rootUrl
$process = $null
$keepLogs = $false

try {
    $process = Start-Process `
        -FilePath "dotnet" `
        -ArgumentList @("run", "--project", $hostProject, "--no-launch-profile", "-c", $Configuration, "--no-build", "--no-restore") `
        -WorkingDirectory $sampleRoot `
        -RedirectStandardOutput $stdoutLog `
        -RedirectStandardError $stderrLog `
        -PassThru `
        -WindowStyle Hidden

    $healthResponse = Wait-ForHttpOk -Url $healthUrl -Process $process -TimeoutSeconds $StartupTimeoutSeconds
    $healthBody = $healthResponse.Content.Trim().Trim('"')
    if ($healthBody -ne "ok") {
        throw "Unexpected /health response body: '$($healthResponse.Content.Trim())'"
    }

    foreach ($route in $docsRoutes) {
        $response = Invoke-WebRequest -Uri $route.Url -TimeoutSec 5
        if ($response.StatusCode -ne 200) {
            throw "Unexpected $($route.Path) status code: $($response.StatusCode)"
        }

        Assert-Contains -Text $response.Content -Snippet 'id="app"' -Description "Vue mount root in served route $($route.Path)"
        Assert-Contains -Text $response.Content -Snippet './jazor/main.mjs' -Description "main module entry in served route $($route.Path)"
    }

    $unknownRouteResponse = Invoke-WebRequest -Uri $unknownRoute.Url -TimeoutSec 5
    if ($unknownRouteResponse.StatusCode -ne 200) {
        throw "Unexpected $($unknownRoute.Path) status code: $($unknownRouteResponse.StatusCode)"
    }

    Assert-Contains -Text $unknownRouteResponse.Content -Snippet 'id="app"' -Description "Vue mount root in served unknown route $($unknownRoute.Path)"
    Assert-Contains -Text $unknownRouteResponse.Content -Snippet './jazor/main.mjs' -Description "main module entry in served unknown route $($unknownRoute.Path)"

    Write-Host "Wiki smoke verification passed."
    Write-Host "Verified: build output, emitted module routes, nav/filter/section/permalink contracts, /health, all registered docs routes, and unknown-route fallback"
}
catch {
    $keepLogs = $true
    throw
}
finally {
    if ($process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
        Wait-Process -Id $process.Id -Timeout 5 -ErrorAction SilentlyContinue
    }

    $env:ASPNETCORE_URLS = $previousAspNetCoreUrls

    if (-not $keepLogs) {
        if (Test-Path $stdoutLog) {
            Remove-FileWithRetry -Path $stdoutLog
        }

        if (Test-Path $stderrLog) {
            Remove-FileWithRetry -Path $stderrLog
        }
    }
}
