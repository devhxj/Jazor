#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;
using System.Xml.Linq;

// Windows SSR release consumer gate: packages Jazor locally, publishes the RazorVue TodoList
// sample as an isolated NuGet consumer with JazorSSR=true, and verifies the published app
// end to end. Deno renders from the self-contained SSR bundle and ASP.NET Core serves the
// published browser closure, including a configured public PathBase.
var options = VerificationOptions.Parse(args);
var repoRoot = RequireRepoRoot();
ConfigureRepositoryEnvironment(repoRoot);
var sourceSampleRoot = Path.Combine(repoRoot, "samples", "RazorVue.TodoList");
var packageRoot = ResolvePath(repoRoot, options.PackageSource);
var workRoot = Path.Combine(repoRoot, ".tmp", "windows-ssr-release-" + Environment.ProcessId);
var consumerRoot = Path.Combine(workRoot, "TodoList");
var publishRoot = Path.Combine(workRoot, "publish");
var restorePackagesRoot = Path.Combine(workRoot, "packages");
var dotnetCliHome = Path.Combine(repoRoot, ".dotnet");
var browserScriptPath = Path.Combine(consumerRoot, "verify-ssr-browser.mjs");
var hostStdoutLog = Path.Combine(workRoot, "todo-host.stdout.log");
var hostStderrLog = Path.Combine(workRoot, "todo-host.stderr.log");
var chromeStdoutLog = Path.Combine(workRoot, "todo-chrome.stdout.log");
var chromeStderrLog = Path.Combine(workRoot, "todo-chrome.stderr.log");
var nodeStdoutLog = Path.Combine(workRoot, "todo-node.stdout.log");
var nodeStderrLog = Path.Combine(workRoot, "todo-node.stderr.log");
var chromeUserDataRoot = Path.Combine(workRoot, "todo-chrome-profile");
var nodeExecutable = FindExecutableOnPath(OperatingSystem.IsWindows() ? "node.exe" : "node")
    ?? throw new FileNotFoundException("Node.js executable 'node' was not found on PATH.");
var chromeExecutable = ResolveChromeExecutable();

Console.WriteLine("Starting Windows SSR release consumer verification.");

Process? hostProcess = null;
Process? chromeProcess = null;
try
{
    DeleteDirectoryWithinRepo(repoRoot, workRoot);
    Directory.CreateDirectory(workRoot);

    if (options.PackPackages)
    {
        DeleteDirectoryWithinRepo(repoRoot, packageRoot);
        Directory.CreateDirectory(packageRoot);

        await RunDotNetAsync(
            [
                "run",
                "--file",
                Path.Combine("scripts", "csharp", "publish-nuget.cs"),
                "--",
                "--configuration",
                "Release",
                "--output-directory",
                packageRoot,
                "--skip-push"
            ],
            repoRoot,
            dotnetCliHome);
    }

    var packageVersion = PackageVerifier.ResolveSharedPackageVersion(packageRoot);
    PackageVerifier.VerifyRequiredPackages(packageRoot, packageVersion);

    // Copy the source tree first: the sample ships its own Directory.Build.props that only
    // forwards to the shared samples props, and that unconditional import breaks a detached
    // consumer. Overwriting it with the shared samples props last keeps the Wiki-gate shape.
    CopySampleSource(sourceSampleRoot, consumerRoot);
    CopyConsumerProps(repoRoot, consumerRoot);
    AssertDetachedConsumer(consumerRoot, workRoot);

    var consumerProject = Path.Combine(consumerRoot, "Todo.Host", "Todo.Host.csproj");
    await RunDotNetAsync(
        [
            "publish",
            consumerProject,
            "-c",
            "Release",
            "-o",
            publishRoot,
            "/m:1",
            "/p:BuildInParallel=false",
            "/nr:false",
            "-p:UseSharedCompilation=false",
            "-p:TodoUsePackages=true",
            "-p:JazorMode=release",
            "-p:JazorSSR=true",
            "-p:RestoreSources=" + packageRoot,
            "-p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json",
            "-p:RestorePackagesPath=" + restorePackagesRoot,
            "-p:RestoreForce=true",
            "-p:JazorPackageVersion=" + packageVersion
        ],
        consumerRoot,
        dotnetCliHome);

    SsrReleaseVerifier.VerifyPublishLayout(publishRoot);

    var rootUrl = "http://127.0.0.1:" + options.Port;
    hostProcess = StartProcess(
        "dotnet",
        ["Todo.Host.dll", "--urls", rootUrl],
        publishRoot,
        [
            new KeyValuePair<string, string?>("DOTNET_CLI_HOME", dotnetCliHome),
            new KeyValuePair<string, string?>("DOTNET_SKIP_FIRST_TIME_EXPERIENCE", "1"),
            new KeyValuePair<string, string?>("ASPNETCORE_ENVIRONMENT", "Production"),
            new KeyValuePair<string, string?>("DOTNET_ENVIRONMENT", "Production"),
            // The default Windows WebApplication host registers EventLog. The isolated
            // release consumer runs without permission to write the machine-wide
            // ".NET Runtime" source, so keep diagnostics on redirected process output.
            new KeyValuePair<string, string?>("Logging__EventLog__LogLevel__Default", "None"),
            new KeyValuePair<string, string?>("Todo__PathBase", options.PathBase),
            new KeyValuePair<string, string?>("Todo__Ssr", "true")
        ],
        hostStdoutLog,
        hostStderrLog);

    using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
    var pageUrl = rootUrl + GetExternalPath(options.PathBase, "/");
    var html = await WaitForSsrHtmlAsync(httpClient, pageUrl, hostProcess, TimeSpan.FromSeconds(options.StartupTimeoutSeconds), hostStdoutLog, hostStderrLog);
    SsrReleaseVerifier.VerifySsrDocument(html, options.PathBase);

    await VerifyDeploymentAssetsAsync(httpClient, rootUrl, options.PathBase, html);

    if (Directory.Exists(chromeUserDataRoot))
    {
        await DeleteDirectoryWithRetryAsync(chromeUserDataRoot);
    }

    chromeProcess = StartProcess(
        chromeExecutable,
        [
            "--headless=new",
            "--disable-gpu",
            // Match the other release/browser smoke launchers. Sandboxed Windows hosts can
            // terminate Chromium before its CDP endpoint is ready without these headless flags.
            "--disable-dev-shm-usage",
            "--no-first-run",
            "--no-default-browser-check",
            "--no-sandbox",
            "--remote-debugging-port=" + options.CdpPort,
            "--user-data-dir=" + chromeUserDataRoot,
            "about:blank"
        ],
        workRoot,
        [],
        chromeStdoutLog,
        chromeStderrLog);

    await WaitForCdpReadyAsync(options.CdpPort, chromeProcess, TimeSpan.FromSeconds(options.BrowserStartupTimeoutSeconds), chromeStdoutLog, chromeStderrLog);

    await RunProcessAsync(
        nodeExecutable,
        [browserScriptPath, rootUrl, options.CdpPort.ToString(), options.PathBase],
        workRoot,
        nodeStdoutLog,
        nodeStderrLog);

    Console.WriteLine("Windows SSR release consumer verification passed.");
}
catch
{
    Console.WriteLine("Windows SSR release consumer verification failed. Logs under: " + workRoot);
    throw;
}
finally
{
    if (chromeProcess is not null && !chromeProcess.HasExited)
    {
        chromeProcess.Kill(entireProcessTree: true);
        await chromeProcess.WaitForExitAsync();
    }

    if (hostProcess is not null && !hostProcess.HasExited)
    {
        hostProcess.Kill(entireProcessTree: true);
        await hostProcess.WaitForExitAsync();
    }

    if (!options.KeepWorkRoot && Directory.Exists(workRoot))
    {
        await DeleteDirectoryWithRetryAsync(workRoot);
    }
}

string GetExternalPath(string pathBase, string logicalPath)
{
    if (string.IsNullOrEmpty(pathBase))
    {
        return logicalPath;
    }

    return logicalPath == "/" ? pathBase + "/" : pathBase + logicalPath;
}

async Task<string> WaitForSsrHtmlAsync(
    HttpClient httpClient,
    string pageUrl,
    Process hostProcess,
    TimeSpan timeout,
    string stdoutLog,
    string stderrLog)
{
    var deadline = DateTime.UtcNow + timeout;
    while (DateTime.UtcNow < deadline)
    {
        if (hostProcess.HasExited)
        {
            throw new InvalidOperationException(
                "Todo.Host exited before serving SSR HTML. Stdout: " + ReadLogHead(stdoutLog) + " Stderr: " + ReadLogHead(stderrLog));
        }

        try
        {
            using var response = await httpClient.GetAsync(pageUrl);
            if (response.IsSuccessStatusCode)
            {
                var html = await response.Content.ReadAsStringAsync();
                if (html.Contains("data-todo-template", StringComparison.Ordinal))
                {
                    return html;
                }
            }
        }
        catch
        {
        }

        await Task.Delay(250);
    }

    throw new TimeoutException("Timed out waiting for SSR HTML from " + pageUrl + ". Stdout: " + ReadLogHead(stdoutLog) + " Stderr: " + ReadLogHead(stderrLog));
}

async Task VerifyDeploymentAssetsAsync(HttpClient httpClient, string rootUrl, string pathBase, string html)
{
    if (html.Contains("type=\"importmap\"", StringComparison.Ordinal))
        throw new InvalidOperationException("The standard project must hydrate without a generated import map.");
    await RequireAssetAsync(httpClient, rootUrl, pathBase + "/jazor/dist/hydration.js", "release hydration bundle");
    await RequireAssetAsync(httpClient, rootUrl, pathBase + "/jazor/dist/bundle.js", "release browser bundle");

    // A missing SSR module must stay a real 404; letting the SPA fallback answer with HTML
    // would hide broken hydration imports as a silent client-only page.
    using var missing = await httpClient.GetAsync(rootUrl + pathBase + "/jazor/missing-ssr-module.mjs");
    if (missing.StatusCode != System.Net.HttpStatusCode.NotFound)
    {
        throw new InvalidOperationException("Missing SSR module returned HTTP " + (int)missing.StatusCode + " instead of 404.");
    }
}

async Task RequireAssetAsync(HttpClient httpClient, string rootUrl, string pathAndQuery, string description)
{
    using var response = await httpClient.GetAsync(rootUrl + pathAndQuery);
    if (!response.IsSuccessStatusCode)
    {
        throw new InvalidOperationException("Failed to fetch " + description + " (HTTP " + (int)response.StatusCode + "): " + pathAndQuery);
    }

    var mediaType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
    if (mediaType.Equals("text/html", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException(description + " was served as HTML instead of its asset content type: " + pathAndQuery);
    }
}

Process StartProcess(
    string fileName,
    IReadOnlyList<string> arguments,
    string workdir,
    IReadOnlyList<KeyValuePair<string, string?>> environment,
    string stdoutLogPath,
    string stderrLogPath)
{
    var startInfo = new ProcessStartInfo
    {
        FileName = fileName,
        WorkingDirectory = workdir,
        UseShellExecute = false,
        CreateNoWindow = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true
    };

    foreach (var argument in arguments)
    {
        startInfo.ArgumentList.Add(argument);
    }

    foreach (var pair in environment)
    {
        startInfo.Environment[pair.Key] = pair.Value;
    }

    var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start " + fileName + ".");
    RedirectOutput(process.StandardOutput, stdoutLogPath);
    RedirectOutput(process.StandardError, stderrLogPath);
    return process;
}

void RedirectOutput(StreamReader reader, string logPath)
{
    _ = Task.Run(async () =>
    {
        await using var writer = new StreamWriter(logPath, append: false) { AutoFlush = true };
        while (true)
        {
            var line = await reader.ReadLineAsync();
            if (line is null)
            {
                break;
            }

            await writer.WriteLineAsync(line);
        }
    });
}

async Task RunProcessAsync(
    string fileName,
    IReadOnlyList<string> arguments,
    string workdir,
    string stdoutLogPath,
    string stderrLogPath)
{
    var process = StartProcess(fileName, arguments, workdir, [], stdoutLogPath, stderrLogPath);
    await process.WaitForExitAsync();
    if (process.ExitCode != 0)
    {
        throw new InvalidOperationException(
            "Process failed with exit code " + process.ExitCode + ": " + fileName + " " + string.Join(' ', arguments) +
            " Stdout: " + ReadLogHead(stdoutLogPath) + " Stderr: " + ReadLogHead(stderrLogPath));
    }
}

async Task RunDotNetAsync(IReadOnlyList<string> arguments, string workdir, string dotnetCliHome)
{
    var startInfo = new ProcessStartInfo
    {
        FileName = "dotnet",
        WorkingDirectory = workdir,
        UseShellExecute = false,
        CreateNoWindow = true
    };

    foreach (var argument in arguments)
    {
        startInfo.ArgumentList.Add(argument);
    }

    startInfo.Environment["DOTNET_CLI_HOME"] = dotnetCliHome;
    startInfo.Environment["TEMP"] = TempDirectory(workdir);
    startInfo.Environment["TMP"] = TempDirectory(workdir);
    startInfo.Environment["NUGET_PACKAGES"] = NuGetPackages(workdir);
    startInfo.Environment["NUGET_HTTP_CACHE_PATH"] = NuGetHttpCache(workdir);
    startInfo.Environment["DENO_DIR"] = DenoCache(workdir);
    startInfo.Environment["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1";
    startInfo.Environment["MSBUILDDISABLENODEREUSE"] = "1";
    startInfo.Environment["UseSharedCompilation"] = "false";

    using var process = Process.Start(startInfo)
        ?? throw new InvalidOperationException("Failed to start dotnet.");
    await process.WaitForExitAsync();
    if (process.ExitCode != 0)
    {
        throw new InvalidOperationException("Process failed with exit code " + process.ExitCode + ": dotnet " + string.Join(' ', arguments));
    }
}

void ConfigureRepositoryEnvironment(string repoRoot)
{
    foreach (var directory in new[]
    {
        TempDirectory(repoRoot),
        NuGetPackages(repoRoot),
        NuGetHttpCache(repoRoot),
        DenoCache(repoRoot)
    })
    {
        Directory.CreateDirectory(directory);
    }

    Environment.SetEnvironmentVariable("TEMP", TempDirectory(repoRoot));
    Environment.SetEnvironmentVariable("TMP", TempDirectory(repoRoot));
    Environment.SetEnvironmentVariable("DOTNET_CLI_HOME", Path.Combine(repoRoot, ".dotnet"));
    Environment.SetEnvironmentVariable("NUGET_PACKAGES", NuGetPackages(repoRoot));
    Environment.SetEnvironmentVariable("NUGET_HTTP_CACHE_PATH", NuGetHttpCache(repoRoot));
    Environment.SetEnvironmentVariable("DENO_DIR", DenoCache(repoRoot));
}

string TempDirectory(string repoRoot) => Path.Combine(repoRoot, ".tmp", "agent-temp");
string NuGetPackages(string repoRoot)
    => Path.Combine(repoRoot, ".dotnet", ".nuget", "packages") + Path.DirectorySeparatorChar;
string NuGetHttpCache(string repoRoot) => Path.Combine(repoRoot, ".tmp", "nuget-http-cache");
string DenoCache(string repoRoot) => Path.Combine(repoRoot, ".tmp", "deno-cache");

async Task WaitForCdpReadyAsync(int port, Process process, TimeSpan timeout, string stdoutLog, string stderrLog)
{
    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
    var deadline = DateTime.UtcNow + timeout;
    while (DateTime.UtcNow < deadline)
    {
        if (process.HasExited)
        {
            throw new InvalidOperationException("Browser process exited before the CDP endpoint became ready. Stdout: " + ReadLogHead(stdoutLog) + " Stderr: " + ReadLogHead(stderrLog));
        }

        try
        {
            using var response = await client.GetAsync("http://127.0.0.1:" + port + "/json/list");
            if (response.IsSuccessStatusCode)
            {
                var payload = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(payload) && payload.Trim() != "[]")
                {
                    return;
                }
            }
        }
        catch
        {
        }

        await Task.Delay(250);
    }

    throw new TimeoutException("Timed out waiting for chrome CDP endpoint on port " + port + ".");
}

void CopyConsumerProps(string repoRoot, string consumerRoot)
{
    var sourcePath = Path.Combine(repoRoot, "samples", "Directory.Build.props");
    if (!File.Exists(sourcePath))
    {
        throw new FileNotFoundException("Samples consumer props file was not found.", sourcePath);
    }

    Directory.CreateDirectory(consumerRoot);
    File.Copy(sourcePath, Path.Combine(consumerRoot, "Directory.Build.props"), overwrite: true);
}

void CopySampleSource(string sourceRoot, string targetRoot)
{
    if (!Directory.Exists(sourceRoot))
    {
        throw new DirectoryNotFoundException("TodoList source directory was not found: " + sourceRoot);
    }

    foreach (var directory in Directory.EnumerateDirectories(sourceRoot, "*", SearchOption.AllDirectories))
    {
        var relativePath = Path.GetRelativePath(sourceRoot, directory);
        if (IsExcluded(relativePath))
        {
            continue;
        }

        Directory.CreateDirectory(Path.Combine(targetRoot, relativePath));
    }

    foreach (var sourcePath in Directory.EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories))
    {
        var relativePath = Path.GetRelativePath(sourceRoot, sourcePath);
        if (IsExcluded(relativePath))
        {
            continue;
        }

        var targetPath = Path.Combine(targetRoot, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
        File.Copy(sourcePath, targetPath, overwrite: true);
    }
}

void AssertDetachedConsumer(string consumerRoot, string workRoot)
{
    var hostProject = Path.Combine(consumerRoot, "Todo.Host", "Todo.Host.csproj");
    var projectText = File.ReadAllText(hostProject);
    if (!projectText.Contains("TodoUsePackages", StringComparison.Ordinal) ||
        !projectText.Contains("PackageReference Include=\"Jazor\"", StringComparison.Ordinal))
    {
        throw new InvalidOperationException("Copied Todo.Host does not expose the package-consumer configuration.");
    }

    var libraryProject = Path.Combine(consumerRoot, "Todo.Library", "Todo.Library.csproj");
    var libraryText = File.ReadAllText(libraryProject);
    if (!libraryText.Contains("TodoUsePackages", StringComparison.Ordinal) ||
        !libraryText.Contains("PackageReference Include=\"Jazor\"", StringComparison.Ordinal) ||
        !libraryText.Contains("PackageReference Include=\"Jazor.Vue\"", StringComparison.Ordinal))
    {
        throw new InvalidOperationException(
            "Copied Todo.Library must directly reference both Jazor and Jazor.Vue in package-consumer mode.");
    }

    // Both projects express repository source references as ..\..\..\src; the consumer copy
    // places them one directory deeper than Wiki, so that path must resolve to .tmp/src.
    var resolvedRelativeSourceRoot = Path.GetFullPath(Path.Combine(workRoot, "..", "src"));
    if (Directory.Exists(resolvedRelativeSourceRoot))
    {
        throw new InvalidOperationException(
            "Consumer verification root resolves repository source references at '" + resolvedRelativeSourceRoot + "'.");
    }
}

void DeleteDirectoryWithinRepo(string repoRoot, string path)
{
    if (!Directory.Exists(path))
    {
        return;
    }

    var fullRoot = Path.GetFullPath(repoRoot);
    var fullPath = Path.GetFullPath(path);
    if (!fullPath.StartsWith(fullRoot + Path.DirectorySeparatorChar, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
    {
        throw new InvalidOperationException("Refusing to delete outside the repository: " + fullPath);
    }

    Directory.Delete(fullPath, recursive: true);
}

async Task DeleteDirectoryWithRetryAsync(string path, int attempts = 6)
{
    Exception? lastFailure = null;
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
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            lastFailure = ex;
            await Task.Delay(250);
        }
    }

    // Browser profile files can remain briefly locked after the child process exits. Cleanup
    // must not turn an otherwise successful consumer verification into a false failure; keep
    // the work root for inspection and report the lock explicitly.
    Console.WriteLine("Warning: could not remove temporary directory after " + attempts +
        " attempts; preserving it for diagnostics: " + path +
        (lastFailure is null ? string.Empty : " (" + lastFailure.Message + ")"));
}

string RequireRepoRoot()
{
    for (var directory = new DirectoryInfo(Directory.GetCurrentDirectory()); directory is not null; directory = directory.Parent)
    {
        if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
        {
            return directory.FullName;
        }
    }

    throw new InvalidOperationException("Repository root containing Jazor.slnx was not found.");
}

string ResolvePath(string repoRoot, string path)
    => Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path));

string? FindExecutableOnPath(string fileName)
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

string ResolveChromeExecutable()
{
    var candidates = OperatingSystem.IsWindows()
        ? new[]
        {
            @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
            @"C:\Program Files\Google\Chrome\Application\chrome.exe"
        }
        : Array.Empty<string>();

    foreach (var candidate in candidates)
    {
        if (File.Exists(candidate))
        {
            return candidate;
        }
    }

    throw new FileNotFoundException("Google Chrome executable was not found in the expected install locations.");
}

string ReadLogHead(string path, int maxChars = 2000)
{
    if (!File.Exists(path))
    {
        return string.Empty;
    }

    try
    {
        var text = File.ReadAllText(path);
        return text.Length <= maxChars ? text : text[..maxChars] + "…";
    }
    catch
    {
        return string.Empty;
    }
}

bool IsExcluded(string relativePath)
{
    var segments = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    return segments.Any(segment =>
        segment.Equals("bin", StringComparison.OrdinalIgnoreCase) ||
        segment.Equals("obj", StringComparison.OrdinalIgnoreCase) ||
        segment.Equals("jazor", StringComparison.OrdinalIgnoreCase) ||
        segment.Equals("consumer", StringComparison.OrdinalIgnoreCase) ||
        segment.Equals("node_modules", StringComparison.OrdinalIgnoreCase) ||
        segment.Equals("dist", StringComparison.OrdinalIgnoreCase) ||
        segment.Equals(".tmp", StringComparison.OrdinalIgnoreCase));
}

internal sealed record VerificationOptions(
    string PackageSource,
    bool PackPackages,
    string PathBase,
    int Port,
    int CdpPort,
    int StartupTimeoutSeconds,
    int BrowserStartupTimeoutSeconds,
    bool KeepWorkRoot)
{
    public static VerificationOptions Parse(IReadOnlyList<string> arguments)
    {
        var packageSource = Path.Combine("artifacts", "packages");
        var packPackages = true;
        var pathBase = "/todo";
        var port = 4297;
        var cdpPort = 9337;
        var startupTimeoutSeconds = 90;
        var browserStartupTimeoutSeconds = 20;
        var keepWorkRoot = false;

        for (var index = 0; index < arguments.Count; index++)
        {
            switch (arguments[index])
            {
                case "--package-source":
                    packageSource = RequireValue(arguments, ref index, "--package-source");
                    break;
                case "--skip-pack":
                    packPackages = false;
                    break;
                case "--path-base":
                    pathBase = RequireValue(arguments, ref index, "--path-base");
                    break;
                case "--port":
                    port = int.Parse(RequireValue(arguments, ref index, "--port"));
                    break;
                case "--cdp-port":
                    cdpPort = int.Parse(RequireValue(arguments, ref index, "--cdp-port"));
                    break;
                case "--startup-timeout-seconds":
                    startupTimeoutSeconds = int.Parse(RequireValue(arguments, ref index, "--startup-timeout-seconds"));
                    break;
                case "--browser-startup-timeout-seconds":
                    browserStartupTimeoutSeconds = int.Parse(RequireValue(arguments, ref index, "--browser-startup-timeout-seconds"));
                    break;
                case "--keep-work-root":
                    keepWorkRoot = true;
                    break;
                case "--help":
                case "-h":
                    WriteUsage();
                    Environment.Exit(0);
                    break;
                default:
                    throw new InvalidOperationException("Unsupported argument: " + arguments[index]);
            }
        }

        return new VerificationOptions(
            packageSource,
            packPackages,
            NormalizePathBase(pathBase),
            port,
            cdpPort,
            startupTimeoutSeconds,
            browserStartupTimeoutSeconds,
            keepWorkRoot);
    }

    private static string NormalizePathBase(string pathBase)
    {
        if (string.IsNullOrWhiteSpace(pathBase) || pathBase == "/")
        {
            return string.Empty;
        }

        if (!pathBase.StartsWith('/', StringComparison.Ordinal))
        {
            throw new InvalidOperationException("--path-base must start with '/'.");
        }

        return pathBase.EndsWith('/', StringComparison.Ordinal) && pathBase.Length > 1 ? pathBase[..^1] : pathBase;
    }

    private static string RequireValue(IReadOnlyList<string> arguments, ref int index, string option)
    {
        if (++index >= arguments.Count)
        {
            throw new InvalidOperationException("Missing value for " + option + ".");
        }

        return arguments[index];
    }

    private static void WriteUsage()
    {
        Console.WriteLine("Usage: dotnet run --file scripts/csharp/verify-windows-ssr-release.cs -- [options]");
        Console.WriteLine("Options:");
        Console.WriteLine("  --package-source <path>                 Default: artifacts/packages");
        Console.WriteLine("  --skip-pack                             Consume packages already in --package-source");
        Console.WriteLine("  --path-base </todo>                     Default: /todo");
        Console.WriteLine("  --port <number>                         Default: 4297");
        Console.WriteLine("  --cdp-port <number>                     Default: 9337");
        Console.WriteLine("  --startup-timeout-seconds <seconds>     Default: 90");
        Console.WriteLine("  --browser-startup-timeout-seconds <seconds> Default: 20");
        Console.WriteLine("  --keep-work-root");
    }
}

internal static class PackageVerifier
{
    // Keep this list aligned with publish-nuget.cs' DefaultPublicPackageIds. The
    // consumer gate must fail when a package is omitted from the release artifact,
    // even if the sample itself only exercises the three core references.
    private static readonly string[] RequiredPackageIds =
    [
        "Jazor",
        "Jazor.Vue",
        "ECMAScript.Style",
        "Jazor.Admin",
        "ECMAScript.Vue.Devtools",
        "ECMAScript.VueDataUi",
        "ECMAScript.Lucide",
        "ECMAScript.Pinia",
        "ECMAScript.Pinia.Testing",
        "ECMAScript.VueRoute",
        "ECMAScript.DateFns",
        "ECMAScript.VueUse",
        "ECMAScript.FloatingUi",
        "ECMAScript.VeeValidate",
        "ECMAScript.VueI18n",
        "ECMAScript.VueQuery",
        "ECMAScript.Monaco",
        "ECMAScript.VueDraggable",
        "ECMAScript.FilePond",
        "ECMAScript.WangEditor",
        "ECMAScript.Vuetify",
        "ECMAScript.ElementPlus",
        "ECMAScript.TDesign"
    ];

    public static string ResolveSharedPackageVersion(string packageRoot)
    {
        if (!Directory.Exists(packageRoot))
        {
            throw new DirectoryNotFoundException("Package source directory was not found: " + packageRoot);
        }

        var versions = RequiredPackageIds
            .Select(packageId => FindPackage(packageRoot, packageId))
            .Select(ReadPackageVersion)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return versions.Length == 1
            ? versions[0]
            : throw new InvalidOperationException("Required local packages do not have one shared version: " + string.Join(", ", versions));
    }

    public static void VerifyRequiredPackages(string packageRoot, string version)
    {
        foreach (var packageId in RequiredPackageIds)
        {
            var packageFile = FindPackage(packageRoot, packageId);
            var packageVersion = ReadPackageVersion(packageFile);
            if (!packageVersion.Equals(version, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Package '{packageId}' has version '{packageVersion}', expected '{version}'.");
            }

            using var archive = ZipFile.OpenRead(packageFile.FullName);
            if (packageId == "Jazor")
            {
                RequireEntry(archive, "build/Jazor.props", packageId);
                RequireEntry(archive, "build/Jazor.targets", packageId);
                RequireEntry(archive, "buildTransitive/Jazor.Resources.targets", packageId);
                RequireEntry(archive, "tools/net11.0/Jazor.Emit.dll", packageId);
                RequireEntry(archive, "lib/net11.0/Jazor.AspNetCore.dll", packageId);
                RequireEntry(archive, "lib/net11.0/Jazor.AspNetCore.Dev.dll", packageId);
            }
            else if (packageId == "Jazor.Vue")
            {
                RequireEntry(archive, "buildTransitive/Jazor.Vue.targets", packageId);
                RequireEntry(archive, "tools/net11.0/analyzers/Jazor.RazorVue.dll", packageId);
                RequireEntry(archive, "jazor/vue3/manifest.json", packageId);
            }
            else
            {
                RequireEntry(archive, "lib/net11.0/" + packageId + ".dll", packageId);
            }
        }
    }

    private static FileInfo FindPackage(string packageRoot, string packageId)
    {
        return new DirectoryInfo(packageRoot)
            .EnumerateFiles("*.nupkg", SearchOption.TopDirectoryOnly)
            .Where(static file => !file.Name.EndsWith(".snupkg", StringComparison.OrdinalIgnoreCase))
            .Where(file => ReadPackageId(file).Equals(packageId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(static file => file.LastWriteTimeUtc)
            .FirstOrDefault()
            ?? throw new InvalidOperationException("Required package was not found: " + packageId + " under " + packageRoot);
    }

    private static string ReadPackageId(FileInfo packageFile)
        => ReadNuspecMetadata(packageFile, "id");

    private static string ReadPackageVersion(FileInfo packageFile)
        => ReadNuspecMetadata(packageFile, "version");

    private static string ReadNuspecMetadata(FileInfo packageFile, string elementName)
    {
        using var archive = ZipFile.OpenRead(packageFile.FullName);
        var nuspecEntry = archive.Entries.SingleOrDefault(static entry => entry.FullName.EndsWith(".nuspec", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("Package does not contain a nuspec manifest: " + packageFile.FullName);
        using var stream = nuspecEntry.Open();
        var nuspec = XDocument.Load(stream);
        var ns = nuspec.Root?.Name.Namespace ?? XNamespace.None;
        return nuspec.Root?
            .Element(ns + "metadata")?
            .Element(ns + elementName)?
            .Value
            ?? throw new InvalidOperationException("Package metadata is missing '" + elementName + "': " + packageFile.FullName);
    }

    private static void RequireEntry(ZipArchive archive, string path, string packageId)
    {
        if (archive.GetEntry(path) is null)
        {
            throw new InvalidOperationException("Package '" + packageId + "' is missing '" + path + "'.");
        }
    }
}

internal static class SsrReleaseVerifier
{
    public static void VerifyPublishLayout(string publishRoot)
    {
        RequireFile(Path.Combine(publishRoot, "Todo.Host.dll"), "published Todo.Host");

        var jazorRoot = Path.Combine(publishRoot, "jazor");
        RequireFile(Path.Combine(jazorRoot, "dist", "bundle.js"), "release browser bundle");
        RequireFile(Path.Combine(jazorRoot, "dist", "hydration.js"), "release hydration entry");

        // Build state belongs in obj; runtime source and browser output share this project.
        foreach (var unexpectedPath in new[]
        {
            Path.Combine(jazorRoot, "jazor-manifest.json"),
            Path.Combine(jazorRoot, "importmap.json"),
            Path.Combine(jazorRoot, "ssr-importmap.json")
        })
        {
            if (File.Exists(unexpectedPath) || Directory.Exists(unexpectedPath))
            {
                throw new InvalidOperationException("SSR release publish retained a retired runtime manifest: " + unexpectedPath);
            }
        }

        var browserSources = string.Join("\n", Directory.EnumerateFiles(Path.Combine(jazorRoot, "dist"), "*.js", SearchOption.AllDirectories).Select(File.ReadAllText));
        RequireContains(browserSources, "todo-template-v1", "TodoApp template marker in release browser output");

        // Folder Publish carries only the runnable browser and SSR closures. The source
        // package graph and node_modules stay in the build workspace and are intentionally
        // absent from the deployment root.
        RequireFile(Path.Combine(jazorRoot, "ssr", "ssr-entry.js"), "bundled SSR entry");
        RequireFile(Path.Combine(jazorRoot, "ssr", "package.json"), "bundled SSR task package");
        if (File.Exists(Path.Combine(jazorRoot, "package.json")) ||
            File.Exists(Path.Combine(jazorRoot, "deno.lock")) ||
            Directory.Exists(Path.Combine(jazorRoot, "node_modules")))
        {
            throw new InvalidOperationException("SSR release publish retained build-time standard-project inputs under jazor/.");
        }
        if (Directory.EnumerateFiles(jazorRoot, "*.map", SearchOption.AllDirectories).Any())
        {
            throw new InvalidOperationException("SSR release publish included source maps without JazorPublishSourceMaps=true.");
        }
        if (browserSources.Contains("server-renderer", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Browser release bundle must not contain the SSR server-renderer entry.");
        }

        var ssrRoot = Path.Combine(jazorRoot, "ssr");
        var ssrBundle = string.Join("\n", Directory.EnumerateFiles(ssrRoot, "*.js", SearchOption.AllDirectories).Select(File.ReadAllText));
        RequireContains(ssrBundle, "components/todo-app.js", "SSR root component in bundled runtime");
        RequireContains(ssrBundle, "runSetParametersAsync", "ParameterView queue in bundled SSR runtime");
        RequireContains(ssrBundle, "onServerPrefetch", "SSR wait hook in bundled runtime");
        RequireContains(ssrBundle, "cascading", "cascading adapter in bundled runtime");
    }

    public static void VerifySsrDocument(string html, string pathBase)
    {
        // The DenoHost worker executed the real generated component: the initial task state
        // (2 open, 1 done, 3 total) can only appear when renderToString ran the lowered module.
        RequireContains(html, "<div id=\"app\">", "SSR mount element");
        RequireContains(html, "data-todo-template=\"todo-template-v1\"", "server-rendered TodoApp board");
        RequireContains(html, "data-todo-parameter=\"SSR ParameterView title\"", "server-applied ParameterView value");
        RequireContains(html, "data-todo-service=\"ssr-provider\"", "server-applied browser service provider");
        RequireContains(html, "jazor:auth-state", "serialized authentication provider key");
        RequireContains(html, "ssr-user", "serialized authentication subject");
        RequireContains(html, "data-todo-parameter-status=\"ready\"", "completed async ParameterView lifecycle");
        RequireContains(html, "data-todo-initialized=\"ready\"", "completed OnInitializedAsync lifecycle");
        RequireContains(html, "id=\"todo-cascade-card\"", "server-rendered cascading child");
        RequireContains(html, "data-todo-cascade=\"SSR ParameterView title\"", "server-applied cascading value");
        RequireContains(html, "id=\"todo-open-count\">2<", "server-rendered open count");
        RequireContains(html, "id=\"todo-done-count\">1<", "server-rendered done count");
        RequireContains(html, "id=\"todo-total-count\">3<", "server-rendered total count");
        // props 与 providers 自 P1 起合并进单一 `__jazor_ssr_state` v1 envelope
        // （JazorSsrStateEnvelope），旧的 `__jazor_ssr_props` / `__jazor_ssr_providers`
        // 双 script 标签模型已退役。这里断言 envelope 的真实序列化形状，避免门禁
        // 停留在已删除的旧协议上而误判发布产物。
        RequireContains(html, "<script id=\"__jazor_ssr_state\" type=\"application/json\">", "serialized SSR state envelope");
        RequireContains(html, "\"schema\":\"jazor-ssr-state\"", "SSR state envelope schema");
        RequireContains(html, "\"props\":{\"SsrTitle\":\"SSR ParameterView title\"}", "serialized SSR props");
        RequireContains(html, "\"providers\":[{\"key\":\"jazor:service:Todo.Library.TodoBrowserService\"", "serialized SSR providers");
        RequireContains(html, "jazor:service:Todo.Library.TodoBrowserService", "serialized browser service provider key");
        RequireContains(html, pathBase + "/jazor/dist/hydration.js", "release hydration bundle under the request path base");
        RequireContains(html, "components/todo-app.js", "hydration root component identity");
    }

    private static void RequireFile(string path, string description)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Missing " + description + ": " + path, path);
        }
    }

    private static void RequireContains(string text, string value, string description)
    {
        if (!text.Contains(value, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Missing " + description + ": " + value);
        }
    }

}
