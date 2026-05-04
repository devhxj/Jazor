param(
    [int]$Port = 4173,
    [string]$Configuration = "Debug",
    [switch]$Build,
    [switch]$BuildLocal,
    [switch]$Publish,
    [int]$StartupTimeoutSeconds = 30
)

$ErrorActionPreference = "Stop"

$sampleRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent (Split-Path -Parent $sampleRoot)
$hostProject = Join-Path $sampleRoot "Wiki.csproj"
$publishRoot = Join-Path $repoRoot ".tmp\wiki-publish-smoke-$PID"
$hostRoot = $sampleRoot
$webRoot = Join-Path $sampleRoot "wwwroot"
$jazorRoot = Join-Path $sampleRoot "jazor"
$publishShadowJazorRoot = $null
$mainModulePath = Join-Path $jazorRoot "main.mjs"
$componentModulePath = Join-Path $jazorRoot "components\wiki-home.mjs"
$manifestPath = Join-Path $jazorRoot "jazor-manifest.json"
$moduleTextPath = Join-Path $jazorRoot "components\wiki-home.mjs"
$indexPath = Join-Path $webRoot "index.html"
$faviconPath = Join-Path $webRoot "favicon.svg"
$stdoutLog = Join-Path $sampleRoot ".wiki-smoke-$PID.stdout.log"
$stderrLog = Join-Path $sampleRoot ".wiki-smoke-$PID.stderr.log"
$rootUrl = "http://localhost:$Port"
$healthUrl = "$rootUrl/health"
$homeUrl = "$rootUrl/"
$docsRoutes = @(
    @{ Url = $homeUrl; Path = "/" },
    @{ Url = "$rootUrl/guides/getting-started"; Path = "/guides/getting-started" },
    @{ Url = "$rootUrl/guides/content-model"; Path = "/guides/content-model" },
    @{ Url = "$rootUrl/guides/navigation-discovery"; Path = "/guides/navigation-discovery" },
    @{ Url = "$rootUrl/guides/information-architecture"; Path = "/guides/information-architecture" },
    @{ Url = "$rootUrl/engineering/h-function-authoring"; Path = "/engineering/h-function-authoring" },
    @{ Url = "$rootUrl/engineering/compiler-support-boundary"; Path = "/engineering/compiler-support-boundary" },
    @{ Url = "$rootUrl/engineering/route-catalog-contract"; Path = "/engineering/route-catalog-contract" },
    @{ Url = "$rootUrl/engineering/host-semantic-seams"; Path = "/engineering/host-semantic-seams" },
    @{ Url = "$rootUrl/engineering/import-emit-contract"; Path = "/engineering/import-emit-contract" },
    @{ Url = "$rootUrl/engineering/runtime-catalog"; Path = "/engineering/runtime-catalog" },
    @{ Url = "$rootUrl/operations/content-governance"; Path = "/operations/content-governance" },
    @{ Url = "$rootUrl/operations/deployment"; Path = "/operations/deployment" },
    @{ Url = "$rootUrl/operations/testing-verification"; Path = "/operations/testing-verification" }
)
$unknownRoute = @{ Url = "$rootUrl/guides/missing-page"; Path = "/guides/missing-page" }
$emittedRouteMarkers = @(
    "/guides/getting-started",
    "/guides/content-model",
    "/guides/navigation-discovery",
    "/guides/information-architecture",
    "/engineering/h-function-authoring",
    "/engineering/compiler-support-boundary",
    "/engineering/route-catalog-contract",
    "/engineering/host-semantic-seams",
    "/engineering/import-emit-contract",
    "/engineering/runtime-catalog",
    "/operations/content-governance",
    "/operations/deployment",
    "/operations/testing-verification"
)
$emittedPageTitleMarkers = @(
    "Overview",
    "Getting Started",
    "Content Model",
    "Navigation and Discovery",
    "Information Architecture",
    "H-Function Authoring",
    "Compiler Support Boundary",
    "Route Catalog Contract",
    "Host Semantic Seams",
    "Import and Emit Contract",
    "CLR Runtime Catalog",
    "Content Governance",
    "Deployment",
    "Testing and Verification"
)
$emittedSectionContractMarkers = @(
    "what-ships-now",
    "boot-the-site",
    "page-contract",
    "left-rail",
    "concern-groups",
    "layout-composition",
    "controlled-domain",
    "single-source",
    "why-seams-exist",
    "boundary-split",
    "why-catalog-exists",
    "ownership-model",
    "build-output",
    "verification-layers"
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
$emittedRelatedPageMarkers = @(
    "Related pages",
    "route-card",
    "related-pages-panel"
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
$mainEntryMarkers = @(
    'createApp(',
    'app.mount("#app")'
)
$forbiddenBrowserEntryMarkers = @(
    'vuetify',
    'materialdesignicons'
)
$browserAssetChecks = @(
    @{ Url = "$rootUrl/jazor/main.mjs"; Path = "/jazor/main.mjs"; Snippet = 'createApp(' },
    @{ Url = "$rootUrl/jazor/components/wiki-home.mjs"; Path = "/jazor/components/wiki-home.mjs"; Snippet = 'Search docs pages' },
    @{ Url = "$rootUrl/jazor/System/StringModule.js"; Path = "/jazor/System/StringModule.js"; Snippet = 'export' },
    @{ Url = "$rootUrl/site.css"; Path = "/site.css"; Snippet = '.wiki-shell' },
    @{ Url = "$rootUrl/favicon.svg"; Path = "/favicon.svg"; Snippet = '<svg' }
)

$env:DOTNET_CLI_HOME = Join-Path $repoRoot ".dotnet"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"

if ($Publish -and ($Build -or $BuildLocal)) {
    throw "-Publish already performs its own publish build. Do not combine it with -Build or -BuildLocal."
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

function Assert-NotContains {
    param(
        [string]$Text,
        [string]$Snippet,
        [string]$Description
    )

    if ($Text.Contains($Snippet)) {
        throw "Unexpected ${Description}: found '$Snippet'."
    }
}

function Assert-ImportedSystemModulesExist {
    param(
        [string]$Text,
        [string]$Description
    )

    $matches = [regex]::Matches($Text, 'from "(System/[^"]+\.js)"')
    foreach ($match in $matches) {
        $relativePath = $match.Groups[1].Value
        $physicalPath = Join-Path $jazorRoot $relativePath.Replace('/', '\')
        Assert-PathExists -Path $physicalPath -Description "$Description dependency $relativePath"
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

function Remove-PathWithRetry {
    param(
        [string]$Path,
        [switch]$Recurse,
        [int]$Attempts = 6,
        [int]$DelayMilliseconds = 250
    )

    for ($attempt = 0; $attempt -lt $Attempts; $attempt++) {
        if (-not (Test-Path $Path)) {
            return
        }

        try {
            if ($Recurse) {
                Remove-Item -LiteralPath $Path -Recurse -Force
            }
            else {
                Remove-Item -LiteralPath $Path -Force
            }

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

if ($Publish) {
    Invoke-DotNet @("publish", $hostProject, "-c", $Configuration, "-o", $publishRoot, "/m:1", "/p:BuildInParallel=false")

    $hostRoot = $publishRoot
    $webRoot = Join-Path $hostRoot "wwwroot"
    $jazorRoot = Join-Path $webRoot "jazor"
    $publishShadowJazorRoot = Join-Path $hostRoot "jazor"
    $mainModulePath = Join-Path $jazorRoot "main.mjs"
    $componentModulePath = Join-Path $jazorRoot "components\wiki-home.mjs"
    $manifestPath = Join-Path $jazorRoot "jazor-manifest.json"
    $moduleTextPath = Join-Path $jazorRoot "components\wiki-home.mjs"
    $indexPath = Join-Path $webRoot "index.html"
    $faviconPath = Join-Path $webRoot "favicon.svg"
    $stdoutLog = Join-Path $hostRoot ".wiki-publish-smoke.stdout.log"
    $stderrLog = Join-Path $hostRoot ".wiki-publish-smoke.stderr.log"
}
elseif ($BuildLocal) {
    $buildScript = Join-Path $sampleRoot "build-local.ps1"
    Invoke-Script -Path $buildScript -Args @("-Configuration", $Configuration)
}
elseif ($Build) {
    Invoke-DotNet @("build", $hostProject, "-c", $Configuration, "/m:1", "/p:BuildInParallel=false")
}

if ($Publish -and $publishShadowJazorRoot -and (Test-Path $publishShadowJazorRoot)) {
    throw "Unexpected publish shadow directory: $publishShadowJazorRoot. Publish output must serve /jazor only from wwwroot/jazor."
}

Assert-PathExists -Path $mainModulePath -Description "emitted main module"
Assert-PathExists -Path $componentModulePath -Description "emitted wiki component module"
Assert-PathExists -Path $manifestPath -Description "emit manifest"
Assert-PathExists -Path $indexPath -Description "static index page"
Assert-PathExists -Path $faviconPath -Description "favicon asset"
Assert-PathExists -Path $moduleTextPath -Description "emitted docs shell module"

$indexContent = Get-Content $indexPath -Raw
Assert-Contains -Text $indexContent -Snippet 'id="app"' -Description "Vue mount root in index.html"
Assert-Contains -Text $indexContent -Snippet '/site.css' -Description "root stylesheet entry in index.html"
Assert-Contains -Text $indexContent -Snippet '/favicon.svg' -Description "favicon entry in index.html"
Assert-Contains -Text $indexContent -Snippet '/jazor/main.mjs' -Description "root main module entry in index.html"
Assert-Contains -Text $indexContent -Snippet '"System/": "/jazor/System/"' -Description "CLR runtime import-map entry in index.html"
foreach ($forbiddenBrowserEntryMarker in $forbiddenBrowserEntryMarkers) {
    Assert-NotContains -Text $indexContent -Snippet $forbiddenBrowserEntryMarker -Description "forbidden browser entry marker in index.html"
}

$mainModuleContent = Get-Content $mainModulePath -Raw
foreach ($mainEntryMarker in $mainEntryMarkers) {
    Assert-Contains -Text $mainModuleContent -Snippet $mainEntryMarker -Description "main entry marker in emitted module"
}
foreach ($forbiddenBrowserEntryMarker in $forbiddenBrowserEntryMarkers) {
    Assert-NotContains -Text $mainModuleContent -Snippet $forbiddenBrowserEntryMarker -Description "forbidden browser entry marker in emitted main module"
}
Assert-ImportedSystemModulesExist -Text $mainModuleContent -Description "emitted main module"

$moduleContent = Get-Content $moduleTextPath -Raw
Assert-ImportedSystemModulesExist -Text $moduleContent -Description "emitted docs shell module"
Assert-Contains -Text $moduleContent -Snippet 'Production Docs Built with Vue 3 H Functions' -Description "docs shell title in emitted module"
Assert-Contains -Text $moduleContent -Snippet 'Page Not Found' -Description "not-found title in emitted module"
Assert-Contains -Text $moduleContent -Snippet 'requested-route' -Description "not-found section id in emitted module"
foreach ($routeMarker in $emittedRouteMarkers) {
    Assert-Contains -Text $moduleContent -Snippet $routeMarker -Description "real docs route marker in emitted module"
}
foreach ($pageTitleMarker in $emittedPageTitleMarkers) {
    Assert-Contains -Text $moduleContent -Snippet $pageTitleMarker -Description "page title marker in emitted module"
}
foreach ($sectionContractMarker in $emittedSectionContractMarkers) {
    Assert-Contains -Text $moduleContent -Snippet $sectionContractMarker -Description "section contract marker in emitted module"
}
foreach ($navigationMarker in $emittedNavigationMarkers) {
    Assert-Contains -Text $moduleContent -Snippet $navigationMarker -Description "client-side navigation marker in emitted module"
}
foreach ($discoveryMarker in $emittedDiscoveryMarkers) {
    Assert-Contains -Text $moduleContent -Snippet $discoveryMarker -Description "page-discovery filter marker in emitted module"
}
foreach ($relatedPageMarker in $emittedRelatedPageMarkers) {
    Assert-Contains -Text $moduleContent -Snippet $relatedPageMarker -Description "related-page navigation marker in emitted module"
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
    $startFilePath = "dotnet"
    $startArgumentList = @("run", "--project", $hostProject, "--no-launch-profile", "-c", $Configuration, "--no-build", "--no-restore")
    $startWorkingDirectory = $sampleRoot

    if ($Publish) {
        $startArgumentList = @("Wiki.dll", "--urls", $rootUrl)
        $startWorkingDirectory = $hostRoot
    }

    $process = Start-Process `
        -FilePath $startFilePath `
        -ArgumentList $startArgumentList `
        -WorkingDirectory $startWorkingDirectory `
        -RedirectStandardOutput $stdoutLog `
        -RedirectStandardError $stderrLog `
        -PassThru `
        -WindowStyle Hidden

    $healthResponse = Wait-ForHttpOk -Url $healthUrl -Process $process -TimeoutSeconds $StartupTimeoutSeconds
    $healthBody = $healthResponse.Content.Trim().Trim('"')
    if ($healthBody -ne "ok") {
        throw "Unexpected /health response body: '$($healthResponse.Content.Trim())'"
    }

    foreach ($asset in $browserAssetChecks) {
        $assetResponse = Invoke-WebRequest -Uri $asset.Url -TimeoutSec 5
        if ($assetResponse.StatusCode -ne 200) {
            throw "Unexpected $($asset.Path) status code: $($assetResponse.StatusCode)"
        }

        Assert-Contains -Text $assetResponse.Content -Snippet $asset.Snippet -Description "served browser asset $($asset.Path)"
    }

    foreach ($route in $docsRoutes) {
        $response = Invoke-WebRequest -Uri $route.Url -TimeoutSec 5
        if ($response.StatusCode -ne 200) {
            throw "Unexpected $($route.Path) status code: $($response.StatusCode)"
        }

        Assert-Contains -Text $response.Content -Snippet 'id="app"' -Description "Vue mount root in served route $($route.Path)"
        Assert-Contains -Text $response.Content -Snippet '/jazor/main.mjs' -Description "root main module entry in served route $($route.Path)"
        Assert-Contains -Text $response.Content -Snippet '"System/": "/jazor/System/"' -Description "CLR runtime import-map entry in served route $($route.Path)"
    }

    $unknownRouteResponse = Invoke-WebRequest -Uri $unknownRoute.Url -TimeoutSec 5
    if ($unknownRouteResponse.StatusCode -ne 200) {
        throw "Unexpected $($unknownRoute.Path) status code: $($unknownRouteResponse.StatusCode)"
    }

    Assert-Contains -Text $unknownRouteResponse.Content -Snippet 'id="app"' -Description "Vue mount root in served unknown route $($unknownRoute.Path)"
    Assert-Contains -Text $unknownRouteResponse.Content -Snippet '/jazor/main.mjs' -Description "root main module entry in served unknown route $($unknownRoute.Path)"
    Assert-Contains -Text $unknownRouteResponse.Content -Snippet '"System/": "/jazor/System/"' -Description "CLR runtime import-map entry in served unknown route $($unknownRoute.Path)"

    if ($Publish) {
        Write-Host "Wiki publish smoke verification passed."
        Write-Host "Verified: publish output materialization, published /jazor browser asset routes, CLR runtime import-map wiring, published docs routes, /health, and unknown-route fallback"
    }
    else {
        Write-Host "Wiki smoke verification passed."
        Write-Host "Verified: build output, browser asset routes, CLR runtime import-map wiring, emitted module routes, /health, all registered docs routes, and unknown-route fallback"
    }
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

        if ($Publish -and (Test-Path $publishRoot)) {
            Remove-PathWithRetry -Path $publishRoot -Recurse
        }
    }
}
