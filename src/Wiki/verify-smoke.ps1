param(
    [int]$Port = 4173,
    [string]$Configuration = "Debug",
    [string]$BaseOutputPath = "",
    [string]$BaseIntermediateOutputPath = "",
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
    @{ Url = "$rootUrl/search"; Path = "/search" },
    @{ Url = "$rootUrl/guides/getting-started"; Path = "/guides/getting-started" },
    @{ Url = "$rootUrl/guides/project-lines"; Path = "/guides/project-lines" },
    @{ Url = "$rootUrl/guides/content-model"; Path = "/guides/content-model" },
    @{ Url = "$rootUrl/guides/navigation-discovery"; Path = "/guides/navigation-discovery" },
    @{ Url = "$rootUrl/guides/information-architecture"; Path = "/guides/information-architecture" },
    @{ Url = "$rootUrl/guides/topic-index"; Path = "/guides/topic-index" },
    @{ Url = "$rootUrl/guides/glossary"; Path = "/guides/glossary" },
    @{ Url = "$rootUrl/guides/faq"; Path = "/guides/faq" },
    @{ Url = "$rootUrl/guides/troubleshooting"; Path = "/guides/troubleshooting" },
    @{ Url = "$rootUrl/engineering/h-function-authoring"; Path = "/engineering/h-function-authoring" },
    @{ Url = "$rootUrl/engineering/compiler-overview"; Path = "/engineering/compiler-overview" },
    @{ Url = "$rootUrl/engineering/compiler-support-boundary"; Path = "/engineering/compiler-support-boundary" },
    @{ Url = "$rootUrl/engineering/route-catalog-contract"; Path = "/engineering/route-catalog-contract" },
    @{ Url = "$rootUrl/engineering/host-semantic-seams"; Path = "/engineering/host-semantic-seams" },
    @{ Url = "$rootUrl/engineering/import-emit-contract"; Path = "/engineering/import-emit-contract" },
    @{ Url = "$rootUrl/engineering/runtime-catalog"; Path = "/engineering/runtime-catalog" },
    @{ Url = "$rootUrl/engineering/jolt-host"; Path = "/engineering/jolt-host" },
    @{ Url = "$rootUrl/engineering/razorvue-library-mode"; Path = "/engineering/razorvue-library-mode" },
    @{ Url = "$rootUrl/engineering/vueroute-bindings"; Path = "/engineering/vueroute-bindings" },
    @{ Url = "$rootUrl/operations/content-governance"; Path = "/operations/content-governance" },
    @{ Url = "$rootUrl/operations/deployment"; Path = "/operations/deployment" },
    @{ Url = "$rootUrl/operations/testing-verification"; Path = "/operations/testing-verification" }
)
$searchQueryRoute = @{ Url = "$rootUrl/search?q=compiler"; Path = "/search?q=compiler" }
$unknownRoute = @{ Url = "$rootUrl/guides/missing-page"; Path = "/guides/missing-page" }
$emittedRouteMarkers = @(
    "/search",
    "/guides/getting-started",
    "/guides/project-lines",
    "/guides/content-model",
    "/guides/navigation-discovery",
    "/guides/information-architecture",
    "/guides/topic-index",
    "/guides/glossary",
    "/guides/faq",
    "/guides/troubleshooting",
    "/engineering/h-function-authoring",
    "/engineering/compiler-overview",
    "/engineering/compiler-support-boundary",
    "/engineering/route-catalog-contract",
    "/engineering/host-semantic-seams",
    "/engineering/import-emit-contract",
    "/engineering/runtime-catalog",
    "/engineering/jolt-host",
    "/engineering/razorvue-library-mode",
    "/engineering/vueroute-bindings",
    "/operations/content-governance",
    "/operations/deployment",
    "/operations/testing-verification"
)
$emittedPageTitleMarkers = @(
    "Overview",
    "Search",
    "Getting Started",
    "Project Lines",
    "Content Model",
    "Navigation and Discovery",
    "Information Architecture",
    "Topic Index",
    "Glossary",
    "FAQ",
    "Troubleshooting",
    "H-Function Authoring",
    "Compiler Overview",
    "Compiler Support Boundary",
    "Route Catalog Contract",
    "Host Semantic Seams",
    "Import and Emit Contract",
    "CLR Runtime Catalog",
    "Jolt Host",
    "RazorVue Library Mode",
    "VueRoute Bindings",
    "Content Governance",
    "Deployment",
    "Testing and Verification"
)
$emittedSectionContractMarkers = @(
    "what-ships-now",
    "full-text",
    "boot-the-site",
    "two-lines",
    "page-contract",
    "left-rail",
    "concern-groups",
    "topic-clusters",
    "compiler-terms",
    "using-jazor",
    "route-and-host",
    "layout-composition",
    "what-it-is",
    "controlled-domain",
    "single-source",
    "why-seams-exist",
    "boundary-split",
    "why-catalog-exists",
    "why-jolt",
    "why-razorvue",
    "why-vueroute-exists",
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
    "nav-search-empty",
    "nav-search-hint",
    "Press / or Ctrl+K to focus search. Press Escape to clear or exit.",
    "window.onkeydown = onGlobalKeyDown",
    "wiki-nav-search-input"
)
$emittedRelatedPageMarkers = @(
    "Related pages",
    "route-card",
    "related-pages-panel"
)
$emittedSectionNavigationMarkers = @(
    "window.onhashchange = onHashChange",
    "window.onscroll = onScroll",
    "requestAnimationFrame(syncActiveSectionOnFrame)",
    "storedScrollRouteKeys",
    "scrollIntoView(true)",
    "focus({ preventScroll: true, focusVisible: true })",
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
$emittedCodeCopyMarkers = @(
    "Copy code",
    "Copy unavailable",
    "code-copy-button",
    "code-copy-button-copied",
    "code-copy-button-unavailable",
    "code-block-"
)
$emittedProductShellMarkers = @(
    "Theme: Dark",
    "Reading progress",
    "Copy page link",
    "View source",
    "Report issue",
    "Owner",
    "Audience",
    "Reading time",
    "Page feedback",
    "Helpful",
    "Needs work",
    "breadcrumbs",
    "drawer-backdrop",
    "utility-button"
)
$emittedSearchMarkers = @(
    "Full-text search",
    "Section matches",
    "Current search URL",
    "search-shell-card",
    "search-result-card",
    "search-mark"
)
$indexMetaMarkers = @(
    'meta name="description"',
    'link rel="canonical"',
    'meta property="og:title"',
    'meta property="og:description"',
    'meta property="og:url"',
    'meta name="twitter:title"',
    'meta name="twitter:description"'
)
$siteCssMarkers = @(
    '.skip-link',
    '.breadcrumbs',
    '.meta-card',
    '.feedback-button',
    '.reading-progress-track',
    '.reading-progress-bar',
    '.search-result-card',
    '.mobile-utility-bar',
    '.drawer-backdrop',
    'html[data-theme="light"]'
)
$mainEntryMarkers = @(
    'createApp(',
    'app.mount("#app")'
)
$forbiddenBrowserEntryMarkers = @(
    'vuetify',
    'materialdesignicons'
)
$vendorLocalMarkers = @(
    '/vendor/vue@3.5.16.mjs'
)
$forbiddenCdnMarkers = @(
    'unpkg.com'
)
$browserAssetChecks = @(
    @{ Url = "$rootUrl/jazor/main.mjs"; Path = "/jazor/main.mjs"; Snippet = 'createApp(' },
    @{ Url = "$rootUrl/jazor/components/wiki-home.mjs"; Path = "/jazor/components/wiki-home.mjs"; Snippet = 'Search docs pages' },
    @{ Url = "$rootUrl/jazor/System/StringModule.js"; Path = "/jazor/System/StringModule.js"; Snippet = 'export' },
    @{ Url = "$rootUrl/site.css"; Path = "/site.css"; Snippet = '.wiki-shell' },
    @{ Url = "$rootUrl/favicon.svg"; Path = "/favicon.svg"; Snippet = '<svg' },
    @{ Url = "$rootUrl/vendor/vue@3.5.16.mjs"; Path = "/vendor/vue@3.5.16.mjs"; Snippet = 'createApp(' }
)

$env:DOTNET_CLI_HOME = Join-Path $repoRoot ".dotnet"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$baseOutputPathWasExplicit = $PSBoundParameters.ContainsKey("BaseOutputPath")
$baseIntermediateOutputPathWasExplicit = $PSBoundParameters.ContainsKey("BaseIntermediateOutputPath")

if ($Publish -and ($Build -or $BuildLocal)) {
    throw "-Publish already performs its own publish build. Do not combine it with -Build or -BuildLocal."
}

if ($Publish -and -not $PSBoundParameters.ContainsKey("Configuration")) {
    $Configuration = "Release"
}

function Invoke-DotNet {
    param([string[]]$DotNetArgs)

    if ($baseOutputPathWasExplicit) {
        $DotNetArgs += "-p:JazorIsolatedBaseOutputRoot=$BaseOutputPath"
    }

    if ($baseIntermediateOutputPathWasExplicit) {
        $DotNetArgs += "-p:JazorIsolatedBaseIntermediateOutputRoot=$BaseIntermediateOutputPath"
    }

    $DotNetArgs += "/nr:false"
    $DotNetArgs += "-p:UseSharedCompilation=false"

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
    $buildLocalArgs = @("-Configuration", $Configuration)
    if ($baseOutputPathWasExplicit) {
        $buildLocalArgs += @("-BaseOutputPath", $BaseOutputPath)
    }
    if ($baseIntermediateOutputPathWasExplicit) {
        $buildLocalArgs += @("-BaseIntermediateOutputPath", $BaseIntermediateOutputPath)
    }

    Invoke-Script -Path $buildScript -Args $buildLocalArgs
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
foreach ($indexMetaMarker in $indexMetaMarkers) {
    Assert-Contains -Text $indexContent -Snippet $indexMetaMarker -Description "meta contract marker in index.html"
}
foreach ($forbiddenBrowserEntryMarker in $forbiddenBrowserEntryMarkers) {
    Assert-NotContains -Text $indexContent -Snippet $forbiddenBrowserEntryMarker -Description "forbidden browser entry marker in index.html"
}
foreach ($vendorLocalMarker in $vendorLocalMarkers) {
    Assert-Contains -Text $indexContent -Snippet $vendorLocalMarker -Description "vendored dependency marker in index.html"
}
foreach ($forbiddenCdnMarker in $forbiddenCdnMarkers) {
    Assert-NotContains -Text $indexContent -Snippet $forbiddenCdnMarker -Description "forbidden CDN URL in index.html"
}
Assert-PathExists -Path (Join-Path $webRoot "vendor\vue@3.5.16.mjs") -Description "vendored Vue ESM module"

$siteCssContent = Get-Content (Join-Path $webRoot "site.css") -Raw
foreach ($siteCssMarker in $siteCssMarkers) {
    Assert-Contains -Text $siteCssContent -Snippet $siteCssMarker -Description "Wiki shell CSS marker"
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
foreach ($codeCopyMarker in $emittedCodeCopyMarkers) {
    Assert-Contains -Text $moduleContent -Snippet $codeCopyMarker -Description "code copy marker in emitted module"
}
foreach ($productShellMarker in $emittedProductShellMarkers) {
    Assert-Contains -Text $moduleContent -Snippet $productShellMarker -Description "product shell marker in emitted module"
}
foreach ($searchMarker in $emittedSearchMarkers) {
    Assert-Contains -Text $moduleContent -Snippet $searchMarker -Description "search experience marker in emitted module"
}

$expectedPageSourceFiles = @(
    "WikiHomeModule.Overview.cs",
    "WikiHomeModule.Search.cs",
    "WikiHomeModule.GettingStarted.cs",
    "WikiHomeModule.ProjectLines.cs",
    "WikiHomeModule.ContentModel.cs",
    "WikiHomeModule.NavigationDiscovery.cs",
    "WikiHomeModule.InformationArchitecture.cs",
    "WikiHomeModule.TopicIndex.cs",
    "WikiHomeModule.Glossary.cs",
    "WikiHomeModule.Faq.cs",
    "WikiHomeModule.Troubleshooting.cs",
    "WikiHomeModule.HFunctionAuthoring.cs",
    "WikiHomeModule.CompilerOverview.cs",
    "WikiHomeModule.CompilerBoundary.cs",
    "WikiHomeModule.RouteCatalogContract.cs",
    "WikiHomeModule.HostSemanticSeams.cs",
    "WikiHomeModule.ImportEmitContract.cs",
    "WikiHomeModule.RuntimeCatalog.cs",
    "WikiHomeModule.JoltHost.cs",
    "WikiHomeModule.RazorVueLibraryMode.cs",
    "WikiHomeModule.VueRouteBindings.cs",
    "WikiHomeModule.ContentGovernance.cs",
    "WikiHomeModule.Deployment.cs",
    "WikiHomeModule.TestingVerification.cs"
)
foreach ($expectedPageFile in $expectedPageSourceFiles) {
    Assert-PathExists -Path (Join-Path $sampleRoot $expectedPageFile) -Description "expected page source file"
}
$onDiskPageFiles = Get-ChildItem -Path $sampleRoot -Filter "WikiHomeModule.*.cs" -Name |
    Where-Object { $_ -notin @("WikiHomeModule.cs", "WikiHomeModule.RouteContract.cs", "WikiHomeModule.Elements.cs") }
$expectedFileNames = $expectedPageSourceFiles | ForEach-Object { $_ }
$missingFromSmoke = $expectedFileNames | Where-Object { $_ -notin $onDiskPageFiles }
$extraOnDisk = $onDiskPageFiles | Where-Object { $_ -notin $expectedFileNames }
if ($missingFromSmoke.Count -gt 0) {
    throw "Page source files registered in smoke but missing from disk: $($missingFromSmoke -join ', ')"
}
if ($extraOnDisk.Count -gt 0) {
    Write-Warning "Page source files on disk but not registered in smoke drift check: $($extraOnDisk -join ', ')"
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

    $searchQueryResponse = Invoke-WebRequest -Uri $searchQueryRoute.Url -TimeoutSec 5
    if ($searchQueryResponse.StatusCode -ne 200) {
        throw "Unexpected $($searchQueryRoute.Path) status code: $($searchQueryResponse.StatusCode)"
    }

    Assert-Contains -Text $searchQueryResponse.Content -Snippet 'id="app"' -Description "Vue mount root in served route $($searchQueryRoute.Path)"
    Assert-Contains -Text $searchQueryResponse.Content -Snippet '/jazor/main.mjs' -Description "root main module entry in served route $($searchQueryRoute.Path)"
    Assert-Contains -Text $searchQueryResponse.Content -Snippet '"System/": "/jazor/System/"' -Description "CLR runtime import-map entry in served route $($searchQueryRoute.Path)"

    $unknownRouteResponse = Invoke-WebRequest -Uri $unknownRoute.Url -TimeoutSec 5
    if ($unknownRouteResponse.StatusCode -ne 200) {
        throw "Unexpected $($unknownRoute.Path) status code: $($unknownRouteResponse.StatusCode)"
    }

    Assert-Contains -Text $unknownRouteResponse.Content -Snippet 'id="app"' -Description "Vue mount root in served unknown route $($unknownRoute.Path)"
    Assert-Contains -Text $unknownRouteResponse.Content -Snippet '/jazor/main.mjs' -Description "root main module entry in served unknown route $($unknownRoute.Path)"
    Assert-Contains -Text $unknownRouteResponse.Content -Snippet '"System/": "/jazor/System/"' -Description "CLR runtime import-map entry in served unknown route $($unknownRoute.Path)"

    if ($Publish) {
        Write-Host "Wiki publish smoke verification passed."
        Write-Host "Verified: publish output materialization, published /jazor browser asset routes, CLR runtime import-map wiring, published docs routes including /search?q=..., /health, and unknown-route fallback"
    }
    else {
        Write-Host "Wiki smoke verification passed."
        Write-Host "Verified: build output, browser asset routes, CLR runtime import-map wiring, emitted product-shell markers, /health, all registered docs routes including /search?q=..., and unknown-route fallback"
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
