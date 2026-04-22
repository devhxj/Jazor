param(
    [ValidateSet("all", "hmr", "matrix", "matrix-exception")]
    [string]$Mode = "all",
    [string]$Configuration = "Debug",
    [string]$BrowserPath = ""
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$env:DOTNET_CLI_HOME = Join-Path $repoRoot ".dotnet"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"

$runHmrStress = $Mode -eq "all" -or $Mode -eq "hmr"
$runSourceMapMatrix = $Mode -eq "all" -or $Mode -eq "matrix" -or $Mode -eq "matrix-exception"
$env:JOLT_RUN_REAL_CDP_HMR_STRESS = if ($runHmrStress) { "true" } else { "false" }
$env:JOLT_RUN_REAL_CDP_SOURCE_MAP_MATRIX = if ($runSourceMapMatrix) { "true" } else { "false" }

$testFilter = switch ($Mode) {
    "hmr" { "FullyQualifiedName~Jolt_DapProcess_RealBrowserCdpAndHmrStress" }
    "matrix" { "FullyQualifiedName~Jolt_DapProcess_RealBrowserCdpSourceMap" }
    "matrix-exception" { "FullyQualifiedName~Jolt_DapProcess_RealBrowserCdpSourceMapExceptionMatrix" }
    default { "FullyQualifiedName~Jolt_DapProcess_RealBrowserCdp" }
}

if (-not [string]::IsNullOrWhiteSpace($BrowserPath)) {
    if (-not (Test-Path -LiteralPath $BrowserPath)) {
        throw "Browser executable path '$BrowserPath' does not exist."
    }

    $resolvedBrowserPath = (Resolve-Path -LiteralPath $BrowserPath).Path
    $env:JOLT_REAL_BROWSER_PATH = $resolvedBrowserPath
}

$joltTestProject = Join-Path $repoRoot "src\Jolt.Test\Jolt.Test.csproj"
dotnet build $joltTestProject -c $Configuration /m:1 /p:BuildInParallel=false -v minimal
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed for '$joltTestProject' with exit code $LASTEXITCODE."
}

dotnet test $joltTestProject -c $Configuration --no-build --no-restore --filter $testFilter -v minimal
if ($LASTEXITCODE -ne 0) {
    throw "dotnet test failed for '$joltTestProject' with exit code $LASTEXITCODE."
}
