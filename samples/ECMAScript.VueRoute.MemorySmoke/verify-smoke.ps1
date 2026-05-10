param(
    [string]$Configuration = "Debug",
    [switch]$BuildLocal,
    [switch]$FrontendOnly,
    [string]$GeneratedOutputRoot = ""
)

$ErrorActionPreference = "Stop"

$sampleRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent (Split-Path -Parent $sampleRoot)
$buildLocalScript = Join-Path $sampleRoot "build-local.ps1"
$consumerRoot = Join-Path $sampleRoot "vueroute-consumer"
$hostRoot = Join-Path $sampleRoot "VueRoute.MemorySmoke.Host"
$hostAssemblyPath = Join-Path $hostRoot "bin\$Configuration\net11.0\VueRoute.MemorySmoke.Host.dll"
$defaultGeneratedOutputRoot = Join-Path $repoRoot ".tmp\sample-smoke\ECMAScript.VueRoute.MemorySmoke\$Configuration\jazor"
if ([string]::IsNullOrWhiteSpace($GeneratedOutputRoot)) {
    $GeneratedOutputRoot = $defaultGeneratedOutputRoot
}

$jazorRoot = [System.IO.Path]::GetFullPath($GeneratedOutputRoot)
$routerModulePath = Join-Path $jazorRoot "router\memory-router.mjs"
$componentModulePath = Join-Path $jazorRoot "components\route-shell.mjs"
$testingModulePath = Join-Path $jazorRoot "tests\router-testing.mjs"
$hostAppModulePath = Join-Path $jazorRoot "host\app.mjs"
$manifestPath = Join-Path $jazorRoot "jazor-manifest.json"
$consumerPackageJsonPath = Join-Path $consumerRoot "package.json"
$consumerPackageLockPath = Join-Path $consumerRoot "package-lock.json"
$consumerNodeModulesPath = Join-Path $consumerRoot "node_modules"

$env:DOTNET_CLI_HOME = Join-Path $repoRoot ".dotnet"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"

function Invoke-Npm {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    npm @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "npm $($Arguments -join ' ') failed with exit code $LASTEXITCODE."
    }
}

function Assert-PathExists {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Description
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Missing ${Description}: $Path"
    }
}

function Assert-Contains {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Text,
        [Parameter(Mandatory = $true)]
        [string]$Snippet,
        [Parameter(Mandatory = $true)]
        [string]$Description
    )

    if (-not $Text.Contains($Snippet, [StringComparison]::Ordinal)) {
        throw "Missing ${Description}: expected to find '$Snippet'."
    }
}

function Assert-GeneratedHostArtifacts {
    Assert-PathExists -Path $routerModulePath -Description "generated router module"
    Assert-PathExists -Path $componentModulePath -Description "generated route-shell component module"
    Assert-PathExists -Path $testingModulePath -Description "generated router testing module"
    Assert-PathExists -Path $hostAppModulePath -Description "generated host app module"
    Assert-PathExists -Path $manifestPath -Description "generated manifest"

    $routerModule = Get-Content $routerModulePath -Raw
    $componentModule = Get-Content $componentModulePath -Raw
    $testingModule = Get-Content $testingModulePath -Raw
    $hostAppModule = Get-Content $hostAppModulePath -Raw
    $manifest = Get-Content $manifestPath -Raw

    Assert-Contains -Text $routerModule -Snippet 'from "npm:vue-router@4"' -Description "vue-router runtime import in router module"
    Assert-Contains -Text $routerModule -Snippet 'createMemoryHistory(' -Description "createMemoryHistory lowering in router module"
    Assert-Contains -Text $routerModule -Snippet 'createRouter({' -Description "createRouter lowering in router module"
    Assert-Contains -Text $routerModule -Snippet 'beforeEach(' -Description "beforeEach lowering in router module"
    Assert-Contains -Text $routerModule -Snippet 'afterEach(' -Description "afterEach lowering in router module"

    Assert-Contains -Text $componentModule -Snippet 'useRouter()' -Description "useRouter lowering in component module"
    Assert-Contains -Text $componentModule -Snippet 'useRoute()' -Description "useRoute lowering in component module"
    Assert-Contains -Text $componentModule -Snippet 'useLink({' -Description "useLink lowering in component module"
    Assert-Contains -Text $componentModule -Snippet 'onBeforeRouteLeave(' -Description "component leave guard usage in component module"
    Assert-Contains -Text $componentModule -Snippet 'inject(routerViewLocationKey)' -Description "typed router-view injection usage in component module"

    Assert-Contains -Text $testingModule -Snippet 'loadRouteLocation(' -Description "loadRouteLocation lowering in testing module"
    Assert-Contains -Text $testingModule -Snippet 'router.push(' -Description "router push lowering in testing module"
    Assert-Contains -Text $testingModule -Snippet 'navigateScenario(' -Description "navigateScenario usage in testing module"

    Assert-Contains -Text $hostAppModule -Snippet 'app.use(router);' -Description "router installation in host app module"
    Assert-Contains -Text $hostAppModule -Snippet 'router.isReady()' -Description "router readiness flow in host app module"
    Assert-Contains -Text $hostAppModule -Snippet 'RouterLink' -Description "RouterLink usage in host app module"
    Assert-Contains -Text $hostAppModule -Snippet 'RouterView' -Description "RouterView usage in host app module"

    Assert-Contains -Text $manifest -Snippet '"host/app.mjs"' -Description "host app entry in manifest"
    Assert-Contains -Text $manifest -Snippet '"router/memory-router.mjs"' -Description "router module entry in manifest"
    Assert-Contains -Text $manifest -Snippet '"tests/router-testing.mjs"' -Description "testing module entry in manifest"
}

if (-not $FrontendOnly -or $BuildLocal) {
    & $buildLocalScript -Configuration $Configuration -JazorOutDir $jazorRoot
    if ($LASTEXITCODE -ne 0) {
        throw "build-local.ps1 failed with exit code ${LASTEXITCODE}."
    }
}

Assert-PathExists -Path $consumerPackageJsonPath -Description "consumer package.json"
Assert-PathExists -Path $consumerPackageLockPath -Description "consumer package-lock.json"
Assert-PathExists -Path $hostAssemblyPath -Description "sample host assembly for requested configuration"
Assert-GeneratedHostArtifacts

$previousGeneratedRoot = $env:JAZOR_GENERATED_ROOT
$restoreGeneratedRoot = -not [string]::IsNullOrEmpty($previousGeneratedRoot)
$env:JAZOR_GENERATED_ROOT = $jazorRoot

Push-Location $consumerRoot
try {
    Invoke-Npm -Arguments @("ci")

    Invoke-Npm -Arguments @("run", "build")
    Invoke-Npm -Arguments @("run", "test", "--", "--run")
}
finally {
    Pop-Location

    if ($restoreGeneratedRoot) {
        $env:JAZOR_GENERATED_ROOT = $previousGeneratedRoot
    }
    else {
        Remove-Item Env:JAZOR_GENERATED_ROOT -ErrorAction SilentlyContinue
    }
}

Write-Host "ECMAScript.VueRoute sample smoke verification passed."
Write-Host "Verified: local Jazor package pack, isolated generated Vue Router modules, Vite build, and Vitest runtime/DOM coverage."
