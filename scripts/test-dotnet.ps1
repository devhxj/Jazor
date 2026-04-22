param(
    [ValidateSet("all", "compiler", "clr", "razorvue", "jolt", "emit")]
    [string]$Project = "all",
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$env:DOTNET_CLI_HOME = Join-Path $repoRoot ".dotnet"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"

$compilerTestProject = Join-Path $repoRoot "src\Jazor.CompilerTest\Jazor.CompilerTest.csproj"
$clrTestProject = Join-Path $repoRoot "src\Jazor.CLR.Test\Jazor.CLR.Test.csproj"
$razorVueTestProject = Join-Path $repoRoot "src\Jazor.RazorVue.Test\Jazor.RazorVue.Test.csproj"
$joltTestProject = Join-Path $repoRoot "src\Jolt.Test\Jolt.Test.csproj"
$emitTestProject = Join-Path $repoRoot "src\Jazor.EmitTest\Jazor.EmitTest.csproj"

$testTargets = @(
    switch ($Project) {
        "compiler" { $compilerTestProject }
        "clr" { $clrTestProject }
        "razorvue" { $razorVueTestProject }
        "jolt" { $joltTestProject }
        "emit" { $emitTestProject }
        default { $compilerTestProject, $clrTestProject, $razorVueTestProject, $joltTestProject, $emitTestProject }
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
    dotnet test $testProject -c $Configuration --no-build --no-restore -v minimal
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet test failed for '$testProject' with exit code $LASTEXITCODE."
    }
}
