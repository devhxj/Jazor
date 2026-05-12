param(
    [int]$Port = 4173,
    [string]$Configuration = "Debug",
    [string]$BaseOutputPath = "",
    [string]$BaseIntermediateOutputPath = "",
    [string]$PathBase = "",
    [switch]$Build,
    [switch]$BuildLocal,
    [switch]$Publish,
    [int]$StartupTimeoutSeconds = 30
)

$ErrorActionPreference = "Stop"

$sampleRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent (Split-Path -Parent $sampleRoot)

$args = @(
    "run",
    "--file",
    (Join-Path $repoRoot "scripts\csharp\wiki-verify-smoke.cs"),
    "--",
    "--port",
    $Port,
    "--configuration",
    $Configuration,
    "--startup-timeout-seconds",
    $StartupTimeoutSeconds
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
