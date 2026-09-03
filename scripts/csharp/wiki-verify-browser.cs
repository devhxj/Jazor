#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
var options = ScriptArguments.Parse(args);

var repoRoot = WikiScriptHelpers.RequireRepoRoot();
var sampleRoot = WikiScriptHelpers.ResolvePath(repoRoot, options.WikiRoot ?? Path.Combine("samples", "Wiki"));
var hostProject = Path.Combine(sampleRoot, "Wiki.csproj");
var publishRoot = options.PublishedRoot is null
    ? Path.Combine(repoRoot, ".tmp", "wiki-publish-browser-" + Environment.ProcessId)
    : WikiScriptHelpers.ResolvePath(repoRoot, options.PublishedRoot);
var browserScriptPath = Path.Combine(sampleRoot, "verify-browser.mjs");
var runnerLog = Path.Combine(sampleRoot, $".wiki-browser-runner-{Environment.ProcessId}.log");
var stdoutLog = Path.Combine(sampleRoot, $".wiki-browser-{Environment.ProcessId}.stdout.log");
var stderrLog = Path.Combine(sampleRoot, $".wiki-browser-{Environment.ProcessId}.stderr.log");
var chromeStdoutLog = Path.Combine(sampleRoot, $".wiki-browser-chrome-{Environment.ProcessId}.stdout.log");
var chromeStderrLog = Path.Combine(sampleRoot, $".wiki-browser-chrome-{Environment.ProcessId}.stderr.log");
var nodeStdoutLog = Path.Combine(sampleRoot, $".wiki-browser-node-{Environment.ProcessId}.stdout.log");
var nodeStderrLog = Path.Combine(sampleRoot, $".wiki-browser-node-{Environment.ProcessId}.stderr.log");
var chromeUserDataRoot = Path.Combine(sampleRoot, $".wiki-browser-chrome-profile-{Environment.ProcessId}");
var dotnetCliHome = Path.Combine(repoRoot, ".dotnet");
var chromeExecutable = WikiScriptHelpers.ResolveChromeExecutable();
var nodeExecutable = WikiScriptHelpers.FindNodeOnPath()
    ?? throw new FileNotFoundException("Node.js executable 'node' was not found on PATH.");

if (options.Publish && (options.Build || options.BuildLocal || options.PublishedRoot is not null))
{
    throw new InvalidOperationException("--publish already performs its own publish build. Do not combine it with --build, --build-local, or --published-root.");
}

if (options.PublishedRoot is not null && (options.Build || options.BuildLocal))
{
    throw new InvalidOperationException("--published-root uses an existing release publish. Do not combine it with --build or --build-local.");
}

var normalizedPathBase = WikiScriptHelpers.NormalizePathBase(options.PathBase);
var rootUrl = $"http://localhost:{options.Port}";
var healthUrl = rootUrl + WikiScriptHelpers.GetExternalPath(normalizedPathBase, "/health");
var effectiveConfiguration = options.Publish && !options.ConfigurationWasExplicit
    ? "Release"
    : options.Configuration;
var isPublished = options.Publish || options.PublishedRoot is not null;

string hostRoot = sampleRoot;
string jazorRoot = Path.Combine(sampleRoot, "jazor");

Trace("Starting wiki browser verification.");

try
{
    if (options.PublishedRoot is not null)
    {
        Trace("Using an existing Wiki release publish.");
        hostRoot = publishRoot;
        jazorRoot = Path.Combine(hostRoot, "jazor");
    }
    else if (options.Publish)
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
            "-p:UseSharedCompilation=false",
            // Wiki defaults to debug for local development; publishing must exercise Netpack's
            // release artifact contract even when the configuration is Release already.
            "-p:JazorMode=release"
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
        jazorRoot = Path.Combine(hostRoot, "jazor");
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

    if (isPublished && !Directory.Exists(jazorRoot))
    {
        throw new InvalidOperationException("Published Jazor artifacts were not copied to: " + jazorRoot);
    }

    if (isPublished)
    {
        AssertReleaseArtifacts(jazorRoot);
    }
    else
    {
        AssertDebugArtifacts(jazorRoot);
    }

    // 浏览器场景使用与 RouteContract 相同的生成目录，不把正文页路由、标题或相邻页写死在 Node 断言里。
    var verificationRoutes = ReadBrowserVerificationRoutes(Path.Combine(sampleRoot, "obj", "wiki", "WikiDocsContent.g.cs"));
    WikiScriptHelpers.EnsureFileExists(browserScriptPath, "browser verification script");

    Process? hostProcess = null;
    Process? chromeProcess = null;
    var keepLogs = false;
    try
    {
        Trace("Starting Wiki host process.");
        var hostArguments = isPublished
            ? new[] { "Wiki.dll", "--urls", rootUrl }
            : new[] { "run", "--project", hostProject, "--no-launch-profile", "-c", effectiveConfiguration, "--no-build", "--no-restore", "--urls", rootUrl };

        hostProcess = WikiScriptHelpers.StartProcess(
            fileName: "dotnet",
            arguments: hostArguments,
            workdir: isPublished ? hostRoot : sampleRoot,
            environment:
            [
                new KeyValuePair<string, string?>("DOTNET_CLI_HOME", dotnetCliHome),
                new KeyValuePair<string, string?>("DOTNET_SKIP_FIRST_TIME_EXPERIENCE", "1"),
                new KeyValuePair<string, string?>("ASPNETCORE_ENVIRONMENT", isPublished ? "Production" : "Development"),
                new KeyValuePair<string, string?>("DOTNET_ENVIRONMENT", isPublished ? "Production" : "Development"),
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

        if (isPublished)
        {
            // A missing generated module must stay an asset 404. Returning the SPA shell here
            // hides broken release imports and turns an observable deployment failure into HTML.
            // 缺失生成模块必须保持 asset 404；若回退为 SPA HTML，会掩盖 release import 的部署故障。
            using var missingModuleResponse = await WikiScriptHelpers.GetAsync(
                rootUrl + WikiScriptHelpers.GetExternalPath(normalizedPathBase, "/jazor/missing-release-module.mjs"));
            if (missingModuleResponse.StatusCode != HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException(
                    "Missing release module returned HTTP " + (int)missingModuleResponse.StatusCode + " instead of 404.");
            }

            var missingModuleContentType = missingModuleResponse.Content.Headers.ContentType?.MediaType ?? string.Empty;
            if (missingModuleContentType.Equals("text/html", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Missing release module incorrectly returned an HTML response.");
            }
        }

        if (Directory.Exists(chromeUserDataRoot))
        {
            Trace("Removing stale Chrome profile directory.");
            await WikiScriptHelpers.RemoveDirectoryWithRetryAsync(chromeUserDataRoot);
        }

        Trace("Starting Chrome headless browser.");
        chromeProcess = WikiScriptHelpers.StartProcess(
            fileName: chromeExecutable,
            arguments:
            [
                "--headless=new",
                "--disable-gpu",
                "--no-first-run",
                "--no-default-browser-check",
                "--remote-debugging-port=" + options.CdpPort,
                "--user-data-dir=" + chromeUserDataRoot,
                "about:blank"
            ],
            workdir: sampleRoot,
            stdoutLogPath: chromeStdoutLog,
            stderrLogPath: chromeStderrLog);
        Trace("Chrome process started.");

        await WikiScriptHelpers.WaitForCdpReadyAsync(
            options.CdpPort,
            chromeProcess,
            TimeSpan.FromSeconds(options.BrowserStartupTimeoutSeconds),
            failureContext: $"See logs: {chromeStdoutLog} ; {chromeStderrLog}");
        Trace("Chrome CDP endpoint is ready.");

        var verificationMode = isPublished ? "production" : "development";
        Trace("Starting browser verification script.");
        await WikiScriptHelpers.RunProcessAsync(
            fileName: nodeExecutable,
            arguments:
            [
                browserScriptPath,
                rootUrl,
                options.CdpPort.ToString(),
                verificationMode,
                normalizedPathBase,
                verificationRoutes.HomeTitle,
                verificationRoutes.HomeSummary,
                verificationRoutes.PrimaryPath,
                verificationRoutes.PrimaryTitle,
                verificationRoutes.RelatedPath
            ],
            workdir: sampleRoot,
            stdoutLogPath: nodeStdoutLog,
            stderrLogPath: nodeStderrLog);
        Trace("Browser verification script completed.");

        Console.WriteLine(isPublished
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

        if (!keepLogs)
        {
            foreach (var logPath in new[] { runnerLog, stdoutLog, stderrLog, chromeStdoutLog, chromeStderrLog, nodeStdoutLog, nodeStderrLog })
            {
                if (File.Exists(logPath))
                {
                    await WikiScriptHelpers.RemoveFileWithRetryAsync(logPath);
                }
            }

            if (Directory.Exists(chromeUserDataRoot))
            {
                await WikiScriptHelpers.RemoveDirectoryWithRetryAsync(chromeUserDataRoot);
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

void AssertDebugArtifacts(string artifactRoot)
{
    WikiScriptHelpers.EnsureFileExists(Path.Combine(artifactRoot, "main.mjs"), "emitted main module");
    WikiScriptHelpers.EnsureFileExists(Path.Combine(artifactRoot, "main.mjs.map"), "emitted main source map");
    WikiScriptHelpers.EnsureFileExists(Path.Combine(artifactRoot, "jazor-manifest.json"), "emit manifest");
    AssertImportMapTargetExists(artifactRoot, "System/StringModule.js");
    WikiScriptHelpers.EnsureFileExists(Path.Combine(artifactRoot, "components", "wiki-home.mjs"), "emitted wiki component module");
    WikiScriptHelpers.EnsureFileExists(Path.Combine(artifactRoot, "components", "wiki-home.mjs.map"), "emitted wiki component source map");
    WikiScriptHelpers.EnsureFileExists(Path.Combine(artifactRoot, "components", "wiki-styles.mjs"), "emitted Wiki CSS module");
}

void AssertReleaseArtifacts(string artifactRoot)
{
    WikiScriptHelpers.EnsureFileExists(Path.Combine(artifactRoot, "bundle.js"), "production browser bundle");
    WikiScriptHelpers.EnsureFileExists(Path.Combine(artifactRoot, "bundle.js.map"), "production browser bundle source map");

    // Netpack may retain its entry helper, but the inspectable debug graph must not leak into
    // a non-SSR release publish. That catches a configuration-only release build by mistake.
    // Netpack 可以保留入口辅助文件，但非 SSR 的 release publish 不能泄漏可调试模块图。
    foreach (var unexpectedPath in new[]
    {
        Path.Combine(artifactRoot, "main.mjs"),
        Path.Combine(artifactRoot, "jazor-manifest.json"),
        Path.Combine(artifactRoot, "importmap.json"),
        Path.Combine(artifactRoot, "ssr-importmap.json"),
        Path.Combine(artifactRoot, "manifest.json"),
        Path.Combine(artifactRoot, "style.mjs"),
        Path.Combine(artifactRoot, "components")
    })
    {
        if (File.Exists(unexpectedPath) || Directory.Exists(unexpectedPath))
        {
            throw new InvalidOperationException("Release publish unexpectedly retained debug artifact: " + unexpectedPath);
        }
    }
}

void AssertImportMapTargetExists(string artifactRoot, string specifier)
{
    var importMapPath = Path.Combine(artifactRoot, "importmap.json");
    WikiScriptHelpers.EnsureFileExists(importMapPath, "browser import map");

    using var document = JsonDocument.Parse(File.ReadAllText(importMapPath, Encoding.UTF8));
    if (!document.RootElement.TryGetProperty("imports", out var imports) ||
        imports.ValueKind != JsonValueKind.Object ||
        !imports.TryGetProperty(specifier, out var targetElement) ||
        targetElement.ValueKind != JsonValueKind.String ||
        targetElement.GetString() is not { } target)
    {
        throw new InvalidOperationException("Browser import map is missing string entry '" + specifier + "'.");
    }

    const string artifactPrefix = "/jazor/";
    if (!target.StartsWith(artifactPrefix, StringComparison.Ordinal))
        throw new InvalidOperationException("Browser import target for '" + specifier + "' is not a Jazor artifact URL: " + target);

    var relativePath = target[artifactPrefix.Length..].Replace('/', Path.DirectorySeparatorChar);
    WikiScriptHelpers.EnsureFileExists(
        Path.Combine(artifactRoot, relativePath),
        "materialized browser import target for " + specifier);
}

BrowserVerificationRoutes ReadBrowserVerificationRoutes(string generatedCatalogPath)
{
    WikiScriptHelpers.EnsureFileExists(generatedCatalogPath, "generated Wiki docs catalog");
    var catalog = File.ReadAllText(generatedCatalogPath, Encoding.UTF8);
    var paths = ReadGeneratedStringArray(catalog, "PagePaths");
    var groups = ReadGeneratedStringArray(catalog, "PageGroups");
    var titles = ReadGeneratedStringArray(catalog, "PageTitles");
    var summaries = ReadGeneratedStringArray(catalog, "PageSummaries");

    if (paths.Count != groups.Count || paths.Count != titles.Count || paths.Count != summaries.Count)
    {
        throw new InvalidOperationException("WikiDocsContent route metadata arrays do not have matching lengths.");
    }

    var homeIndex = paths.FindIndex(path => path == "/");
    var primaryIndex = paths.FindIndex(path => path == "/guides/quick-start");
    if (homeIndex < 0 || primaryIndex < 0 || groups[primaryIndex] != "Guides")
    {
        throw new InvalidOperationException("WikiDocsContent must contain the root page and the Guides quick-start browser fixture.");
    }

    // importer 的 RelatedPaths 契约是同组相邻页面；从排序后的目录推导相邻页，文档新增时自动保持正确。
    var relatedIndex = -1;
    for (var index = primaryIndex - 1; index >= 0; index--)
    {
        if (groups[index] == groups[primaryIndex] && paths[index] != "/search")
        {
            relatedIndex = index;
            break;
        }
    }

    if (relatedIndex < 0)
    {
        for (var index = primaryIndex + 1; index < paths.Count; index++)
        {
            if (groups[index] == groups[primaryIndex] && paths[index] != "/search")
            {
                relatedIndex = index;
                break;
            }
        }
    }

    if (relatedIndex < 0 || titles[homeIndex].Length == 0 || summaries[homeIndex].Length == 0 || titles[primaryIndex].Length == 0)
    {
        throw new InvalidOperationException("WikiDocsContent does not contain enough metadata for the browser verification fixture.");
    }

    return new BrowserVerificationRoutes(
        titles[homeIndex] + " | jazor.wiki",
        summaries[homeIndex],
        paths[primaryIndex],
        titles[primaryIndex],
        paths[relatedIndex]);
}

List<string> ReadGeneratedStringArray(string catalog, string arrayName)
{
    var declaration = "internal static readonly string[] " + arrayName;
    var declarationIndex = catalog.IndexOf(declaration, StringComparison.Ordinal);
    if (declarationIndex < 0)
    {
        throw new InvalidOperationException("WikiDocsContent is missing " + arrayName + ".");
    }

    var assignmentIndex = catalog.IndexOf('=', declarationIndex);
    var position = assignmentIndex < 0 ? -1 : catalog.IndexOf('[', assignmentIndex + 1);
    if (position < 0)
    {
        throw new InvalidOperationException("WikiDocsContent has no array initializer for " + arrayName + ".");
    }

    position++;
    var values = new List<string>();
    while (true)
    {
        SkipGeneratedWhitespace(catalog, ref position);
        if (position >= catalog.Length)
            throw new InvalidOperationException("WikiDocsContent array " + arrayName + " is not terminated.");

        if (catalog[position] == ']')
            return values;

        if (catalog[position] != '"')
            throw new InvalidOperationException("Unexpected token in WikiDocsContent array " + arrayName + ".");

        var valueStart = ++position;
        while (position < catalog.Length && catalog[position] != '"')
        {
            if (catalog[position] == '\\')
                position++;
            position++;
        }

        if (position >= catalog.Length)
            throw new InvalidOperationException("Unterminated string in WikiDocsContent array " + arrayName + ".");

        var escapedValue = catalog.Substring(valueStart, position - valueStart);
        values.Add(Regex.Unescape(escapedValue));
        position++;

        SkipGeneratedWhitespace(catalog, ref position);
        if (position < catalog.Length && catalog[position] == ',')
            position++;
    }
}

void SkipGeneratedWhitespace(string text, ref int position)
{
    while (position < text.Length && char.IsWhiteSpace(text[position]))
        position++;
}

internal sealed record BrowserVerificationRoutes(
    string HomeTitle,
    string HomeSummary,
    string PrimaryPath,
    string PrimaryTitle,
    string RelatedPath);

internal sealed record ScriptArguments
{
    public int Port { get; init; } = 4196;

    public int CdpPort { get; init; } = 9236;

    public string Configuration { get; init; } = "Debug";

    public bool ConfigurationWasExplicit { get; init; }

    public string? BaseOutputPath { get; init; }

    public string? BaseIntermediateOutputPath { get; init; }

    public string? PathBase { get; init; }

    public string? WikiRoot { get; init; }

    public string? PublishedRoot { get; init; }

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
                case "--wiki-root":
                    result = result with { WikiRoot = GetValue(args, ref index, argument) };
                    break;
                case "--published-root":
                    result = result with { PublishedRoot = GetValue(args, ref index, argument) };
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
        Console.WriteLine("  --wiki-root <path>");
        Console.WriteLine("  --published-root <path>");
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

    public static string ResolvePath(string repoRoot, string path)
        => Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path));

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

    public static async Task<HttpResponseMessage> GetAsync(string url, CancellationToken cancellationToken = default)
    {
        using var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

        return await client.GetAsync(url, cancellationToken);
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
            "Timed out waiting for Chrome CDP endpoint on port " + port + "." + FormatFailureContext(failureContext));
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

    public static string ResolveChromeExecutable()
    {
        var candidates = OperatingSystem.IsWindows()
            ? new[]
            {
                @"C:\Program Files\Google\Chrome\Application\chrome.exe",
                @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "Application", "chrome.exe")
            }
            : OperatingSystem.IsMacOS()
                ? new[] { "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome" }
                : Array.Empty<string>();

        foreach (var candidate in candidates)
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        var pathExecutable = OperatingSystem.IsWindows()
            ? FindExecutableOnPath("chrome.exe")
            : FindExecutableOnPath("google-chrome") ?? FindExecutableOnPath("google-chrome-stable") ?? FindExecutableOnPath("chromium");
        if (pathExecutable is not null)
        {
            return pathExecutable;
        }

        throw new FileNotFoundException("Google Chrome executable was not found in the expected install locations or PATH.");
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
