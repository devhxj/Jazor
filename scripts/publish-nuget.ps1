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
$emitProject = Join-Path $repoRoot "src\Jazor.Emit\Jazor.Emit.csproj"
$env:DOTNET_CLI_HOME = Join-Path $repoRoot ".dotnet"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"

if (-not (Test-Path $packageProject)) {
    throw "Package project not found: $packageProject"
}

function Invoke-DotNet {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($Arguments[0]) failed with exit code $LASTEXITCODE."
    }
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

if (-not $NoBuild) {
    $dependencyProjects = @(
        (Join-Path $repoRoot "src\ECMAScript\ECMAScript.csproj"),
        (Join-Path $repoRoot "src\ECMAScript.Contract\ECMAScript.Contract.csproj"),
        (Join-Path $repoRoot "src\ECMAScript.Vuetify\ECMAScript.Vuetify.csproj"),
        (Join-Path $repoRoot "src\Jazor.Common\Jazor.Common.csproj"),
        (Join-Path $repoRoot "src\Jazor.Compiler\Jazor.Compiler.csproj"),
        (Join-Path $repoRoot "src\Jazor.Analyzer\Jazor.Analyzer.csproj")
    )

    foreach ($project in $dependencyProjects) {
        Invoke-DotNet -Arguments @("build", $project, "-c", $Configuration, "-v", "minimal")
    }

    Invoke-DotNet -Arguments @("publish", $emitProject, "-c", $Configuration, "-v", "minimal")
}

$packArgs = @(
    "pack",
    $packageProject,
    "-c", $Configuration,
    "-o", $resolvedOutputDirectory,
    "-p:JazorPreparePackageArtifacts=false",
    "-v", "minimal"
)

if ($NoBuild) {
    $packArgs += "--no-build"
}
Invoke-DotNet -Arguments $packArgs

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
    $ApiKey = [Environment]::GetEnvironmentVariable("NUGET_API_KEY", "User")
}

if ([string]::IsNullOrWhiteSpace($ApiKey)) {
    $ApiKey = [Environment]::GetEnvironmentVariable("NUGET_API_KEY", "Machine")
}

$pushArgs = @(
    "nuget", "push",
    $packagePath,
    "--source", $Source,
    "--skip-duplicate"
)

if (-not [string]::IsNullOrWhiteSpace($ApiKey)) {
    $pushArgs += @("--api-key", $ApiKey)
}

Invoke-DotNet -Arguments $pushArgs

Write-Host "Published package: $packagePath"
