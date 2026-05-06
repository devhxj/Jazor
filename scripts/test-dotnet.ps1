param(
    [ValidateSet("all", "compiler", "clr", "pinia", "vueroute", "razorvue", "jolt", "jolt-build", "emit", "wiki", "wiki-publish", "wiki-browser", "wiki-browser-publish")]
    [string]$Project = "all",
    [string]$Configuration = "Debug",
    [string]$Filter = ""
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$env:DOTNET_CLI_HOME = Join-Path $repoRoot ".dotnet"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$configurationWasExplicit = $PSBoundParameters.ContainsKey("Configuration")

$compilerTestProject = Join-Path $repoRoot "src\Jazor.CompilerTest\Jazor.CompilerTest.csproj"
$clrTestProject = Join-Path $repoRoot "src\Jazor.CLR.Test\Jazor.CLR.Test.csproj"
$piniaTestProject = Join-Path $repoRoot "src\ECMAScript.Pinia.Test\ECMAScript.Pinia.Test.csproj"
$vueRouteTestProject = Join-Path $repoRoot "src\ECMAScript.VueRoute.Test\ECMAScript.VueRoute.Test.csproj"
$razorVueTestProject = Join-Path $repoRoot "src\Jazor.RazorVue.Test\Jazor.RazorVue.Test.csproj"
$joltTestProject = Join-Path $repoRoot "src\Jolt.Test\Jolt.Test.csproj"
$emitTestProject = Join-Path $repoRoot "src\Jazor.EmitTest\Jazor.EmitTest.csproj"
$wikiSmokeScript = Join-Path $repoRoot "src\Wiki\verify-smoke.ps1"
$wikiBrowserScript = Join-Path $repoRoot "src\Wiki\verify-browser.ps1"

function Invoke-WikiVerification {
    param(
        [string]$ScriptPath,
        [string]$FailureName,
        [switch]$Publish
    )

    if (-not [string]::IsNullOrWhiteSpace($Filter)) {
        throw "-Filter is not supported for Wiki smoke targets."
    }

    $effectiveWikiConfiguration = $Configuration
    if ($Publish -and -not $configurationWasExplicit) {
        $effectiveWikiConfiguration = "Release"
    }

    $scriptArgs = @{
        Configuration = $effectiveWikiConfiguration
    }
    if ($Publish) {
        $scriptArgs.Publish = $true
    }
    else {
        $scriptArgs.Build = $true
    }

    & $ScriptPath @scriptArgs
    if ($LASTEXITCODE -ne 0) {
        throw "$FailureName failed for '$Project' with exit code $LASTEXITCODE."
    }
}

switch ($Project) {
    "wiki" {
        Invoke-WikiVerification -ScriptPath $wikiSmokeScript -FailureName "Wiki smoke verification"
        return
    }
    "wiki-publish" {
        Invoke-WikiVerification -ScriptPath $wikiSmokeScript -FailureName "Wiki smoke verification" -Publish
        return
    }
    "wiki-browser" {
        Invoke-WikiVerification -ScriptPath $wikiBrowserScript -FailureName "Wiki browser verification"
        return
    }
    "wiki-browser-publish" {
        Invoke-WikiVerification -ScriptPath $wikiBrowserScript -FailureName "Wiki browser verification" -Publish
        return
    }
}

$joltBuildFilter = @(
    "FullyQualifiedName~JoltBuildTests",
    "FullyQualifiedName~JoltStaticAssetHandlerTests",
    "FullyQualifiedName~JoltBuildCssPipelineTests",
    "FullyQualifiedName~JoltBuildOptimizationTests",
    "FullyQualifiedName~JoltBuildJsSourceMapTests",
    "FullyQualifiedName~JoltBuildSliceFixTests"
) -join "|"
$effectiveFilter = if ($Project -eq "jolt-build" -and [string]::IsNullOrWhiteSpace($Filter)) { $joltBuildFilter } else { $Filter }

$testTargets = @(
    switch ($Project) {
        "compiler" { $compilerTestProject }
        "clr" { $clrTestProject }
        "pinia" { $piniaTestProject }
        "vueroute" { $vueRouteTestProject }
        "razorvue" { $razorVueTestProject }
        "jolt" { $joltTestProject }
        "jolt-build" { $joltTestProject }
        "emit" { $emitTestProject }
        default { $compilerTestProject, $clrTestProject, $piniaTestProject, $vueRouteTestProject, $razorVueTestProject, $joltTestProject, $emitTestProject }
    }
)

$buildTarget = if ($testTargets.Count -gt 1) {
    Join-Path $repoRoot "Jazor.slnx"
} else {
    $testTargets[0]
}

dotnet build $buildTarget -c $Configuration /m:1 /p:BuildInParallel=false -v minimal
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed for '$buildTarget' with exit code $LASTEXITCODE."
}

foreach ($testProject in $testTargets) {
    $testArgs = @("test", $testProject, "-c", $Configuration, "--no-build", "--no-restore", "-v", "minimal")
    if (-not [string]::IsNullOrWhiteSpace($effectiveFilter)) {
        $testArgs += @("--filter", $effectiveFilter)
    }

    dotnet @testArgs
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet test failed for '$testProject' with exit code $LASTEXITCODE."
    }
}
