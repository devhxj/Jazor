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
$consumerRoot = Join-Path $sampleRoot "pinia-consumer"
$hostRoot = Join-Path $sampleRoot "Pinia.Counter.Host"
$hostAssemblyPath = Join-Path $hostRoot "bin\$Configuration\net10.0\Pinia.Counter.Host.dll"
$defaultGeneratedOutputRoot = Join-Path $repoRoot ".tmp\sample-smoke\ECMAScript.Pinia.Counter\$Configuration\jazor"
if ([string]::IsNullOrWhiteSpace($GeneratedOutputRoot)) {
    $GeneratedOutputRoot = $defaultGeneratedOutputRoot
}

$jazorRoot = [System.IO.Path]::GetFullPath($GeneratedOutputRoot)
$counterStoreModulePath = Join-Path $jazorRoot "stores\counter-store.mjs"
$testingModulePath = Join-Path $jazorRoot "tests\counter-testing.mjs"
$hostAppModulePath = Join-Path $jazorRoot "host\app.mjs"
$manifestPath = Join-Path $jazorRoot "jazor-manifest.json"
$consumerPackageJsonPath = Join-Path $consumerRoot "package.json"
$consumerPackageLockPath = Join-Path $consumerRoot "package-lock.json"
$consumerNodeModulesPath = Join-Path $consumerRoot "node_modules"

$env:DOTNET_CLI_HOME = Join-Path $repoRoot ".dotnet"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"

function Invoke-DotNet {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($Arguments -join ' ') failed with exit code $LASTEXITCODE."
    }
}

function Invoke-Script {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [string[]]$Arguments = @()
    )

    & $Path @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "script failed with exit code ${LASTEXITCODE}: $Path $($Arguments -join ' ')"
    }
}

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
    Assert-PathExists -Path $counterStoreModulePath -Description "generated counter store module"
    Assert-PathExists -Path $testingModulePath -Description "generated testing module"
    Assert-PathExists -Path $hostAppModulePath -Description "generated host app module"
    Assert-PathExists -Path $manifestPath -Description "generated manifest"

    $counterStoreModule = Get-Content $counterStoreModulePath -Raw
    $testingModule = Get-Content $testingModulePath -Raw
    $hostAppModule = Get-Content $hostAppModulePath -Raw
    $manifest = Get-Content $manifestPath -Raw

    Assert-Contains -Text $counterStoreModule -Snippet 'from "pinia"' -Description "pinia runtime import in counter store module"
    Assert-Contains -Text $counterStoreModule -Snippet "defineStore(" -Description "defineStore lowering in counter store module"
    Assert-Contains -Text $counterStoreModule -Snippet "storeToRefs(" -Description "storeToRefs lowering in counter store module"

    Assert-Contains -Text $testingModule -Snippet 'from "@pinia/testing"' -Description "@pinia/testing runtime import in testing module"
    Assert-Contains -Text $testingModule -Snippet "createTestingPinia({" -Description "createTestingPinia lowering in testing module"
    Assert-Contains -Text $testingModule -Snippet "stubActions" -Description "testing stubActions contract in testing module"

    Assert-Contains -Text $hostAppModule -Snippet "disposePinia(" -Description "disposePinia teardown in host app module"
    Assert-Contains -Text $hostAppModule -Snippet "createPinia()" -Description "createPinia root creation in host app module"

    Assert-Contains -Text $manifest -Snippet '"host/app.mjs"' -Description "host app entry in manifest"
    Assert-Contains -Text $manifest -Snippet '"stores/counter-store.mjs"' -Description "counter store entry in manifest"
    Assert-Contains -Text $manifest -Snippet '"tests/counter-testing.mjs"' -Description "testing entry in manifest"
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

if ($env:CI -eq "true" -or -not (Test-Path -LiteralPath $consumerNodeModulesPath)) {
    Invoke-Npm -Arguments @("ci")
}

$previousGeneratedRoot = $env:JAZOR_GENERATED_ROOT
$restoreGeneratedRoot = -not [string]::IsNullOrEmpty($previousGeneratedRoot)
$env:JAZOR_GENERATED_ROOT = $jazorRoot

Push-Location $consumerRoot
try {
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

Write-Host "ECMAScript.Pinia sample smoke verification passed."
Write-Host "Verified: local Jazor package pack, isolated generated Pinia/testing modules, Vite build, and Vitest runtime/DOM coverage."
