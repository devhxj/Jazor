#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.Net;
using System.Text;
var options = ScriptArguments.Parse(args);

var repoRoot = WikiScriptHelpers.RequireRepoRoot();
var sampleRoot = Path.Combine(repoRoot, "samples", "Wiki");
var hostProject = Path.Combine(sampleRoot, "Wiki.csproj");
var publishRoot = Path.Combine(repoRoot, ".tmp", "wiki-publish-browser-" + Environment.ProcessId);
var browserScriptPath = Path.Combine(sampleRoot, "verify-browser.mjs");
var runnerLog = Path.Combine(sampleRoot, $".wiki-browser-runner-{Environment.ProcessId}.log");
var stdoutLog = Path.Combine(sampleRoot, $".wiki-browser-{Environment.ProcessId}.stdout.log");
var stderrLog = Path.Combine(sampleRoot, $".wiki-browser-{Environment.ProcessId}.stderr.log");
var edgeStdoutLog = Path.Combine(sampleRoot, $".wiki-browser-edge-{Environment.ProcessId}.stdout.log");
var edgeStderrLog = Path.Combine(sampleRoot, $".wiki-browser-edge-{Environment.ProcessId}.stderr.log");
var nodeStdoutLog = Path.Combine(sampleRoot, $".wiki-browser-node-{Environment.ProcessId}.stdout.log");
var nodeStderrLog = Path.Combine(sampleRoot, $".wiki-browser-node-{Environment.ProcessId}.stderr.log");
var edgeUserDataRoot = Path.Combine(sampleRoot, $".wiki-browser-edge-profile-{Environment.ProcessId}");
var dotnetCliHome = Path.Combine(repoRoot, ".dotnet");
var edgeExecutable = WikiScriptHelpers.ResolveEdgeExecutable();
var nodeExecutable = WikiScriptHelpers.FindNodeOnPath()
    ?? throw new FileNotFoundException("Node.js executable 'node' was not found on PATH.");

if (options.Publish && (options.Build || options.BuildLocal))
{
    throw new InvalidOperationException("-Publish already performs its own publish build. Do not combine it with -Build or -BuildLocal.");
}

var normalizedPathBase = WikiScriptHelpers.NormalizePathBase(options.PathBase);
var rootUrl = $"http://localhost:{options.Port}";
var healthUrl = rootUrl + WikiScriptHelpers.GetExternalPath(normalizedPathBase, "/health");
var effectiveConfiguration = options.Publish && !options.ConfigurationWasExplicit
    ? "Release"
    : options.Configuration;

string hostRoot = sampleRoot;
string jazorRoot = Path.Combine(sampleRoot, "wwwroot", "jazor");
string? publishShadowJazorRoot = null;
Trace("Starting wiki browser verification.");

try
{
    if (options.Publish)
    {
        Trace("Publishing Wiki host.");
        WikiScriptHelpers.EnsureDirectoryDeletedWithinRepo(repoRoot, publishRoot);

        var publishArguments = new List<string>
        {
            "publish",
            hostProject,
            "-c",
            effectiveConfiguration,
            "-o",
            publishRoot,
            "/m:1",
            "/p:BuildInParallel=false",
            "/nr:false",
            "-p:UseSharedCompilation=false"
        };

        if (!string.IsNullOrWhiteSpace(options.BaseOutputPath))
        {
            publishArguments.Add("-p:JazorIsolatedBaseOutputRoot=" + WikiScriptHelpers.ResolveBuildRoot(repoRoot, options.BaseOutputPath));
        }

        if (!string.IsNullOrWhiteSpace(options.BaseIntermediateOutputPath))
        {
            publishArguments.Add("-p:JazorIsolatedBaseIntermediateOutputRoot=" + WikiScriptHelpers.ResolveBuildRoot(repoRoot, options.BaseIntermediateOutputPath));
        }

        await WikiScriptHelpers.RunDotNetAsync(
            publishArguments,
            workdir: repoRoot,
            dotnetCliHome: dotnetCliHome);
        Trace("Publish completed.");

        hostRoot = publishRoot;
        jazorRoot = Path.Combine(hostRoot, "wwwroot", "jazor");
        publishShadowJazorRoot = Path.Combine(hostRoot, "jazor");
    }
    else if (options.BuildLocal)
    {
        Trace("Running local Wiki build script.");
        var buildLocalArguments = new List<string>
        {
            "run",
            "--file",
            Path.Combine("scripts", "csharp", "wiki-build-local.cs"),
            "--",
            "--configuration",
            effectiveConfiguration
        };

        if (!string.IsNullOrWhiteSpace(options.BaseOutputPath))
        {
            buildLocalArguments.Add("--base-output-path");
            buildLocalArguments.Add(options.BaseOutputPath);
        }

        if (!string.IsNullOrWhiteSpace(options.BaseIntermediateOutputPath))
        {
            buildLocalArguments.Add("--base-intermediate-output-path");
            buildLocalArguments.Add(options.BaseIntermediateOutputPath);
        }

        await WikiScriptHelpers.RunDotNetAsync(
            buildLocalArguments,
            workdir: repoRoot,
            dotnetCliHome: dotnetCliHome);
        Trace("Local Wiki build completed.");
    }
    else if (options.Build)
    {
        Trace("Running Wiki build.");
        var buildArguments = new List<string>
        {
            "build",
            hostProject,
            "-c",
            effectiveConfiguration,
            "/m:1",
            "/p:BuildInParallel=false",
            "/nr:false",
            "-p:UseSharedCompilation=false"
        };

        if (!string.IsNullOrWhiteSpace(options.BaseOutputPath))
        {
            buildArguments.Add("-p:JazorIsolatedBaseOutputRoot=" + WikiScriptHelpers.ResolveBuildRoot(repoRoot, options.BaseOutputPath));
        }

        if (!string.IsNullOrWhiteSpace(options.BaseIntermediateOutputPath))
        {
            buildArguments.Add("-p:JazorIsolatedBaseIntermediateOutputRoot=" + WikiScriptHelpers.ResolveBuildRoot(repoRoot, options.BaseIntermediateOutputPath));
        }

        await WikiScriptHelpers.RunDotNetAsync(
            buildArguments,
            workdir: repoRoot,
            dotnetCliHome: dotnetCliHome);
        Trace("Wiki build completed.");
    }

    if (publishShadowJazorRoot is not null && Directory.Exists(publishShadowJazorRoot))
    {
        throw new InvalidOperationException("Unexpected publish shadow directory: " + publishShadowJazorRoot + ". Publish output must serve /jazor only from wwwroot/jazor.");
    }

    WikiScriptHelpers.EnsureFileExists(Path.Combine(jazorRoot, "main.mjs"), "emitted main module");
    WikiScriptHelpers.EnsureFileExists(Path.Combine(jazorRoot, "main.mjs.map"), "emitted main source map");
    WikiScriptHelpers.EnsureFileExists(Path.Combine(jazorRoot, "components", "wiki-home.mjs"), "emitted wiki component module");
    WikiScriptHelpers.EnsureFileExists(Path.Combine(jazorRoot, "components", "wiki-home.mjs.map"), "emitted wiki component source map");
    WikiScriptHelpers.EnsureFileExists(browserScriptPath, "browser verification script");

    Process? hostProcess = null;
    Process? edgeProcess = null;
    var keepLogs = false;
    try
    {
        Trace("Starting Wiki host process.");
        var hostArguments = options.Publish
            ? new[] { "Wiki.dll", "--urls", rootUrl }
            : new[] { "run", "--project", hostProject, "--no-launch-profile", "-c", effectiveConfiguration, "--no-build", "--no-restore", "--urls", rootUrl };

        hostProcess = WikiScriptHelpers.StartProcess(
            fileName: "dotnet",
            arguments: hostArguments,
            workdir: options.Publish ? hostRoot : sampleRoot,
            environment:
            [
                new KeyValuePair<string, string?>("DOTNET_CLI_HOME", dotnetCliHome),
                new KeyValuePair<string, string?>("DOTNET_SKIP_FIRST_TIME_EXPERIENCE", "1"),
                new KeyValuePair<string, string?>("ASPNETCORE_ENVIRONMENT", options.Publish ? "Production" : "Development"),
                new KeyValuePair<string, string?>("DOTNET_ENVIRONMENT", options.Publish ? "Production" : "Development"),
                new KeyValuePair<string, string?>("Wiki__PathBase", normalizedPathBase)
            ],
            stdoutLogPath: stdoutLog,
            stderrLogPath: stderrLog);
        Trace("Wiki host process started.");

        using var healthResponse = await WikiScriptHelpers.WaitForHttpOkAsync(
            healthUrl,
            hostProcess,
            TimeSpan.FromSeconds(options.StartupTimeoutSeconds),
            failureContext: $"See logs: {stdoutLog} ; {stderrLog}");
        Trace("Wiki host passed health check.");
        var healthBody = (await healthResponse.Content.ReadAsStringAsync()).Trim().Trim('"');
        if (healthBody != "ok")
        {
            throw new InvalidOperationException("Unexpected /health response body: '" + healthBody + "'");
        }

        if (Directory.Exists(edgeUserDataRoot))
        {
            Trace("Removing stale Edge profile directory.");
            await WikiScriptHelpers.RemoveDirectoryWithRetryAsync(edgeUserDataRoot);
        }

        Trace("Starting Edge headless browser.");
        edgeProcess = WikiScriptHelpers.StartProcess(
            fileName: edgeExecutable,
            arguments:
            [
                "--headless=new",
                "--disable-gpu",
                "--no-first-run",
                "--no-default-browser-check",
                "--remote-debugging-port=" + options.CdpPort,
                "--user-data-dir=" + edgeUserDataRoot,
                "about:blank"
            ],
            workdir: sampleRoot,
            stdoutLogPath: edgeStdoutLog,
            stderrLogPath: edgeStderrLog);
        Trace("Edge process started.");

        await WikiScriptHelpers.WaitForCdpReadyAsync(
            options.CdpPort,
            edgeProcess,
            TimeSpan.FromSeconds(options.BrowserStartupTimeoutSeconds),
            failureContext: $"See logs: {edgeStdoutLog} ; {edgeStderrLog}");
        Trace("Edge CDP endpoint is ready.");

        var verificationMode = options.Publish ? "production" : "development";
        Trace("Starting browser verification script.");
        await WikiScriptHelpers.RunProcessAsync(
            fileName: nodeExecutable,
            arguments:
            [
                browserScriptPath,
                rootUrl,
                options.CdpPort.ToString(),
                verificationMode,
                normalizedPathBase
            ],
            workdir: sampleRoot,
            stdoutLogPath: nodeStdoutLog,
            stderrLogPath: nodeStderrLog);
        Trace("Browser verification script completed.");

        Console.WriteLine(options.Publish
            ? "Wiki publish browser verification passed."
            : "Wiki browser verification passed.");
        Trace("Wiki browser verification passed.");
    }
    catch
    {
        keepLogs = true;
        Trace("Wiki browser verification failed.");
        throw;
    }
    finally
    {
        if (edgeProcess is not null && !edgeProcess.HasExited)
        {
            edgeProcess.Kill(entireProcessTree: true);
            await edgeProcess.WaitForExitAsync();
        }

        if (hostProcess is not null && !hostProcess.HasExited)
        {
            hostProcess.Kill(entireProcessTree: true);
            await hostProcess.WaitForExitAsync();
        }

        if (!keepLogs)
        {
            foreach (var logPath in new[] { runnerLog, stdoutLog, stderrLog, edgeStdoutLog, edgeStderrLog, nodeStdoutLog, nodeStderrLog })
            {
                if (File.Exists(logPath))
                {
                    await WikiScriptHelpers.RemoveFileWithRetryAsync(logPath);
                }
            }

            if (Directory.Exists(edgeUserDataRoot))
            {
                await WikiScriptHelpers.RemoveDirectoryWithRetryAsync(edgeUserDataRoot);
            }

            if (options.Publish && Directory.Exists(publishRoot))
            {
                await WikiScriptHelpers.RemoveDirectoryWithRetryAsync(publishRoot);
            }
        }
    }
}
finally
{
    if (options.Publish && Directory.Exists(publishRoot))
    {
        await WikiScriptHelpers.RemoveDirectoryWithRetryAsync(publishRoot);
    }
}

void Trace(string message)
{
    var line = $"[{DateTimeOffset.Now:O}] {message}";
    Console.WriteLine(line);
    File.AppendAllText(runnerLog, line + Environment.NewLine);
}

internal sealed record ScriptArguments
{
    public int Port { get; init; } = 4196;

    public int CdpPort { get; init; } = 9236;

    public string Configuration { get; init; } = "Debug";

    public bool ConfigurationWasExplicit { get; init; }

    public string? BaseOutputPath { get; init; }

    public string? BaseIntermediateOutputPath { get; init; }

    public string? PathBase { get; init; }

    public bool Build { get; init; }

    public bool BuildLocal { get; init; }

    public bool Publish { get; init; }

    public int StartupTimeoutSeconds { get; init; } = 30;

    public int BrowserStartupTimeoutSeconds { get; init; } = 15;

    public static ScriptArguments Parse(string[] args)
    {
        var result = new ScriptArguments();
        for (var index = 0; index < args.Length; index++)
        {
            var argument = args[index];
            switch (argument)
            {
                case "--port":
                    result = result with { Port = int.Parse(GetValue(args, ref index, argument)) };
                    break;
                case "--cdp-port":
                    result = result with { CdpPort = int.Parse(GetValue(args, ref index, argument)) };
                    break;
                case "--configuration":
                    result = result with
                    {
                        Configuration = GetValue(args, ref index, argument),
                        ConfigurationWasExplicit = true
                    };
                    break;
                case "--base-output-path":
                    result = result with { BaseOutputPath = GetValue(args, ref index, argument) };
                    break;
                case "--base-intermediate-output-path":
                    result = result with { BaseIntermediateOutputPath = GetValue(args, ref index, argument) };
                    break;
                case "--path-base":
                    result = result with { PathBase = GetValue(args, ref index, argument) };
                    break;
                case "--build":
                    result = result with { Build = true };
                    break;
                case "--build-local":
                    result = result with { BuildLocal = true };
                    break;
                case "--publish":
                    result = result with { Publish = true };
                    break;
                case "--startup-timeout-seconds":
                    result = result with { StartupTimeoutSeconds = int.Parse(GetValue(args, ref index, argument)) };
                    break;
                case "--browser-startup-timeout-seconds":
                    result = result with { BrowserStartupTimeoutSeconds = int.Parse(GetValue(args, ref index, argument)) };
                    break;
                case "--help":
                case "-h":
                    WriteUsage();
                    Environment.Exit(0);
                    break;
                default:
                    throw new InvalidOperationException("Unknown argument: " + argument);
            }
        }

        return result;
    }

    private static string GetValue(string[] args, ref int index, string argumentName)
    {
        if (index + 1 >= args.Length)
        {
            throw new InvalidOperationException("Missing value for " + argumentName);
        }

        index++;
        return args[index];
    }

    private static void WriteUsage()
    {
        Console.WriteLine("Usage: dotnet run --file scripts/csharp/wiki-verify-browser.cs -- [options]");
        Console.WriteLine("Options:");
        Console.WriteLine("  --port <number>");
        Console.WriteLine("  --cdp-port <number>");
        Console.WriteLine("  --configuration <Debug|Release>");
        Console.WriteLine("  --base-output-path <path>");
        Console.WriteLine("  --base-intermediate-output-path <path>");
        Console.WriteLine("  --path-base </docs>");
        Console.WriteLine("  --build");
        Console.WriteLine("  --build-local");
        Console.WriteLine("  --publish");
        Console.WriteLine("  --startup-timeout-seconds <seconds>");
        Console.WriteLine("  --browser-startup-timeout-seconds <seconds>");
    }
}

internal static class WikiScriptHelpers
{
    public static readonly StringComparer PathComparer =
        OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    public static string RequireRepoRoot()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Repository root containing Jazor.slnx was not found from the current directory upward.");
    }

    public static string NormalizePathBase(string? pathBase)
    {
        if (string.IsNullOrWhiteSpace(pathBase) || pathBase == "/")
        {
            return string.Empty;
        }

        if (!pathBase.StartsWith('/', StringComparison.Ordinal))
        {
            throw new InvalidOperationException("-PathBase must start with '/'.");
        }

        return pathBase.Length > 1 && pathBase.EndsWith('/', StringComparison.Ordinal)
            ? pathBase[..^1]
            : pathBase;
    }

    public static string GetExternalPath(string normalizedPathBase, string logicalPath)
    {
        if (string.IsNullOrWhiteSpace(logicalPath) || !logicalPath.StartsWith('/', StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Logical path must start with '/': " + logicalPath);
        }

        if (string.IsNullOrEmpty(normalizedPathBase))
        {
            return logicalPath;
        }

        return logicalPath == "/"
            ? normalizedPathBase + "/"
            : normalizedPathBase + logicalPath;
    }

    public static string ResolveBuildRoot(string repoRoot, string path)
    {
        if (path.Contains("$(", StringComparison.Ordinal))
        {
            return path;
        }

        var resolved = Path.IsPathRooted(path)
            ? Path.GetFullPath(path)
            : Path.GetFullPath(Path.Combine(repoRoot, path));

        return resolved.EndsWith(Path.DirectorySeparatorChar)
            ? resolved
            : resolved + Path.DirectorySeparatorChar;
    }

    public static async Task RunDotNetAsync(
        IReadOnlyList<string> arguments,
        string workdir,
        string dotnetCliHome,
        string? aspNetCoreUrls = null,
        string? aspNetCoreEnvironment = null,
        string? dotNetEnvironment = null,
        string? wikiPathBase = null,
        string? stdoutLogPath = null,
        string? stderrLogPath = null,
        CancellationToken cancellationToken = default)
    {
        using var process = StartProcess(
            fileName: "dotnet",
            arguments: arguments,
            workdir: workdir,
            environment:
            [
                new KeyValuePair<string, string?>("DOTNET_CLI_HOME", dotnetCliHome),
                new KeyValuePair<string, string?>("DOTNET_SKIP_FIRST_TIME_EXPERIENCE", "1"),
                new KeyValuePair<string, string?>("ASPNETCORE_URLS", aspNetCoreUrls),
                new KeyValuePair<string, string?>("ASPNETCORE_ENVIRONMENT", aspNetCoreEnvironment),
                new KeyValuePair<string, string?>("DOTNET_ENVIRONMENT", dotNetEnvironment),
                new KeyValuePair<string, string?>("Wiki__PathBase", wikiPathBase)
            ],
            stdoutLogPath: stdoutLogPath,
            stderrLogPath: stderrLogPath);

        await process.WaitForExitAsync(cancellationToken);
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Process failed with exit code {process.ExitCode}: dotnet {string.Join(' ', arguments)}");
        }
    }

    public static async Task RunProcessAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        string workdir,
        IReadOnlyList<KeyValuePair<string, string?>>? environment = null,
        string? stdoutLogPath = null,
        string? stderrLogPath = null,
        CancellationToken cancellationToken = default)
    {
        using var process = StartProcess(fileName, arguments, workdir, environment, stdoutLogPath, stderrLogPath);
        await process.WaitForExitAsync(cancellationToken);
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Process failed with exit code {process.ExitCode}: {fileName} {string.Join(' ', arguments)}");
        }
    }

    public static Process StartProcess(
        string fileName,
        IReadOnlyList<string> arguments,
        string workdir,
        IReadOnlyList<KeyValuePair<string, string?>>? environment = null,
        string? stdoutLogPath = null,
        string? stderrLogPath = null)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            WorkingDirectory = workdir,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = stdoutLogPath is not null,
            RedirectStandardError = stderrLogPath is not null
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        if (environment is not null)
        {
            foreach (var entry in environment)
            {
                if (entry.Value is null)
                {
                    startInfo.Environment.Remove(entry.Key);
                }
                else
                {
                    startInfo.Environment[entry.Key] = entry.Value;
                }
            }
        }

        var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start process: " + fileName);

        if (stdoutLogPath is not null)
        {
            _ = RedirectAsync(process.StandardOutput, stdoutLogPath);
        }

        if (stderrLogPath is not null)
        {
            _ = RedirectAsync(process.StandardError, stderrLogPath);
        }

        return process;
    }

    public static async Task<HttpResponseMessage> WaitForHttpOkAsync(
        string url,
        Process process,
        TimeSpan timeout,
        string? failureContext = null,
        CancellationToken cancellationToken = default)
    {
        using var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(3)
        };

        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (process.HasExited)
            {
                throw new InvalidOperationException(
                    "Process exited before responding on " + url + "." + FormatFailureContext(failureContext));
            }

            try
            {
                var response = await client.GetAsync(url, cancellationToken);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return response;
                }
            }
            catch
            {
            }

            await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
        }

        throw new TimeoutException("Timed out waiting for " + url + "." + FormatFailureContext(failureContext));
    }

    public static async Task WaitForCdpReadyAsync(
        int port,
        Process process,
        TimeSpan timeout,
        string? failureContext = null,
        CancellationToken cancellationToken = default)
    {
        using var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(2)
        };

        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (process.HasExited)
            {
                throw new InvalidOperationException(
                    "Browser process exited before the CDP endpoint became ready." + FormatFailureContext(failureContext));
            }

            try
            {
                using var response = await client.GetAsync($"http://127.0.0.1:{port}/json/list", cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var payload = await response.Content.ReadAsStringAsync(cancellationToken);
                    if (!string.IsNullOrWhiteSpace(payload) && payload.Trim() != "[]")
                    {
                        return;
                    }
                }
            }
            catch
            {
            }

            await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);
        }

        throw new TimeoutException(
            "Timed out waiting for Edge CDP endpoint on port " + port + "." + FormatFailureContext(failureContext));
    }

    public static void EnsureFileExists(string path, string description)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Missing " + description + ": " + path, path);
        }
    }

    public static void EnsureDirectoryDeletedWithinRepo(string repoRoot, string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        var fullRepoRoot = Path.GetFullPath(repoRoot);
        var fullPath = Path.GetFullPath(path);
        if (!fullPath.StartsWith(fullRepoRoot, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Refusing to delete outside repository root: " + fullPath);
        }

        Directory.Delete(fullPath, recursive: true);
    }

    public static async Task RemoveFileWithRetryAsync(string path, int attempts = 6, int delayMilliseconds = 250)
    {
        for (var attempt = 0; attempt < attempts; attempt++)
        {
            if (!File.Exists(path))
            {
                return;
            }

            try
            {
                File.Delete(path);
                return;
            }
            catch when (attempt < attempts - 1)
            {
                await Task.Delay(delayMilliseconds);
            }
        }
    }

    public static async Task RemoveDirectoryWithRetryAsync(string path, int attempts = 6, int delayMilliseconds = 250)
    {
        for (var attempt = 0; attempt < attempts; attempt++)
        {
            if (!Directory.Exists(path))
            {
                return;
            }

            try
            {
                Directory.Delete(path, recursive: true);
                return;
            }
            catch when (attempt < attempts - 1)
            {
                await Task.Delay(delayMilliseconds);
            }
        }
    }

    public static string? FindNodeOnPath()
        => FindExecutableOnPath(OperatingSystem.IsWindows() ? "node.exe" : "node");

    public static string ResolveEdgeExecutable()
    {
        var candidates = OperatingSystem.IsWindows()
            ? new[]
            {
                @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
                @"C:\Program Files\Microsoft\Edge\Application\msedge.exe"
            }
            : Array.Empty<string>();

        foreach (var candidate in candidates)
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new FileNotFoundException("Microsoft Edge executable was not found in the expected install locations.");
    }

    private static async Task RedirectAsync(StreamReader? reader, string? logPath)
    {
        if (reader is null || logPath is null)
        {
            return;
        }

        await using var writer = new StreamWriter(logPath, append: false, Encoding.UTF8);
        while (true)
        {
            var line = await reader.ReadLineAsync();
            if (line is null)
            {
                break;
            }

            await writer.WriteLineAsync(line);
        }
    }

    private static string? FindExecutableOnPath(string fileName)
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        foreach (var segment in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            try
            {
                var candidate = Path.Combine(segment, fileName);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
            catch
            {
            }
        }

        return null;
    }

    private static string FormatFailureContext(string? failureContext)
        => string.IsNullOrWhiteSpace(failureContext) ? string.Empty : " " + failureContext;
}
