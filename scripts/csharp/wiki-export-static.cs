#!/usr/bin/env dotnet run

// Wiki 官方站静态导出器。
// 先通过真实 ASP.NET Core host 生成首屏 HTML，再将每个 clean URL 物化为目录/index.html，
// 因而 GitHub Pages 不需要理解 ASP.NET fallback 或自定义响应头。

using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

var options = ExportOptions.Parse(args);
var repoRoot = RequireRepoRoot();
var sampleRoot = Path.Combine(repoRoot, "samples", "Wiki");
var projectPath = Path.Combine(sampleRoot, "Wiki.csproj");
var generatedCatalog = Path.Combine(sampleRoot, "obj", "wiki", "WikiDocsContent.g.cs");
var publishRoot = Path.Combine(repoRoot, ".tmp", "wiki-static-publish-" + Environment.ProcessId);
var outputRoot = ResolveRepoPath(repoRoot, options.Output);
var hostUrl = "http://127.0.0.1:" + options.Port;
var normalizedPathBase = NormalizePathBase(options.PathBase);
var siteOrigin = options.SiteOrigin.TrimEnd('/');
var dotnetCliHome = Path.Combine(repoRoot, ".dotnet");
var stdoutLog = Path.Combine(repoRoot, ".tmp", "wiki-export-" + Environment.ProcessId + ".stdout.log");
var stderrLog = Path.Combine(repoRoot, ".tmp", "wiki-export-" + Environment.ProcessId + ".stderr.log");
Process? hostProcess = null;

try
{
    // 路由枚举来自生成目录；每次导出先刷新，避免读取上一次构建留下的 obj 数据。
    await RunProcessAsync(
        "dotnet",
        ["run", "--file", Path.Combine("scripts", "csharp", "wiki-import-docs.cs")],
        repoRoot,
        dotnetCliHome);

    var routes = ReadRegisteredRoutes(generatedCatalog);
    if (routes.Count == 0 || routes[0] != "/")
        throw new InvalidOperationException("WikiDocsContent does not contain the root route.");

    EnsureDirectoryDeletedWithinRepo(repoRoot, publishRoot);
    Directory.CreateDirectory(Path.GetDirectoryName(stdoutLog)!);
    await RunProcessAsync(
        "dotnet",
        [
            "publish", projectPath, "-c", "Release", "-o", publishRoot,
            "/m:1", "/p:BuildInParallel=false", "/nr:false",
            "-p:UseSharedCompilation=false", "-p:JazorMode=release"
        ],
        repoRoot,
        dotnetCliHome);

    if (!Directory.Exists(Path.Combine(publishRoot, "jazor")))
        throw new InvalidOperationException("Release publish did not contain a jazor directory.");

    EnsureDirectoryDeletedWithinRepo(repoRoot, outputRoot);
    Directory.CreateDirectory(outputRoot);
    CopyDirectory(Path.Combine(publishRoot, "wwwroot"), outputRoot);
    CopyDirectory(Path.Combine(publishRoot, "jazor"), Path.Combine(outputRoot, "jazor"));
    File.WriteAllText(Path.Combine(outputRoot, ".nojekyll"), "GitHub Pages static export\n", new UTF8Encoding(false));

    hostProcess = StartProcess(
        "dotnet",
        ["Wiki.dll", "--urls", hostUrl],
        publishRoot,
        new Dictionary<string, string?>
        {
            ["DOTNET_CLI_HOME"] = dotnetCliHome,
            ["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1",
            ["ASPNETCORE_ENVIRONMENT"] = "Production",
            ["DOTNET_ENVIRONMENT"] = "Production",
            ["Wiki__PathBase"] = normalizedPathBase,
            ["Wiki__SiteOrigin"] = siteOrigin
        },
        stdoutLog,
        stderrLog);

    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
    await WaitForHealthAsync(client, hostProcess, hostUrl + ExternalPath(normalizedPathBase, "/health"));

    var exportedRoutes = routes.Concat(["/search"]).Distinct(StringComparer.Ordinal).ToArray();
    foreach (var route in exportedRoutes)
    {
        using var response = await GetAsync(client, hostProcess, hostUrl + ExternalPath(normalizedPathBase, route));
        if (response.StatusCode != HttpStatusCode.OK)
            throw new InvalidOperationException($"Route {route} returned {(int)response.StatusCode}, expected 200.");

        var html = await response.Content.ReadAsStringAsync();
        ValidateHtml(route, html);
        WriteRouteHtml(outputRoot, route, html);
    }

    using (var robots = await GetAsync(client, hostProcess, hostUrl + ExternalPath(normalizedPathBase, "/robots.txt")))
    {
        EnsureStatus(robots, HttpStatusCode.OK, "/robots.txt");
        var text = await robots.Content.ReadAsStringAsync();
        ValidateDiscovery(text, siteOrigin, normalizedPathBase, "robots.txt");
        File.WriteAllText(Path.Combine(outputRoot, "robots.txt"), text, new UTF8Encoding(false));
    }

    using (var sitemap = await GetAsync(client, hostProcess, hostUrl + ExternalPath(normalizedPathBase, "/sitemap.xml")))
    {
        EnsureStatus(sitemap, HttpStatusCode.OK, "/sitemap.xml");
        var text = await sitemap.Content.ReadAsStringAsync();
        ValidateDiscovery(text, siteOrigin, normalizedPathBase, "sitemap.xml");
        File.WriteAllText(Path.Combine(outputRoot, "sitemap.xml"), text, new UTF8Encoding(false));
    }

    const string missingRoute = "/__wiki-static-export-not-found__";
    using (var missing = await GetAsync(client, hostProcess, hostUrl + ExternalPath(normalizedPathBase, missingRoute), allowNonSuccess: true))
    {
        EnsureStatus(missing, HttpStatusCode.NotFound, missingRoute);
        var html = await missing.Content.ReadAsStringAsync();
        ValidateHtml(missingRoute, html);
        WriteRouteHtml(outputRoot, "/404", html, fileName: "404.html");
    }

    ValidateOutput(outputRoot, routes, siteOrigin, normalizedPathBase);
    Console.WriteLine("Wiki static export completed: " + outputRoot);
    Console.WriteLine("Exported routes: " + exportedRoutes.Length + "; output files: " + Directory.EnumerateFiles(outputRoot, "*", SearchOption.AllDirectories).Count());

    if (options.Serve)
        await ServeAsync(outputRoot, options.ServePort, normalizedPathBase);
}
finally
{
    if (hostProcess is not null && !hostProcess.HasExited)
    {
        hostProcess.Kill(entireProcessTree: true);
        await hostProcess.WaitForExitAsync();
    }

    if (!options.KeepPublish)
        RemoveDirectoryWithRetry(publishRoot);

    if (!options.KeepLogs)
    {
        RemoveFileWithRetry(stdoutLog);
        RemoveFileWithRetry(stderrLog);
    }
}

static string RequireRepoRoot()
{
    var directory = new DirectoryInfo(Environment.CurrentDirectory);
    while (directory is not null)
    {
        if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
            return directory.FullName;
        directory = directory.Parent;
    }

    throw new InvalidOperationException("Repository root containing Jazor.slnx was not found.");
}

static string ResolveRepoPath(string repoRoot, string path)
{
    var fullPath = Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path));
    var root = Path.GetFullPath(repoRoot).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
    if (!fullPath.StartsWith(root, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
        throw new InvalidOperationException("Static output must stay under the repository: " + fullPath);
    return fullPath;
}

static string NormalizePathBase(string? value)
{
    if (string.IsNullOrWhiteSpace(value) || value == "/")
        return "";
    if (!value.StartsWith('/'))
        throw new InvalidOperationException("--path-base must start with '/'.");
    return value.Length > 1 ? value.TrimEnd('/') : value;
}

static string ExternalPath(string pathBase, string logicalPath)
    => string.IsNullOrEmpty(pathBase) ? logicalPath : logicalPath == "/" ? pathBase + "/" : pathBase + logicalPath;

static List<string> ReadRegisteredRoutes(string generatedCatalog)
{
    var text = File.ReadAllText(generatedCatalog);
    var match = Regex.Match(text, @"PagePaths\s*=\s*\[(?<body>.*?)\];", RegexOptions.Singleline);
    if (!match.Success)
        throw new InvalidOperationException("Could not locate PagePaths in " + generatedCatalog);

    return Regex.Matches(match.Groups["body"].Value, "\\\"(?<path>/[^\\\"]*)\\\"")
        .Select(item => item.Groups["path"].Value)
        .Distinct(StringComparer.Ordinal)
        .ToList();
}

static async Task RunProcessAsync(string fileName, IReadOnlyList<string> arguments, string workdir, string dotnetCliHome)
{
    using var process = StartProcess(fileName, arguments, workdir,
        new Dictionary<string, string?>
        {
            ["DOTNET_CLI_HOME"] = dotnetCliHome,
            ["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1"
        });
    await process.WaitForExitAsync();
    if (process.ExitCode != 0)
        throw new InvalidOperationException($"Process failed with exit code {process.ExitCode}: {fileName} {string.Join(' ', arguments)}");
}

static Process StartProcess(
    string fileName,
    IReadOnlyList<string> arguments,
    string workdir,
    IReadOnlyDictionary<string, string?>? environment = null,
    string? stdoutLog = null,
    string? stderrLog = null)
{
    var info = new ProcessStartInfo
    {
        FileName = fileName,
        WorkingDirectory = workdir,
        UseShellExecute = false,
        CreateNoWindow = true,
        RedirectStandardOutput = stdoutLog is not null,
        RedirectStandardError = stderrLog is not null
    };
    foreach (var argument in arguments)
        info.ArgumentList.Add(argument);
    if (environment is not null)
    {
        foreach (var item in environment)
            info.Environment[item.Key] = item.Value;
    }

    var process = Process.Start(info) ?? throw new InvalidOperationException("Could not start " + fileName);
    if (stdoutLog is not null)
        _ = RedirectAsync(process.StandardOutput, stdoutLog);
    if (stderrLog is not null)
        _ = RedirectAsync(process.StandardError, stderrLog);
    return process;
}

static async Task RedirectAsync(StreamReader reader, string path)
{
    await using var writer = new StreamWriter(path, false, Encoding.UTF8);
    while (await reader.ReadLineAsync() is { } line)
        await writer.WriteLineAsync(line);
}

static async Task WaitForHealthAsync(HttpClient client, Process process, string url)
{
    var deadline = DateTime.UtcNow.AddSeconds(60);
    while (DateTime.UtcNow < deadline)
    {
        if (process.HasExited)
            throw new InvalidOperationException("Wiki publish host exited before health check.");
        try
        {
            using var response = await client.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.OK)
                return;
        }
        catch (HttpRequestException)
        {
        }
        await Task.Delay(400);
    }
    throw new TimeoutException("Timed out waiting for Wiki publish host: " + url);
}

static async Task<HttpResponseMessage> GetAsync(HttpClient client, Process process, string url, bool allowNonSuccess = false)
{
    if (process.HasExited)
        throw new InvalidOperationException("Wiki publish host exited while requesting " + url);
    var response = await client.GetAsync(url);
    if (!allowNonSuccess && !response.IsSuccessStatusCode)
        throw new InvalidOperationException("Request failed: " + url + " -> " + (int)response.StatusCode);
    return response;
}

static void EnsureStatus(HttpResponseMessage response, HttpStatusCode expected, string path)
{
    if (response.StatusCode != expected)
        throw new InvalidOperationException(path + " returned " + (int)response.StatusCode + ", expected " + (int)expected + ".");
}

static void ValidateHtml(string route, string html)
{
    if (string.IsNullOrWhiteSpace(html))
        throw new InvalidOperationException("Empty HTML for route " + route + ".");
    foreach (var forbidden in new[] { "localhost", "127.0.0.1", "__WIKI_" })
    {
        if (html.Contains(forbidden, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Static HTML for " + route + " contains forbidden token " + forbidden + ".");
    }
}

static void ValidateDiscovery(string text, string siteOrigin, string pathBase, string name)
{
    if (string.IsNullOrWhiteSpace(text) || text.Contains("localhost", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Invalid " + name + " content.");
    if (!text.Contains(siteOrigin + pathBase, StringComparison.Ordinal))
        throw new InvalidOperationException(name + " does not use configured SiteOrigin/PathBase.");
}

static void WriteRouteHtml(string outputRoot, string route, string html, string? fileName = null)
{
    var relative = fileName ?? (route == "/" ? "index.html" : route.Trim('/').Replace('/', Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar + "index.html");
    var path = Path.Combine(outputRoot, relative);
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    File.WriteAllText(path, html, new UTF8Encoding(false));
}

static void ValidateOutput(string outputRoot, IReadOnlyList<string> routes, string siteOrigin, string pathBase)
{
    foreach (var route in routes.Concat(["/search"]))
    {
        var path = route == "/"
            ? Path.Combine(outputRoot, "index.html")
            : Path.Combine(outputRoot, route.Trim('/'), "index.html");
        if (!File.Exists(path) || new FileInfo(path).Length == 0)
            throw new InvalidOperationException("Missing static route file for " + route + ": " + path);
    }

    foreach (var file in Directory.EnumerateFiles(outputRoot, "*", SearchOption.AllDirectories))
    {
        if (new FileInfo(file).Length == 0 && !Path.GetFileName(file).Equals(".nojekyll", StringComparison.Ordinal))
            throw new InvalidOperationException("Static output contains an empty file: " + file);
        if (Path.GetExtension(file).Equals(".html", StringComparison.OrdinalIgnoreCase))
        {
            var content = File.ReadAllText(file);
            ValidateHtml(file, content);
            if (!content.Contains(siteOrigin + pathBase, StringComparison.Ordinal))
                throw new InvalidOperationException("HTML file does not contain configured site origin: " + file);
        }
    }

    foreach (var required in new[] { "robots.txt", "sitemap.xml", "404.html", "site.css", "favicon.svg", "jazor/bundle.js", "jazor/bundle.js.map" })
        if (!File.Exists(Path.Combine(outputRoot, required)))
            throw new InvalidOperationException("Static output is missing " + required + ".");
}

static void CopyDirectory(string source, string destination)
{
    if (!Directory.Exists(source))
        throw new DirectoryNotFoundException(source);
    Directory.CreateDirectory(destination);
    foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
    {
        var relative = Path.GetRelativePath(source, file);
        var target = Path.Combine(destination, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        File.Copy(file, target, true);
    }
}

static async Task ServeAsync(string root, int port, string pathBase)
{
    using var listener = new HttpListener();
    listener.Prefixes.Add("http://127.0.0.1:" + port + "/");
    listener.Start();
    Console.WriteLine("Serving static Wiki at http://127.0.0.1:" + port + ExternalPath(pathBase, "/"));
    Console.WriteLine("Press Ctrl+C to stop.");

    while (true)
    {
        var context = await listener.GetContextAsync();
        _ = Task.Run(async () =>
        {
            try
            {
                var requestPath = Uri.UnescapeDataString(context.Request.Url?.AbsolutePath ?? "/");
                if (!string.IsNullOrEmpty(pathBase) && requestPath.StartsWith(pathBase, StringComparison.OrdinalIgnoreCase))
                    requestPath = requestPath[pathBase.Length..];
                if (requestPath.Length == 0)
                    requestPath = "/";
                var segments = requestPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                var safePath = segments.Any(segment => segment is "." or "..")
                    ? "__invalid_path__"
                    : Path.Combine(segments);
                var candidate = requestPath == "/"
                    ? Path.Combine(root, "index.html")
                    : Path.Combine(root, safePath);
                if (Directory.Exists(candidate))
                    candidate = Path.Combine(candidate, "index.html");
                if (!File.Exists(candidate))
                    candidate = Path.Combine(root, "404.html");
                var bytes = await File.ReadAllBytesAsync(candidate);
                context.Response.StatusCode = Path.GetFileName(candidate) == "404.html" ? 404 : 200;
                context.Response.ContentType = ContentType(Path.GetExtension(candidate));
                context.Response.ContentLength64 = bytes.Length;
                await context.Response.OutputStream.WriteAsync(bytes);
                context.Response.Close();
            }
            catch
            {
                context.Response.StatusCode = 500;
                context.Response.Close();
            }
        });
    }
}

static string ContentType(string extension)
    => extension.ToLowerInvariant() switch
    {
        ".html" => "text/html; charset=utf-8",
        ".css" => "text/css; charset=utf-8",
        ".js" or ".mjs" => "text/javascript; charset=utf-8",
        ".json" or ".map" => "application/json; charset=utf-8",
        ".svg" => "image/svg+xml",
        ".txt" => "text/plain; charset=utf-8",
        ".xml" => "application/xml; charset=utf-8",
        _ => "application/octet-stream"
    };

static void EnsureDirectoryDeletedWithinRepo(string repoRoot, string path)
{
    if (!Directory.Exists(path))
        return;
    var root = Path.GetFullPath(repoRoot).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
    var fullPath = Path.GetFullPath(path);
    if (!fullPath.StartsWith(root, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
        throw new InvalidOperationException("Refusing to delete outside repository: " + fullPath);
    Directory.Delete(fullPath, true);
}

static void RemoveDirectoryWithRetry(string path)
{
    if (Directory.Exists(path))
        Directory.Delete(path, true);
}

static void RemoveFileWithRetry(string path)
{
    if (File.Exists(path))
        File.Delete(path);
}

internal sealed record ExportOptions
{
    public string Output { get; init; } = "output/wiki";
    public string PathBase { get; init; } = "/Jazor";
    public string SiteOrigin { get; init; } = "https://devhxj.github.io";
    public int Port { get; init; } = 4317;
    public int ServePort { get; init; } = 4327;
    public bool Serve { get; init; }
    public bool KeepPublish { get; init; }
    public bool KeepLogs { get; init; }

    public static ExportOptions Parse(string[] args)
    {
        var result = new ExportOptions();
        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            string Next() => index + 1 < args.Length ? args[++index] : throw new InvalidOperationException("Missing value for " + arg);
            result = arg switch
            {
                "--output" => result with { Output = Next() },
                "--path-base" => result with { PathBase = Next() },
                "--site-origin" => result with { SiteOrigin = Next() },
                "--port" => result with { Port = int.Parse(Next()) },
                "--serve-port" => result with { ServePort = int.Parse(Next()) },
                "--serve" => result with { Serve = true },
                "--keep-publish" => result with { KeepPublish = true },
                "--keep-logs" => result with { KeepLogs = true },
                "--help" or "-h" => throw new InvalidOperationException("Usage: dotnet run --file scripts/csharp/wiki-export-static.cs -- [--output path] [--path-base /Jazor] [--site-origin https://devhxj.github.io] [--serve]"),
                _ => throw new InvalidOperationException("Unknown argument: " + arg)
            };
        }
        return result;
    }
}
