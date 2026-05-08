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
    @{ Url = $homeUrl; Path = "/"; Title = "Overview | jazor.wiki"; Description = "A production-oriented docs shell for Jazor, authored entirely with ECMAScript.Vue3 H functions."; Robots = "index, follow" },
    @{ Url = "$rootUrl/search"; Path = "/search"; Title = "Search | jazor.wiki"; Description = "Search the full Wiki corpus by subsystem, route fragment, workflow, or tag."; Robots = "noindex, nofollow" },
    @{ Url = "$rootUrl/guides/getting-started"; Path = "/guides/getting-started"; Title = "Getting Started | jazor.wiki"; Description = "Run the site locally, understand the route model, and validate the emitted Wiki host end to end."; Robots = "index, follow" },
    @{ Url = "$rootUrl/guides/project-lines"; Path = "/guides/project-lines"; Title = "Project Lines | jazor.wiki"; Description = "Understand the two active Jazor lines, when to choose them, and which shared compiler foundations they consume."; Robots = "index, follow" },
    @{ Url = "$rootUrl/guides/content-model"; Path = "/guides/content-model"; Title = "Content Model | jazor.wiki"; Description = "Code-first page metadata, explicit sections, and a navigation contract that stays readable in C#."; Robots = "index, follow" },
    @{ Url = "$rootUrl/guides/navigation-discovery"; Path = "/guides/navigation-discovery"; Title = "Navigation and Discovery | jazor.wiki"; Description = "How readers move through the docs shell with grouped navigation, section TOCs, related pages, and not-found recovery."; Robots = "index, follow" },
    @{ Url = "$rootUrl/guides/information-architecture"; Path = "/guides/information-architecture"; Title = "Information Architecture | jazor.wiki"; Description = "How routes, concern groups, page order, and naming rules keep the docs surface coherent as it grows."; Robots = "index, follow" },
    @{ Url = "$rootUrl/guides/topic-index"; Path = "/guides/topic-index"; Title = "Topic Index | jazor.wiki"; Description = "Use a route-first index to jump into Jazor topics by concern instead of memorizing exact URLs."; Robots = "index, follow" },
    @{ Url = "$rootUrl/guides/glossary"; Path = "/guides/glossary"; Title = "Glossary | jazor.wiki"; Description = "Shared vocabulary for compiler, runtime, host, and documentation terms used across the repository."; Robots = "index, follow" },
    @{ Url = "$rootUrl/guides/faq"; Path = "/guides/faq"; Title = "FAQ | jazor.wiki"; Description = "Short answers to the questions that recur most often when contributors first touch Jazor or Wiki."; Robots = "index, follow" },
    @{ Url = "$rootUrl/guides/troubleshooting"; Path = "/guides/troubleshooting"; Title = "Troubleshooting | jazor.wiki"; Description = "Recover from the most common local Wiki, runtime-module, and compiler-boundary failures."; Robots = "index, follow" },
    @{ Url = "$rootUrl/engineering/h-function-authoring"; Path = "/engineering/h-function-authoring"; Title = "H-Function Authoring | jazor.wiki"; Description = "Why H functions are the production authoring surface for this Wiki, and the conventions that keep it maintainable."; Robots = "index, follow" },
    @{ Url = "$rootUrl/engineering/compiler-overview"; Path = "/engineering/compiler-overview"; Title = "Compiler Overview | jazor.wiki"; Description = "A high-level view of the compiler pipeline, active contracts, and where to read deeper."; Robots = "index, follow" },
    @{ Url = "$rootUrl/engineering/compiler-support-boundary"; Path = "/engineering/compiler-support-boundary"; Title = "Compiler Support Boundary | jazor.wiki"; Description = "The active compiler contract for controlled input, usage-site validation, semantic erasure, and explicit failure boundaries."; Robots = "index, follow" },
    @{ Url = "$rootUrl/engineering/route-catalog-contract"; Path = "/engineering/route-catalog-contract"; Title = "Route Catalog Contract | jazor.wiki"; Description = 'Why `WikiHomeModule.RouteContract.cs` is the single registration surface for route metadata, body dispatch, TOC anchors, and adjacent-page flow.'; Robots = "index, follow" },
    @{ Url = "$rootUrl/engineering/host-semantic-seams"; Path = "/engineering/host-semantic-seams"; Title = "Host Semantic Seams | jazor.wiki"; Description = "How WhiteList, Alias, Inline, Import, and Compile divide responsibility across the supported host semantic surface."; Robots = "index, follow" },
    @{ Url = "$rootUrl/engineering/import-emit-contract"; Path = "/engineering/import-emit-contract"; Title = "Import and Emit Contract | jazor.wiki"; Description = "The stable boundary between import discovery, module AST assembly, generated catalogs, and host-facing file materialization."; Robots = "index, follow" },
    @{ Url = "$rootUrl/engineering/runtime-catalog"; Path = "/engineering/runtime-catalog"; Title = "CLR Runtime Catalog | jazor.wiki"; Description = 'How CLR import helpers become browser-ready `System/*` runtime modules, and what guarantees keep that catalog safe to ship.'; Robots = "index, follow" },
    @{ Url = "$rootUrl/engineering/jolt-host"; Path = "/engineering/jolt-host"; Title = "Jolt Host | jazor.wiki"; Description = 'The full-featured `.jazor` development host for editing, preview, build, and debug workflows.'; Robots = "index, follow" },
    @{ Url = "$rootUrl/engineering/razorvue-library-mode"; Path = "/engineering/razorvue-library-mode"; Title = "RazorVue Library Mode | jazor.wiki"; Description = "The build-time library mode for compiling Razor components into JavaScript artifacts without a full development host."; Robots = "index, follow" },
    @{ Url = "$rootUrl/engineering/vueroute-bindings"; Path = "/engineering/vueroute-bindings"; Title = "VueRoute Bindings | jazor.wiki"; Description = "The standalone Vue Router binding library, its host-surface scope, and the split verification path that keeps tests out of the compiler suite."; Robots = "index, follow" },
    @{ Url = "$rootUrl/operations/content-governance"; Path = "/operations/content-governance"; Title = "Content Governance | jazor.wiki"; Description = "How code-first docs content is owned, edited, reviewed, and released without drifting away from the emitted product shell."; Robots = "index, follow" },
    @{ Url = "$rootUrl/operations/deployment"; Path = "/operations/deployment"; Title = "Deployment | jazor.wiki"; Description = "Build outputs, fallback routing, smoke verification, and the static delivery contract for Wiki."; Robots = "index, follow" },
    @{ Url = "$rootUrl/operations/testing-verification"; Path = "/operations/testing-verification"; Title = "Testing and Verification | jazor.wiki"; Description = "How compiler, emit, and operational smoke checks fit together to protect the production docs surface."; Robots = "index, follow" }
)
$searchQueryRoute = @{
    Url = "$rootUrl/search?q=compiler"
    Path = "/search?q=compiler"
    Title = "Search: compiler | jazor.wiki"
    Description = 'Search results for "compiler" across route metadata, tags, curated page body text, and section titles.'
}
$unknownRoute = @{
    Url = "$rootUrl/guides/missing-page"
    Path = "/guides/missing-page"
    Title = "Page Not Found | jazor.wiki"
    Description = "The current path is not registered in the Wiki page catalog."
}
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
    @{ Url = "$rootUrl/jazor/main.mjs.map"; Path = "/jazor/main.mjs.map"; Snippet = '"file":"main.mjs"'; ContentType = "application/json"; ExtraSnippets = @('AppModule.cs', '"sourcesContent"') },
    @{ Url = "$rootUrl/jazor/components/wiki-home.mjs"; Path = "/jazor/components/wiki-home.mjs"; Snippet = 'Search docs pages' },
    @{ Url = "$rootUrl/jazor/components/wiki-home.mjs.map"; Path = "/jazor/components/wiki-home.mjs.map"; Snippet = '"file":"components/wiki-home.mjs"'; ContentType = "application/json"; ExtraSnippets = @('WikiHomeModule.cs', 'WikiHomeModule.DocumentContract.cs', '"sourcesContent"') },
    @{ Url = "$rootUrl/jazor/System/StringModule.js"; Path = "/jazor/System/StringModule.js"; Snippet = 'export' },
    @{ Url = "$rootUrl/site.css"; Path = "/site.css"; Snippet = '.wiki-shell' },
    @{ Url = "$rootUrl/favicon.svg"; Path = "/favicon.svg"; Snippet = '<svg' },
    @{ Url = "$rootUrl/vendor/vue@3.5.16.mjs"; Path = "/vendor/vue@3.5.16.mjs"; Snippet = 'createApp(' }
)
$discoveryDocumentChecks = @(
    @{ Url = "$rootUrl/robots.txt"; Path = "/robots.txt"; Snippet = "Sitemap: $rootUrl/sitemap.xml"; MissingSnippet = ""; ContentType = "text/plain; charset=utf-8"; CacheControl = "public, max-age=300, must-revalidate" },
    @{ Url = "$rootUrl/sitemap.xml"; Path = "/sitemap.xml"; Snippet = "<loc>$rootUrl/</loc>"; MissingSnippet = "<loc>$rootUrl/search</loc>"; ContentType = "application/xml; charset=utf-8"; CacheControl = "public, max-age=300, must-revalidate" }
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

function Assert-HeaderEquals {
    param(
        [object]$Response,
        [string]$HeaderName,
        [string]$ExpectedValue,
        [string]$Description
    )

    $actualValue = $Response.Headers[$HeaderName]
    if ([string]::IsNullOrEmpty($actualValue)) {
        throw "Missing ${Description}: response header '$HeaderName' was not present."
    }

    $actualSegments = $actualValue.Split(',') | ForEach-Object { $_.Trim() } | Where-Object { $_.Length -gt 0 }
    $expectedSegments = $ExpectedValue.Split(',') | ForEach-Object { $_.Trim() } | Where-Object { $_.Length -gt 0 }
    $actualComparable = ($actualSegments | Sort-Object) -join ','
    $expectedComparable = ($expectedSegments | Sort-Object) -join ','

    if ($actualComparable -ne $expectedComparable) {
        throw "Unexpected ${Description}: expected '$ExpectedValue', actual '$actualValue'."
    }
}

function Assert-HeaderMatches {
    param(
        [object]$Response,
        [string]$HeaderName,
        [string]$ExpectedPattern,
        [string]$Description
    )

    $actualValue = $Response.Headers[$HeaderName]
    if ([string]::IsNullOrEmpty($actualValue)) {
        throw "Missing ${Description}: response header '$HeaderName' was not present."
    }

    if ($actualValue -notmatch $ExpectedPattern) {
        throw "Unexpected ${Description}: expected pattern '$ExpectedPattern', actual '$actualValue'."
    }
}

function New-ExpectedRouteMetadata {
    param(
        [string]$Title,
        [string]$Description,
        [string]$Robots = "index, follow",
        [bool]$Registered = $true
    )

    return @{
        Title = $Title
        Description = $Description
        Robots = $Robots
        Registered = $Registered
    }
}

function Assert-RouteMetadata {
    param(
        [string]$Html,
        [hashtable]$Expected,
        [string]$ExpectedAbsoluteUrl,
        [string]$Description
    )

    $htmlEncoder = [System.Text.Encodings.Web.HtmlEncoder]::Default
    $encodedTitle = $htmlEncoder.Encode($Expected.Title)
    $encodedDescription = $htmlEncoder.Encode($Expected.Description)
    $encodedRobots = $htmlEncoder.Encode($Expected.Robots)
    $encodedAbsoluteUrl = $htmlEncoder.Encode($ExpectedAbsoluteUrl)

    Assert-Contains -Text $Html -Snippet "<title>$encodedTitle</title>" -Description "$Description title"
    Assert-Contains -Text $Html -Snippet "meta name=`"description`" content=`"$encodedDescription`"" -Description "$Description description"
    Assert-Contains -Text $Html -Snippet "meta name=`"robots`" content=`"$encodedRobots`"" -Description "$Description robots"
    Assert-Contains -Text $Html -Snippet "link rel=`"canonical`" href=`"$encodedAbsoluteUrl`"" -Description "$Description canonical"
    Assert-Contains -Text $Html -Snippet "meta property=`"og:title`" content=`"$encodedTitle`"" -Description "$Description og:title"
    Assert-Contains -Text $Html -Snippet "meta property=`"og:description`" content=`"$encodedDescription`"" -Description "$Description og:description"
    Assert-Contains -Text $Html -Snippet "meta property=`"og:url`" content=`"$encodedAbsoluteUrl`"" -Description "$Description og:url"
    Assert-Contains -Text $Html -Snippet "meta name=`"twitter:title`" content=`"$encodedTitle`"" -Description "$Description twitter:title"
    Assert-Contains -Text $Html -Snippet "meta name=`"twitter:description`" content=`"$encodedDescription`"" -Description "$Description twitter:description"
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
    Where-Object { $_ -notin @("WikiHomeModule.cs", "WikiHomeModule.RouteContract.cs", "WikiHomeModule.Elements.cs", "WikiHomeModule.DocumentContract.cs") }
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
        if ($asset.ContainsKey("ExtraSnippets")) {
            foreach ($extraSnippet in $asset.ExtraSnippets) {
                Assert-Contains -Text $assetResponse.Content -Snippet $extraSnippet -Description "served browser asset $($asset.Path)"
            }
        }
        if ($asset.ContainsKey("ContentType")) {
            Assert-HeaderEquals -Response $assetResponse -HeaderName "Content-Type" -ExpectedValue $asset.ContentType -Description "Content-Type for served browser asset $($asset.Path)"
        }
        Assert-HeaderEquals -Response $assetResponse -HeaderName "Referrer-Policy" -ExpectedValue "strict-origin-when-cross-origin" -Description "Referrer-Policy for served browser asset $($asset.Path)"
        Assert-HeaderEquals -Response $assetResponse -HeaderName "X-Content-Type-Options" -ExpectedValue "nosniff" -Description "X-Content-Type-Options for served browser asset $($asset.Path)"
        Assert-HeaderEquals -Response $assetResponse -HeaderName "X-Frame-Options" -ExpectedValue "DENY" -Description "X-Frame-Options for served browser asset $($asset.Path)"

        if ($asset.Path -like "/vendor/*") {
            Assert-HeaderEquals -Response $assetResponse -HeaderName "Cache-Control" -ExpectedValue "public, max-age=31536000, immutable" -Description "Cache-Control for served browser asset $($asset.Path)"
        }
        else {
            Assert-HeaderEquals -Response $assetResponse -HeaderName "Cache-Control" -ExpectedValue "no-cache, must-revalidate" -Description "Cache-Control for served browser asset $($asset.Path)"
        }
    }

    foreach ($document in $discoveryDocumentChecks) {
        $documentResponse = Invoke-WebRequest -Uri $document.Url -TimeoutSec 5
        if ($documentResponse.StatusCode -ne 200) {
            throw "Unexpected $($document.Path) status code: $($documentResponse.StatusCode)"
        }

        Assert-Contains -Text $documentResponse.Content -Snippet $document.Snippet -Description "served discovery document $($document.Path)"
        if (-not [string]::IsNullOrEmpty($document.MissingSnippet)) {
            Assert-NotContains -Text $documentResponse.Content -Snippet $document.MissingSnippet -Description "forbidden discovery document marker in $($document.Path)"
        }
        Assert-HeaderEquals -Response $documentResponse -HeaderName "Content-Type" -ExpectedValue $document.ContentType -Description "Content-Type for served discovery document $($document.Path)"
        Assert-HeaderEquals -Response $documentResponse -HeaderName "Cache-Control" -ExpectedValue $document.CacheControl -Description "Cache-Control for served discovery document $($document.Path)"
        Assert-HeaderEquals -Response $documentResponse -HeaderName "Referrer-Policy" -ExpectedValue "strict-origin-when-cross-origin" -Description "Referrer-Policy for served discovery document $($document.Path)"
        Assert-HeaderEquals -Response $documentResponse -HeaderName "X-Content-Type-Options" -ExpectedValue "nosniff" -Description "X-Content-Type-Options for served discovery document $($document.Path)"
        Assert-HeaderEquals -Response $documentResponse -HeaderName "X-Frame-Options" -ExpectedValue "DENY" -Description "X-Frame-Options for served discovery document $($document.Path)"
    }

    foreach ($route in $docsRoutes) {
        $expectedMetadata = New-ExpectedRouteMetadata -Title $route.Title -Description $route.Description -Robots $route.Robots
        $response = Invoke-WebRequest -Uri $route.Url -TimeoutSec 5
        if ($response.StatusCode -ne 200) {
            throw "Unexpected $($route.Path) status code: $($response.StatusCode)"
        }

        Assert-Contains -Text $response.Content -Snippet 'id="app"' -Description "Vue mount root in served route $($route.Path)"
        Assert-Contains -Text $response.Content -Snippet '/jazor/main.mjs' -Description "root main module entry in served route $($route.Path)"
        Assert-Contains -Text $response.Content -Snippet '"System/": "/jazor/System/"' -Description "CLR runtime import-map entry in served route $($route.Path)"
        Assert-RouteMetadata -Html $response.Content -Expected $expectedMetadata -ExpectedAbsoluteUrl ($rootUrl + $route.Path) -Description "served route $($route.Path)"
        Assert-HeaderEquals -Response $response -HeaderName "Referrer-Policy" -ExpectedValue "strict-origin-when-cross-origin" -Description "Referrer-Policy for served route $($route.Path)"
        Assert-HeaderEquals -Response $response -HeaderName "X-Content-Type-Options" -ExpectedValue "nosniff" -Description "X-Content-Type-Options for served route $($route.Path)"
        Assert-HeaderEquals -Response $response -HeaderName "X-Frame-Options" -ExpectedValue "DENY" -Description "X-Frame-Options for served route $($route.Path)"
        if ($route.Robots -eq "noindex, nofollow") {
            Assert-HeaderEquals -Response $response -HeaderName "X-Robots-Tag" -ExpectedValue "noindex, nofollow" -Description "X-Robots-Tag for served route $($route.Path)"
        }
        Assert-HeaderEquals -Response $response -HeaderName "Cache-Control" -ExpectedValue "no-cache, must-revalidate" -Description "Cache-Control for served route $($route.Path)"
        Assert-HeaderEquals -Response $response -HeaderName "Cross-Origin-Opener-Policy" -ExpectedValue "same-origin" -Description "Cross-Origin-Opener-Policy for served route $($route.Path)"
        Assert-HeaderEquals -Response $response -HeaderName "Cross-Origin-Resource-Policy" -ExpectedValue "same-origin" -Description "Cross-Origin-Resource-Policy for served route $($route.Path)"
        Assert-HeaderEquals -Response $response -HeaderName "X-Permitted-Cross-Domain-Policies" -ExpectedValue "none" -Description "X-Permitted-Cross-Domain-Policies for served route $($route.Path)"
        Assert-HeaderEquals -Response $response -HeaderName "Permissions-Policy" -ExpectedValue "accelerometer=(), autoplay=(), camera=(), display-capture=(), geolocation=(), gyroscope=(), hid=(), microphone=(), payment=(), usb=(), clipboard-read=(self), clipboard-write=(self)" -Description "Permissions-Policy for served route $($route.Path)"
        Assert-HeaderMatches -Response $response -HeaderName "Content-Security-Policy" -ExpectedPattern "script-src 'self' 'nonce-[^']+'" -Description "Content-Security-Policy nonce for served route $($route.Path)"
        Assert-Contains -Text $response.Content -Snippet 'script type="importmap" nonce="' -Description "importmap nonce marker in served route $($route.Path)"
    }

    $searchQueryResponse = Invoke-WebRequest -Uri $searchQueryRoute.Url -TimeoutSec 5
    if ($searchQueryResponse.StatusCode -ne 200) {
        throw "Unexpected $($searchQueryRoute.Path) status code: $($searchQueryResponse.StatusCode)"
    }

    Assert-Contains -Text $searchQueryResponse.Content -Snippet 'id="app"' -Description "Vue mount root in served route $($searchQueryRoute.Path)"
    Assert-Contains -Text $searchQueryResponse.Content -Snippet '/jazor/main.mjs' -Description "root main module entry in served route $($searchQueryRoute.Path)"
    Assert-Contains -Text $searchQueryResponse.Content -Snippet '"System/": "/jazor/System/"' -Description "CLR runtime import-map entry in served route $($searchQueryRoute.Path)"
    $expectedSearchMetadata = New-ExpectedRouteMetadata -Title $searchQueryRoute.Title -Description $searchQueryRoute.Description -Robots "noindex, nofollow"
    Assert-RouteMetadata -Html $searchQueryResponse.Content -Expected $expectedSearchMetadata -ExpectedAbsoluteUrl $searchQueryRoute.Url -Description "served route $($searchQueryRoute.Path)"
    Assert-HeaderEquals -Response $searchQueryResponse -HeaderName "Referrer-Policy" -ExpectedValue "strict-origin-when-cross-origin" -Description "Referrer-Policy for served route $($searchQueryRoute.Path)"
    Assert-HeaderEquals -Response $searchQueryResponse -HeaderName "X-Content-Type-Options" -ExpectedValue "nosniff" -Description "X-Content-Type-Options for served route $($searchQueryRoute.Path)"
    Assert-HeaderEquals -Response $searchQueryResponse -HeaderName "X-Frame-Options" -ExpectedValue "DENY" -Description "X-Frame-Options for served route $($searchQueryRoute.Path)"
    Assert-HeaderEquals -Response $searchQueryResponse -HeaderName "X-Robots-Tag" -ExpectedValue "noindex, nofollow" -Description "X-Robots-Tag for served route $($searchQueryRoute.Path)"
    Assert-HeaderEquals -Response $searchQueryResponse -HeaderName "Cache-Control" -ExpectedValue "no-cache, must-revalidate" -Description "Cache-Control for served route $($searchQueryRoute.Path)"
    Assert-HeaderMatches -Response $searchQueryResponse -HeaderName "Content-Security-Policy" -ExpectedPattern "script-src 'self' 'nonce-[^']+'" -Description "Content-Security-Policy nonce for served route $($searchQueryRoute.Path)"
    Assert-Contains -Text $searchQueryResponse.Content -Snippet 'script type="importmap" nonce="' -Description "importmap nonce marker in served route $($searchQueryRoute.Path)"

    $unknownRouteResponse = Invoke-WebRequest -Uri $unknownRoute.Url -TimeoutSec 5 -SkipHttpErrorCheck
    if ($unknownRouteResponse.StatusCode -ne 404) {
        throw "Unexpected $($unknownRoute.Path) status code: $($unknownRouteResponse.StatusCode)"
    }

    Assert-Contains -Text $unknownRouteResponse.Content -Snippet 'id="app"' -Description "Vue mount root in served unknown route $($unknownRoute.Path)"
    Assert-Contains -Text $unknownRouteResponse.Content -Snippet '/jazor/main.mjs' -Description "root main module entry in served unknown route $($unknownRoute.Path)"
    Assert-Contains -Text $unknownRouteResponse.Content -Snippet '"System/": "/jazor/System/"' -Description "CLR runtime import-map entry in served unknown route $($unknownRoute.Path)"
    $expectedNotFoundMetadata = New-ExpectedRouteMetadata -Title $unknownRoute.Title -Description $unknownRoute.Description -Robots "noindex, nofollow" -Registered:$false
    Assert-RouteMetadata -Html $unknownRouteResponse.Content -Expected $expectedNotFoundMetadata -ExpectedAbsoluteUrl $unknownRoute.Url -Description "served unknown route $($unknownRoute.Path)"
    Assert-HeaderEquals -Response $unknownRouteResponse -HeaderName "Referrer-Policy" -ExpectedValue "strict-origin-when-cross-origin" -Description "Referrer-Policy for served unknown route $($unknownRoute.Path)"
    Assert-HeaderEquals -Response $unknownRouteResponse -HeaderName "X-Content-Type-Options" -ExpectedValue "nosniff" -Description "X-Content-Type-Options for served unknown route $($unknownRoute.Path)"
    Assert-HeaderEquals -Response $unknownRouteResponse -HeaderName "X-Frame-Options" -ExpectedValue "DENY" -Description "X-Frame-Options for served unknown route $($unknownRoute.Path)"
    Assert-HeaderEquals -Response $unknownRouteResponse -HeaderName "X-Robots-Tag" -ExpectedValue "noindex, nofollow" -Description "X-Robots-Tag for served unknown route $($unknownRoute.Path)"
    Assert-HeaderEquals -Response $unknownRouteResponse -HeaderName "Cache-Control" -ExpectedValue "no-cache, must-revalidate" -Description "Cache-Control for served unknown route $($unknownRoute.Path)"
    Assert-HeaderMatches -Response $unknownRouteResponse -HeaderName "Content-Security-Policy" -ExpectedPattern "script-src 'self' 'nonce-[^']+'" -Description "Content-Security-Policy nonce for served unknown route $($unknownRoute.Path)"
    Assert-Contains -Text $unknownRouteResponse.Content -Snippet 'script type="importmap" nonce="' -Description "importmap nonce marker in served unknown route $($unknownRoute.Path)"

    if ($Publish) {
        Write-Host "Wiki publish smoke verification passed."
        Write-Host "Verified: publish output materialization, published /jazor browser asset routes, route-specific HTML metadata, security headers, CLR runtime import-map wiring, published docs routes including /search?q=..., /health, and 404 shell fallback"
    }
    else {
        Write-Host "Wiki smoke verification passed."
        Write-Host "Verified: build output, browser asset routes, route-specific HTML metadata, security headers, CLR runtime import-map wiring, emitted product-shell markers, /health, all registered docs routes including /search?q=..., and 404 shell fallback"
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
