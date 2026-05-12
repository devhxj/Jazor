param(
    [string]$Configuration = "Debug",
    [string]$BaseOutputPath = "",
    [string]$BaseIntermediateOutputPath = "",
    [switch]$Bundle
)

$ErrorActionPreference = "Stop"

$sampleRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent (Split-Path -Parent $sampleRoot)

$args = @(
    "run",
    "--file",
    (Join-Path $repoRoot "scripts\csharp\wiki-build-local.cs"),
    "--",
    "--configuration",
    $Configuration
)

if ($PSBoundParameters.ContainsKey("BaseOutputPath")) {
    $args += "--base-output-path"
    $args += $BaseOutputPath
}

if ($PSBoundParameters.ContainsKey("BaseIntermediateOutputPath")) {
    $args += "--base-intermediate-output-path"
    $args += $BaseIntermediateOutputPath
}

if ($Bundle) {
    $args += "--bundle"
}

dotnet @args
exit $LASTEXITCODE
