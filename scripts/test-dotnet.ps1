param(
    [string]$Project = "all",
    [string]$Configuration = "Debug",
    [string]$Filter = "",
    [string]$BaseOutputPath = "",
    [string]$BaseIntermediateOutputPath = ""
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$args = @(
    "run",
    "--file",
    (Join-Path $repoRoot "scripts\csharp\test-dotnet.cs"),
    "--",
    "--project",
    $Project,
    "--configuration",
    $Configuration
)

if ($PSBoundParameters.ContainsKey("Filter")) {
    $args += "--filter"
    $args += $Filter
}

if ($PSBoundParameters.ContainsKey("BaseOutputPath")) {
    $args += "--base-output-path"
    $args += $BaseOutputPath
}

if ($PSBoundParameters.ContainsKey("BaseIntermediateOutputPath")) {
    $args += "--base-intermediate-output-path"
    $args += $BaseIntermediateOutputPath
}

dotnet @args
exit $LASTEXITCODE
