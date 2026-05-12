param(
    [int]$Port = 4196,
    [int]$CdpPort = 9236,
    [string]$Configuration = "Debug",
    [string]$BaseOutputPath = "",
    [string]$BaseIntermediateOutputPath = "",
    [string]$PathBase = "",
    [switch]$Build,
    [switch]$BuildLocal,
    [switch]$Publish,
    [int]$StartupTimeoutSeconds = 30,
    [int]$BrowserStartupTimeoutSeconds = 15
)

$ErrorActionPreference = "Stop"

$sampleRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent (Split-Path -Parent $sampleRoot)

$args = @(
    "run",
    "--file",
    (Join-Path $repoRoot "scripts\csharp\wiki-verify-browser.cs"),
    "--",
    "--port",
    $Port,
    "--cdp-port",
    $CdpPort,
    "--configuration",
    $Configuration,
    "--startup-timeout-seconds",
    $StartupTimeoutSeconds,
    "--browser-startup-timeout-seconds",
    $BrowserStartupTimeoutSeconds
)

if ($PSBoundParameters.ContainsKey("BaseOutputPath")) {
    $args += "--base-output-path"
    $args += $BaseOutputPath
}

if ($PSBoundParameters.ContainsKey("BaseIntermediateOutputPath")) {
    $args += "--base-intermediate-output-path"
    $args += $BaseIntermediateOutputPath
}

if ($PSBoundParameters.ContainsKey("PathBase")) {
    $args += "--path-base"
    $args += $PathBase
}

if ($Build) {
    $args += "--build"
}

if ($BuildLocal) {
    $args += "--build-local"
}

if ($Publish) {
    $args += "--publish"
}

dotnet @args
exit $LASTEXITCODE
