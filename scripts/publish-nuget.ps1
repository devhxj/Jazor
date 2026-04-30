param(
    [string]$Configuration = "Release",
    [string]$OutputDirectory = ".artifacts\packages",
    [string]$Source = "https://api.nuget.org/v3/index.json",
    [string]$ApiKey = "",
    [switch]$SkipPush,
    [switch]$NoBuild
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$packageProject = Join-Path $repoRoot "src\Jazor\Jazor.csproj"
$env:DOTNET_CLI_HOME = Join-Path $repoRoot ".dotnet"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"

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

$packageVersion = Get-ProjectPropertyValue -Project $projectXml -Name "Version"
if ([string]::IsNullOrWhiteSpace($packageVersion)) {
    $versionPrefix = Get-ProjectPropertyValue -Project $projectXml -Name "VersionPrefix"
    $versionSuffix = Get-ProjectPropertyValue -Project $projectXml -Name "VersionSuffix"
    $packageVersion = if ([string]::IsNullOrWhiteSpace($versionSuffix)) { $versionPrefix } else { "$versionPrefix-$versionSuffix" }
}

if ([string]::IsNullOrWhiteSpace($packageVersion)) {
    throw "Unable to resolve package version from $packageProject"
}

$resolvedOutputDirectory = if ([System.IO.Path]::IsPathRooted($OutputDirectory)) {
    $OutputDirectory
} else {
    Join-Path $repoRoot $OutputDirectory
}

New-Item -ItemType Directory -Path $resolvedOutputDirectory -Force | Out-Null

$packArgs = @(
    "pack",
    $packageProject,
    "-c", $Configuration,
    "-o", $resolvedOutputDirectory,
    "-v", "minimal"
)

if ($NoBuild) {
    $packArgs += "--no-build"
    $packArgs += "-p:JazorPreparePackageArtifacts=false"
}

dotnet @packArgs
if ($LASTEXITCODE -ne 0) {
    throw "dotnet pack failed for '$packageProject' with exit code $LASTEXITCODE."
}

$packagePath = Join-Path $resolvedOutputDirectory "$packageId.$packageVersion.nupkg"
if (-not (Test-Path $packagePath)) {
    $matches = Get-ChildItem -Path $resolvedOutputDirectory -Filter "$packageId.$packageVersion*.nupkg" -File |
        Where-Object { $_.Name -notlike "*.snupkg" } |
        Sort-Object LastWriteTimeUtc -Descending

    if ($matches.Count -eq 0) {
        throw "Packed package not found under '$resolvedOutputDirectory'."
    }

    $packagePath = $matches[0].FullName
}

Write-Host "Packed package: $packagePath"

if ($SkipPush) {
    Write-Host "SkipPush set. Package was not pushed."
    return
}

if ([string]::IsNullOrWhiteSpace($ApiKey)) {
    $ApiKey = $env:NUGET_API_KEY
}

if ([string]::IsNullOrWhiteSpace($ApiKey)) {
    throw "NuGet API key is required. Pass -ApiKey or set NUGET_API_KEY."
}

$pushArgs = @(
    "nuget", "push",
    $packagePath,
    "--api-key", $ApiKey,
    "--source", $Source,
    "--skip-duplicate"
)

dotnet @pushArgs
if ($LASTEXITCODE -ne 0) {
    throw "dotnet nuget push failed for '$packagePath' with exit code $LASTEXITCODE."
}

Write-Host "Published package: $packagePath"
