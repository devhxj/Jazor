param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$Arguments
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$consumerRoot = Split-Path -Parent $scriptRoot
$repoRoot = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $consumerRoot))

$explicitDenoPath = $env:JAZOR_DENO_EXE
if (-not [string]::IsNullOrWhiteSpace($explicitDenoPath)) {
    if (-not (Test-Path -LiteralPath $explicitDenoPath)) {
        throw "Explicit JAZOR_DENO_EXE path does not exist: $explicitDenoPath"
    }

    $denoPath = $explicitDenoPath
}
else {
$candidatePaths = @(
    (Join-Path $repoRoot "src\Jolt\bin\Debug\net11.0\runtimes\win-x64\native\deno.exe"),
    (Join-Path $repoRoot "src\Jolt\bin\Release\net11.0\runtimes\win-x64\native\deno.exe"),
    (Join-Path $repoRoot "src\Jazor.Emit\bin\Debug\net11.0\runtimes\win-x64\native\deno.exe"),
    (Join-Path $repoRoot "src\Jazor.Emit\bin\Release\net11.0\runtimes\win-x64\native\deno.exe"),
    (Join-Path $repoRoot ".dotnet\.nuget\packages\denohost.runtime.win-x64\2.7.14\runtimes\win-x64\native\deno.exe")
)

$denoPath = $candidatePaths | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
if ([string]::IsNullOrWhiteSpace($denoPath)) {
    throw "Bundled Deno runtime was not found. Build Jolt or Jazor.Emit first so DenoHost runtime assets exist."
}
}

& $denoPath @Arguments
if ($LASTEXITCODE -ne 0) {
    throw "Bundled deno command failed with exit code $LASTEXITCODE."
}
