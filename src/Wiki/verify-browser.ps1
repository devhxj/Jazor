param(
    [int]$Port = 4196,
    [int]$CdpPort = 9236,
    [string]$Configuration = "Debug",
    [string]$BaseOutputPath = "",
    [string]$BaseIntermediateOutputPath = "",
    [switch]$Build,
    [switch]$BuildLocal,
    [switch]$Publish,
    [int]$StartupTimeoutSeconds = 30,
    [int]$BrowserStartupTimeoutSeconds = 15
)

$ErrorActionPreference = "Stop"

$sampleRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent (Split-Path -Parent $sampleRoot)
$hostProject = Join-Path $sampleRoot "Wiki.csproj"
$publishRoot = Join-Path $repoRoot ".tmp\wiki-publish-browser-$PID"
$hostRoot = $sampleRoot
$jazorRoot = Join-Path $sampleRoot "jazor"
$publishShadowJazorRoot = $null
$mainModulePath = Join-Path $jazorRoot "main.mjs"
$mainSourceMapPath = Join-Path $jazorRoot "main.mjs.map"
$componentModulePath = Join-Path $jazorRoot "components\wiki-home.mjs"
$componentSourceMapPath = Join-Path $jazorRoot "components\wiki-home.mjs.map"
$browserScriptPath = Join-Path $sampleRoot "verify-browser.mjs"
$stdoutLog = Join-Path $sampleRoot ".wiki-browser-$PID.stdout.log"
$stderrLog = Join-Path $sampleRoot ".wiki-browser-$PID.stderr.log"
$edgeStdoutLog = Join-Path $sampleRoot ".wiki-browser-edge-$PID.stdout.log"
$edgeStderrLog = Join-Path $sampleRoot ".wiki-browser-edge-$PID.stderr.log"
$edgeUserDataRoot = Join-Path $sampleRoot ".wiki-browser-edge-profile-$PID"
$rootUrl = "http://localhost:$Port"
$healthUrl = "$rootUrl/health"
$edgeExecutable = "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"

$env:DOTNET_CLI_HOME = Join-Path $repoRoot ".dotnet"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$baseOutputPathWasExplicit = $PSBoundParameters.ContainsKey("BaseOutputPath")
$baseIntermediateOutputPathWasExplicit = $PSBoundParameters.ContainsKey("BaseIntermediateOutputPath")

if ($Publish -and ($Build -or $BuildLocal)) {
    throw "-Publish already performs its own publish build. Do not combine it with -Build or -BuildLocal."
}

if ($Publish -and -not $PSBoundParameters.ContainsKey("Configuration")) {
    $Configuration = "Release"
}

function Invoke-DotNet {
    param([string[]]$DotNetArgs)

    if ($baseOutputPathWasExplicit) {
        $DotNetArgs += "-p:JazorIsolatedBaseOutputRoot=$BaseOutputPath"
    }

    if ($baseIntermediateOutputPathWasExplicit) {
        $DotNetArgs += "-p:JazorIsolatedBaseIntermediateOutputRoot=$BaseIntermediateOutputPath"
    }

    $DotNetArgs += "/nr:false"
    $DotNetArgs += "-p:UseSharedCompilation=false"

    dotnet @DotNetArgs
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet command failed with exit code ${LASTEXITCODE}: dotnet $($DotNetArgs -join ' ')"
    }
}

function Invoke-Script {
    param(
        [string]$Path,
        [string[]]$Args
    )

    & $Path @Args
    if ($LASTEXITCODE -ne 0) {
        throw "script failed with exit code ${LASTEXITCODE}: $Path $($Args -join ' ')"
    }
}

function Assert-PathExists {
    param(
        [string]$Path,
        [string]$Description
    )

    if (-not (Test-Path $Path)) {
        throw "Missing ${Description}: $Path"
    }
}

function Remove-FileWithRetry {
    param(
        [string]$Path,
        [int]$Attempts = 6,
        [int]$DelayMilliseconds = 250
    )

    for ($attempt = 0; $attempt -lt $Attempts; $attempt++) {
        if (-not (Test-Path $Path)) {
            return
        }

        try {
            Remove-Item -LiteralPath $Path -Force
            return
        }
        catch {
            if ($attempt -ge ($Attempts - 1)) {
                throw
            }

            Start-Sleep -Milliseconds $DelayMilliseconds
        }
    }
}

function Remove-PathWithRetry {
    param(
        [string]$Path,
        [switch]$Recurse,
        [int]$Attempts = 6,
        [int]$DelayMilliseconds = 250
    )

    for ($attempt = 0; $attempt -lt $Attempts; $attempt++) {
        if (-not (Test-Path $Path)) {
            return
        }

        try {
            if ($Recurse) {
                Remove-Item -LiteralPath $Path -Recurse -Force
            }
            else {
                Remove-Item -LiteralPath $Path -Force
            }

            return
        }
        catch {
            if ($attempt -ge ($Attempts - 1)) {
                throw
            }

            Start-Sleep -Milliseconds $DelayMilliseconds
        }
    }
}

function Restore-EnvironmentVariable {
    param(
        [string]$Name,
        [AllowNull()][string]$Value
    )

    if ($null -eq $Value) {
        Remove-Item -Path ("Env:" + $Name) -ErrorAction SilentlyContinue
        return
    }

    Set-Item -Path ("Env:" + $Name) -Value $Value
}

function Wait-ForHttpOk {
    param(
        [string]$Url,
        [System.Diagnostics.Process]$Process,
        [int]$TimeoutSeconds
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        if ($Process.HasExited) {
            throw "Wiki host exited before responding. See logs: $stdoutLog ; $stderrLog"
        }

        try {
            $response = Invoke-WebRequest -Uri $Url -TimeoutSec 3
            if ($response.StatusCode -eq 200) {
                return $response
            }
        }
        catch {
        }

        Start-Sleep -Milliseconds 500
    } while ((Get-Date) -lt $deadline)

    throw "Timed out waiting for $Url. See logs: $stdoutLog ; $stderrLog"
}

function Wait-ForCdpReady {
    param(
        [int]$Port,
        [System.Diagnostics.Process]$Process,
        [int]$TimeoutSeconds
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        if ($Process.HasExited) {
            throw "Edge process exited before the CDP endpoint became ready. See logs: $edgeStdoutLog ; $edgeStderrLog"
        }

        try {
            $targets = Invoke-RestMethod -Uri ("http://127.0.0.1:" + $Port + "/json/list") -TimeoutSec 2
            if ($targets) {
                return
            }
        }
        catch {
        }

        Start-Sleep -Milliseconds 250
    } while ((Get-Date) -lt $deadline)

    throw "Timed out waiting for Edge CDP endpoint on port $Port. See logs: $edgeStdoutLog ; $edgeStderrLog"
}

if (-not (Get-Command node -ErrorAction SilentlyContinue)) {
    throw "Node.js executable 'node' was not found on PATH."
}

Assert-PathExists -Path $edgeExecutable -Description "Microsoft Edge executable"
Assert-PathExists -Path $browserScriptPath -Description "browser verification script"

if ($Publish) {
    Invoke-DotNet @("publish", $hostProject, "-c", $Configuration, "-o", $publishRoot, "/m:1", "/p:BuildInParallel=false")

    $hostRoot = $publishRoot
    $jazorRoot = Join-Path $hostRoot "wwwroot\jazor"
    $publishShadowJazorRoot = Join-Path $hostRoot "jazor"
    $mainModulePath = Join-Path $jazorRoot "main.mjs"
    $mainSourceMapPath = Join-Path $jazorRoot "main.mjs.map"
    $componentModulePath = Join-Path $jazorRoot "components\wiki-home.mjs"
    $componentSourceMapPath = Join-Path $jazorRoot "components\wiki-home.mjs.map"
}
elseif ($BuildLocal) {
    $buildScript = Join-Path $sampleRoot "build-local.ps1"
    $buildLocalArgs = @("-Configuration", $Configuration)
    if ($baseOutputPathWasExplicit) {
        $buildLocalArgs += @("-BaseOutputPath", $BaseOutputPath)
    }
    if ($baseIntermediateOutputPathWasExplicit) {
        $buildLocalArgs += @("-BaseIntermediateOutputPath", $BaseIntermediateOutputPath)
    }

    Invoke-Script -Path $buildScript -Args $buildLocalArgs
}
elseif ($Build) {
    Invoke-DotNet @("build", $hostProject, "-c", $Configuration, "/m:1", "/p:BuildInParallel=false")
}

if ($Publish -and $publishShadowJazorRoot -and (Test-Path $publishShadowJazorRoot)) {
    throw "Unexpected publish shadow directory: $publishShadowJazorRoot. Publish output must serve /jazor only from wwwroot/jazor."
}

Assert-PathExists -Path $mainModulePath -Description "emitted main module"
Assert-PathExists -Path $mainSourceMapPath -Description "emitted main source map"
Assert-PathExists -Path $componentModulePath -Description "emitted wiki component module"
Assert-PathExists -Path $componentSourceMapPath -Description "emitted wiki component source map"

$previousAspNetCoreUrls = [Environment]::GetEnvironmentVariable("ASPNETCORE_URLS")
$previousAspNetCoreEnvironment = [Environment]::GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
$previousDotNetEnvironment = [Environment]::GetEnvironmentVariable("DOTNET_ENVIRONMENT")
$hostProcess = $null
$edgeProcess = $null
$keepLogs = $false

try {
    $hostStartFilePath = "dotnet"
    $hostStartArgumentList = @("run", "--project", $hostProject, "--no-launch-profile", "-c", $Configuration, "--no-build", "--no-restore")
    $hostStartWorkingDirectory = $sampleRoot

    if ($Publish) {
        $hostStartArgumentList = @("Wiki.dll", "--urls", $rootUrl)
        $hostStartWorkingDirectory = $hostRoot
        $env:ASPNETCORE_ENVIRONMENT = "Production"
        $env:DOTNET_ENVIRONMENT = "Production"
    }
    else {
        $env:ASPNETCORE_URLS = $rootUrl
        $env:ASPNETCORE_ENVIRONMENT = "Development"
        $env:DOTNET_ENVIRONMENT = "Development"
    }

    $hostProcess = Start-Process `
        -FilePath $hostStartFilePath `
        -ArgumentList $hostStartArgumentList `
        -WorkingDirectory $hostStartWorkingDirectory `
        -RedirectStandardOutput $stdoutLog `
        -RedirectStandardError $stderrLog `
        -PassThru `
        -WindowStyle Hidden

    $healthResponse = Wait-ForHttpOk -Url $healthUrl -Process $hostProcess -TimeoutSeconds $StartupTimeoutSeconds
    $healthBody = $healthResponse.Content.Trim().Trim('"')
    if ($healthBody -ne "ok") {
        throw "Unexpected /health response body: '$($healthResponse.Content.Trim())'"
    }

    if (Test-Path $edgeUserDataRoot) {
        Remove-PathWithRetry -Path $edgeUserDataRoot -Recurse
    }

    $edgeArgs = @(
        "--headless=new",
        "--disable-gpu",
        "--no-first-run",
        "--no-default-browser-check",
        ("--remote-debugging-port=" + $CdpPort),
        ("--user-data-dir=" + $edgeUserDataRoot),
        "about:blank"
    )

    $edgeProcess = Start-Process `
        -FilePath $edgeExecutable `
        -ArgumentList $edgeArgs `
        -RedirectStandardOutput $edgeStdoutLog `
        -RedirectStandardError $edgeStderrLog `
        -PassThru `
        -WindowStyle Hidden

    Wait-ForCdpReady -Port $CdpPort -Process $edgeProcess -TimeoutSeconds $BrowserStartupTimeoutSeconds

    $verificationMode = if ($Publish) { "production" } else { "development" }
    node $browserScriptPath $rootUrl $CdpPort $verificationMode
    if ($LASTEXITCODE -ne 0) {
        throw "Wiki browser verification failed."
    }

    if ($Publish) {
        Write-Host "Wiki publish browser verification passed."
        Write-Host "Verified: published wwwroot/jazor runtime mount, no development reload injection, debugger-visible source maps for compiled modules, SPA navigation, search/not-found recovery, persisted shell state, section/hash routing, copy affordances, mobile drawers, and clean browser runtime."
    }
    else {
        Write-Host "Wiki browser verification passed."
        Write-Host "Verified: Development-mode local jazor runtime mount, dev reload client and websocket, debugger-visible source maps for compiled modules, SPA navigation, search/not-found recovery, persisted shell state, section/hash routing, copy affordances, mobile drawers, and clean browser runtime."
    }
}
catch {
    $keepLogs = $true
    throw
}
finally {
    if ($edgeProcess -and -not $edgeProcess.HasExited) {
        Stop-Process -Id $edgeProcess.Id -Force
        Wait-Process -Id $edgeProcess.Id -Timeout 5 -ErrorAction SilentlyContinue
    }

    if ($hostProcess -and -not $hostProcess.HasExited) {
        Stop-Process -Id $hostProcess.Id -Force
        Wait-Process -Id $hostProcess.Id -Timeout 5 -ErrorAction SilentlyContinue
    }

    Restore-EnvironmentVariable -Name "ASPNETCORE_URLS" -Value $previousAspNetCoreUrls
    Restore-EnvironmentVariable -Name "ASPNETCORE_ENVIRONMENT" -Value $previousAspNetCoreEnvironment
    Restore-EnvironmentVariable -Name "DOTNET_ENVIRONMENT" -Value $previousDotNetEnvironment

    if (-not $keepLogs) {
        foreach ($logPath in @($stdoutLog, $stderrLog, $edgeStdoutLog, $edgeStderrLog)) {
            if (Test-Path $logPath) {
                Remove-FileWithRetry -Path $logPath
            }
        }

        if (Test-Path $edgeUserDataRoot) {
            Remove-PathWithRetry -Path $edgeUserDataRoot -Recurse
        }

        if ($Publish -and (Test-Path $publishRoot)) {
            Remove-PathWithRetry -Path $publishRoot -Recurse
        }
    }
}
