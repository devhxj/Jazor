param(
    [string]$Configuration = "Release",
    [string]$OutputDirectory = ".verify-out\nuget-preflight"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$publishScript = Join-Path $PSScriptRoot "publish-nuget.ps1"
$packageProject = Join-Path $repoRoot "src\Jazor\Jazor.csproj"
$env:DOTNET_CLI_HOME = Join-Path $repoRoot ".dotnet"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"

if (-not (Test-Path $publishScript)) {
    throw "Publish script not found: $publishScript"
}

if (-not (Test-Path $packageProject)) {
    throw "Package project not found: $packageProject"
}

function Get-ProjectPropertyValue {
    param(
        [Parameter(Mandatory = $true)]
        [xml]$Project,
        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    foreach ($propertyGroup in $Project.Project.PropertyGroup) {
        $value = $propertyGroup.$Name
        if (-not [string]::IsNullOrWhiteSpace($value)) {
            return [string]$value
        }
    }

    return ""
}

[xml]$projectXml = Get-Content $packageProject
$packageId = Get-ProjectPropertyValue -Project $projectXml -Name "PackageId"
if ([string]::IsNullOrWhiteSpace($packageId)) {
    $packageId = [System.IO.Path]::GetFileNameWithoutExtension($packageProject)
}

$resolvedOutputDirectory = if ([System.IO.Path]::IsPathRooted($OutputDirectory)) {
    $OutputDirectory
} else {
    Join-Path $repoRoot $OutputDirectory
}

if (Test-Path $resolvedOutputDirectory) {
    Remove-Item -LiteralPath $resolvedOutputDirectory -Recurse -Force
}

New-Item -ItemType Directory -Path $resolvedOutputDirectory -Force | Out-Null

pwsh $publishScript -Configuration $Configuration -OutputDirectory $resolvedOutputDirectory -SkipPush
if ($LASTEXITCODE -ne 0) {
    throw "publish-nuget.ps1 failed with exit code $LASTEXITCODE."
}

$packageFile = Get-ChildItem -Path $resolvedOutputDirectory -Filter "$packageId.*.nupkg" -File |
    Where-Object { $_.Name -notlike "*.snupkg" } |
    Sort-Object LastWriteTimeUtc -Descending |
    Select-Object -First 1
if (-not $packageFile) {
    throw "Expected package not found under: $resolvedOutputDirectory"
}

$packagePath = $packageFile.FullName
$packageVersion = $packageFile.BaseName -replace "^$([regex]::Escape($packageId))\.", ''
if ([string]::IsNullOrWhiteSpace($packageVersion) -or $packageVersion -eq $packageFile.BaseName) {
    throw "Unable to resolve package version from produced package: $packagePath"
}

$expandedDirectory = Join-Path $resolvedOutputDirectory "expanded"
Expand-Archive -LiteralPath $packagePath -DestinationPath $expandedDirectory -Force

$requiredPaths = @(
    "README.md",
    "LICENSE.txt",
    "NOTICE.txt",
    "buildTransitive\Jazor.props",
    "buildTransitive\Jazor.targets",
    "analyzers\dotnet\cs\Jazor.Analyzer.dll",
    "analyzers\dotnet\cs\Jazor.Compiler.dll",
    "lib\net11.0\ECMAScript.dll",
    "lib\net11.0\Jazor.Compiler.dll",
    "tools\net11.0\Jazor.Emit.dll",
    "tools\net11.0\runtimes\win-x64\native\deno.exe"
)

foreach ($relativePath in $requiredPaths) {
    $fullPath = Join-Path $expandedDirectory $relativePath
    if (-not (Test-Path $fullPath)) {
        throw "Required package entry is missing: $relativePath"
    }
}

$nuspecPath = Join-Path $expandedDirectory "$packageId.nuspec"
if (-not (Test-Path $nuspecPath)) {
    throw "Nuspec not found after package expansion: $nuspecPath"
}

[xml]$nuspec = Get-Content $nuspecPath
$metadata = $nuspec.package.metadata

if ($metadata.id -ne $packageId) {
    throw "Unexpected package id in nuspec. Expected '$packageId', got '$($metadata.id)'."
}

if ($metadata.version -ne $packageVersion) {
    throw "Unexpected package version in nuspec. Expected '$packageVersion', got '$($metadata.version)'."
}

if ($metadata.license.type -ne "file" -or $metadata.license.'#text' -ne "LICENSE.txt") {
    throw "Package license metadata is not configured as LICENSE.txt."
}

if ($metadata.readme -ne "README.md") {
    throw "Package readme metadata is not configured as README.md."
}

Write-Host "Package verification passed: $packagePath"
