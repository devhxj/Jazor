param(
    [string]$Configuration = "Release",
    [string]$OutputDirectory = ".artifacts\packages",
    [string]$Source = "https://api.nuget.org/v3/index.json",
    [string]$ApiKey = "",
    [string]$BaseOutputPath = "",
    [string]$BaseIntermediateOutputPath = "",
    [switch]$SkipPush,
    [switch]$NoBuild
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$packageProject = Join-Path $repoRoot "src\Jazor\Jazor.csproj"
$env:DOTNET_CLI_HOME = Join-Path $repoRoot ".dotnet"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$baseOutputPathWasExplicit = $PSBoundParameters.ContainsKey("BaseOutputPath")
$baseIntermediateOutputPathWasExplicit = $PSBoundParameters.ContainsKey("BaseIntermediateOutputPath")

if (-not (Test-Path $packageProject)) {
    throw "Package project not found: $packageProject"
}

function Invoke-DotNet {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    $effectiveArguments = [System.Collections.Generic.List[string]]::new()
    $effectiveArguments.AddRange($Arguments)
    if ($baseOutputPathWasExplicit) {
        $effectiveArguments.Add("-p:JazorIsolatedBaseOutputRoot=$(Get-IsolatedBuildRoot -Path $BaseOutputPath)")
    }
    if ($baseIntermediateOutputPathWasExplicit) {
        $isolatedIntermediateRoot = Get-IsolatedBuildRoot -Path $BaseIntermediateOutputPath
        $effectiveArguments.Add("-p:JazorIsolatedBaseIntermediateOutputRoot=$isolatedIntermediateRoot")
    }

    dotnet @effectiveArguments
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

function Get-IsolatedBuildRoot {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $resolvedPath = $Path
    if (-not $resolvedPath.Contains('$(', [StringComparison]::Ordinal)) {
        if (-not [System.IO.Path]::IsPathRooted($resolvedPath)) {
            $resolvedPath = Join-Path $repoRoot $resolvedPath
        }

        $resolvedPath = [System.IO.Path]::GetFullPath($resolvedPath)
    }

    if (-not $resolvedPath.EndsWith('\', [StringComparison]::Ordinal)) {
        $resolvedPath += '\'
    }

    return $resolvedPath
}

function Resolve-LocalPackInputPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ProjectDirectory,
        [Parameter(Mandatory = $true)]
        [string]$Include,
        [Parameter(Mandatory = $true)]
        [string]$Configuration,
        [Parameter(Mandatory = $true)]
        [string]$PackageBuildOutputRoot
    )

    $resolved = $Include.Replace('$(Configuration)', $Configuration)
    $resolved = $resolved.Replace('$(MSBuildThisFileDirectory)', "$ProjectDirectory\")
    $resolved = $resolved.Replace('$(JazorPackageBuildOutputRoot)', $PackageBuildOutputRoot)

    if ([System.IO.Path]::IsPathRooted($resolved)) {
        return $resolved
    }

    return [System.IO.Path]::GetFullPath((Join-Path $ProjectDirectory $resolved))
}

function Assert-NoBuildPackInputsExist {
    param(
        [Parameter(Mandatory = $true)]
        [string]$PackageProjectPath,
        [Parameter(Mandatory = $true)]
        [xml]$Project,
        [Parameter(Mandatory = $true)]
        [string]$Configuration
    )

    $projectDirectory = Split-Path -Parent $PackageProjectPath
    $missingInputs = New-Object System.Collections.Generic.List[string]
    $packageBuildOutputRoot = [System.IO.Path]::GetFullPath((Join-Path $projectDirectory "..\"))

    foreach ($itemGroup in $Project.Project.ItemGroup) {
        foreach ($noneItem in $itemGroup.None) {
            $include = [string]$noneItem.Include
            if ([string]::IsNullOrWhiteSpace($include)) {
                continue
            }

            if ($include.Contains('$([', [StringComparison]::Ordinal)) {
                continue
            }

            if ($include.Contains('$(NuGetPackageRoot)', [StringComparison]::Ordinal)) {
                continue
            }

            if ((-not $include.StartsWith("..\", [StringComparison]::Ordinal)) -and (-not $include.Contains('$(JazorPackageBuildOutputRoot)', [StringComparison]::Ordinal))) {
                continue
            }

            $resolvedPath = Resolve-LocalPackInputPath -ProjectDirectory $projectDirectory -Include $include -Configuration $Configuration -PackageBuildOutputRoot $packageBuildOutputRoot
            if (-not (Test-Path -LiteralPath $resolvedPath)) {
                $missingInputs.Add($resolvedPath)
            }
        }
    }

    $emitPublishDir = [System.IO.Path]::GetFullPath((Join-Path $projectDirectory "..\Jazor.Emit\bin\$Configuration\net10.0\publish"))
    if (-not (Test-Path -LiteralPath $emitPublishDir)) {
        $missingInputs.Add("$emitPublishDir (Jazor.Emit publish output directory)")
    }
    elseif (-not (Get-ChildItem -LiteralPath $emitPublishDir -Recurse -File | Select-Object -First 1)) {
        $missingInputs.Add("$emitPublishDir (Jazor.Emit publish output directory is empty)")
    }

    if ($missingInputs.Count -gt 0) {
        $details = ($missingInputs | Sort-Object | Get-Unique | ForEach-Object { " - $_" }) -join [Environment]::NewLine
        throw "NoBuild was requested, but required package inputs are missing.`n$details`nRun publish-nuget.ps1 once without -NoBuild to prepare the full package artifacts."
    }
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
    Assert-NoBuildPackInputsExist -PackageProjectPath $packageProject -Project $projectXml -Configuration $Configuration
    $restoreArgs = @(
        "restore",
        $packageProject,
        "-v", "minimal"
    )
    Invoke-DotNet -Arguments $restoreArgs
    $packArgs += "--no-build"
    $packArgs += "-p:JazorPreparePackageArtifacts=false"
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
