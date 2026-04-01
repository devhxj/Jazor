param(
    [ValidateSet("all", "compiler", "emit")]
    [string]$Project = "all",
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$env:DOTNET_CLI_HOME = Join-Path $repoRoot ".dotnet"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"

$compilerTestProject = Join-Path $repoRoot "src\Jazor.CompilerTest\Jazor.CompilerTest.csproj"
$emitTestProject = Join-Path $repoRoot "src\Jazor.EmitTest\Jazor.EmitTest.csproj"

$testTargets = switch ($Project) {
    "compiler" { @($compilerTestProject) }
    "emit" { @($emitTestProject) }
    default { @($compilerTestProject, $emitTestProject) }
}

foreach ($testProject in $testTargets) {
    dotnet test $testProject -c $Configuration /m:1 /p:BuildInParallel=false -v minimal
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet test failed for '$testProject' with exit code $LASTEXITCODE."
    }
}
