#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.Net;
using System.Text.Json;

var options = ScriptArguments.Parse(args);
var repoRoot = PlaygroundScriptHelpers.RequireRepoRoot();
var projectRoot = Path.Combine(repoRoot, "src", "Playground", "Playground");
var projectPath = Path.Combine(projectRoot, "Playground.csproj");
var publishRoot = Path.Combine(repoRoot, ".tmp", "playground-publish-smoke-" + Environment.ProcessId);
var dotnetCliHome = Path.Combine(repoRoot, ".dotnet");
var rootUrl = "http://127.0.0.1:" + options.Port;

string hostRoot = projectRoot;
string webRoot = Path.Combine(hostRoot, "wwwroot");
string emittedJazorRoot = Path.Combine(projectRoot, "jazor");
string publicJazorRoot = Path.Combine(webRoot, "jazor");
string hostCommand = "run";
var hostArguments = new List<string>
{
    "--no-build",
    "--project",
    projectPath,
    "--urls",
    rootUrl
};

if (options.Publish)
{
    PlaygroundScriptHelpers.EnsureDirectoryDeletedWithinRepo(repoRoot, publishRoot);
    await PlaygroundScriptHelpers.RunDotNetAsync(
        [
            "publish",
            projectPath,
            "-c",
            options.Configuration,
            "-o",
            publishRoot,
            "/m:1",
            "/p:BuildInParallel=false",
            "/nr:false",
            "-p:UseSharedCompilation=false"
        ],
        workdir: repoRoot,
        dotnetCliHome: dotnetCliHome);

    hostRoot = publishRoot;
    webRoot = Path.Combine(hostRoot, "wwwroot");
    emittedJazorRoot = Path.Combine(webRoot, "jazor");
    publicJazorRoot = emittedJazorRoot;
    var shadowJazorRoot = Path.Combine(hostRoot, "jazor");
    if (Directory.Exists(shadowJazorRoot))
    {
        throw new InvalidOperationException("Unexpected publish shadow directory: " + shadowJazorRoot + ". Publish output must serve /jazor only from wwwroot/jazor.");
    }

    hostCommand = Path.Combine(hostRoot, "Playground.dll");
    hostArguments =
    [
        "--urls",
        rootUrl
    ];
}
else if (options.Build)
{
    await PlaygroundScriptHelpers.RunDotNetAsync(
        [
            "build",
            projectPath,
            "-c",
            options.Configuration,
            "/m:1",
            "/p:BuildInParallel=false",
            "/nr:false",
            "-p:UseSharedCompilation=false"
        ],
        workdir: repoRoot,
        dotnetCliHome: dotnetCliHome);
}

var manifestPath = Path.Combine(emittedJazorRoot, "jazor-manifest-razorvue.json");
var hostRequirementsPath = Path.Combine(emittedJazorRoot, "__jazor", "razorvue-host.mjs");
var clientEntryPath = Path.Combine(publicJazorRoot, "client-entry.js");
var clientCssPath = Path.Combine(publicJazorRoot, "client-entry.css");
var legacyAssetsRoot = Path.Combine(webRoot, "assets");

PlaygroundScriptHelpers.EnsureFileExists(clientEntryPath, "browser entry bundle");
PlaygroundScriptHelpers.EnsureFileExists(clientCssPath, "browser CSS bundle");
PlaygroundScriptHelpers.EnsureFileExists(manifestPath, "RazorVue manifest");
PlaygroundScriptHelpers.EnsureFileExists(hostRequirementsPath, "RazorVue host requirements module");
if (Directory.Exists(legacyAssetsRoot))
{
    throw new InvalidOperationException("Unexpected legacy Playground browser bundle directory: " + legacyAssetsRoot + ". Browser bundles must be emitted under wwwroot/jazor.");
}

using var host = StartHost(options.Publish ? "dotnet" : "dotnet", options.Publish ? [hostCommand, ..hostArguments] : [hostCommand, ..hostArguments], hostRoot);
try
{
    using var httpClient = new HttpClient
    {
        BaseAddress = new Uri(rootUrl),
        Timeout = TimeSpan.FromSeconds(10)
    };

    await WaitForReadyAsync(httpClient);
    await AssertEndpointAsync(httpClient, "/health", HttpStatusCode.OK, "application/json", "playground-host");
    await AssertEndpointAsync(httpClient, "/api/playground/examples", HttpStatusCode.OK, "application/json", "Catalog shell with API-backed discovery");
    await AssertEndpointAsync(httpClient, "/api/playground/examples/catalog-shell", HttpStatusCode.OK, "application/json", "whyItMatters");
    var html = await AssertEndpointAsync(httpClient, "/", HttpStatusCode.OK, "text/html", "/jazor/client-entry.js");
    AssertContains(html, "/jazor/client-entry.css", "HTML shell stylesheet reference");
    await AssertEndpointAsync(httpClient, "/examples/catalog-shell", HttpStatusCode.OK, "text/html", "/jazor/client-entry.js");
    await AssertEndpointAsync(httpClient, "/jazor/client-entry.js", HttpStatusCode.OK, "text/javascript", "mountPlaygroundApp");
    await AssertEndpointAsync(httpClient, "/jazor/client-entry.css", HttpStatusCode.OK, "text/css", ".playground-app-shell");
    await AssertEndpointAsync(httpClient, "/jazor/jazor-manifest-razorvue.json", HttpStatusCode.OK, "application/json", "PlaygroundCatalogPage");
    await AssertEndpointAsync(httpClient, "/jazor/__jazor/razorvue-host.mjs", HttpStatusCode.OK, "text/javascript", "razorVueHostRequirements");

    using var catalogDocument = JsonDocument.Parse(await httpClient.GetStringAsync("/api/playground/examples"));
    var catalogRoot = catalogDocument.RootElement;
    if (!catalogRoot.TryGetProperty("examples", out var examples) || examples.GetArrayLength() != 4)
    {
        throw new InvalidOperationException("Expected catalog API to return exactly 4 examples.");
    }

    Console.WriteLine("Playground smoke passed.");
    Console.WriteLine("mode=" + (options.Publish ? "publish" : "local"));
    Console.WriteLine("hostRoot=" + hostRoot);
    Console.WriteLine("webRoot=" + webRoot);
    Console.WriteLine("emittedJazorRoot=" + emittedJazorRoot);
    Console.WriteLine("publicJazorRoot=" + publicJazorRoot);
}
finally
{
    if (!host.HasExited)
    {
        host.Kill(entireProcessTree: true);
        await host.WaitForExitAsync();
    }
}

static Process StartHost(string fileName, IReadOnlyList<string> arguments, string workdir)
{
    var startInfo = new ProcessStartInfo
    {
        FileName = fileName,
        WorkingDirectory = workdir,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true
    };

    foreach (var argument in arguments)
    {
        startInfo.ArgumentList.Add(argument);
    }

    var process = Process.Start(startInfo)
        ?? throw new InvalidOperationException("Failed to start Playground host.");

    _ = Task.Run(async () =>
    {
        while (!process.HasExited && await process.StandardOutput.ReadLineAsync() is { })
        {
        }
    });
    _ = Task.Run(async () =>
    {
        while (!process.HasExited && await process.StandardError.ReadLineAsync() is { })
        {
        }
    });

    return process;
}

static async Task WaitForReadyAsync(HttpClient httpClient)
{
    Exception? lastException = null;
    for (var attempt = 0; attempt < 80; attempt++)
    {
        try
        {
            using var response = await httpClient.GetAsync("/health");
            if (response.IsSuccessStatusCode)
            {
                return;
            }
        }
        catch (Exception exception)
        {
            lastException = exception;
        }

        await Task.Delay(250);
    }

    throw new TimeoutException("Playground host did not become ready.", lastException);
}

static async Task<string> AssertEndpointAsync(
    HttpClient httpClient,
    string path,
    HttpStatusCode expectedStatusCode,
    string expectedContentTypePrefix,
    string expectedBodyMarker)
{
    using var response = await httpClient.GetAsync(path);
    var body = await response.Content.ReadAsStringAsync();
    if (response.StatusCode != expectedStatusCode)
    {
        throw new InvalidOperationException($"Unexpected status for {path}: {(int)response.StatusCode}.");
    }

    var contentType = response.Content.Headers.ContentType?.ToString() ?? "";
    if (!contentType.StartsWith(expectedContentTypePrefix, StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException($"Unexpected content type for {path}: {contentType}.");
    }

    AssertContains(body, expectedBodyMarker, path);
    return body;
}

static void AssertContains(string text, string expected, string description)
{
    if (!text.Contains(expected, StringComparison.Ordinal))
    {
        throw new InvalidOperationException(description + " did not contain expected marker: " + expected);
    }
}

internal sealed record ScriptArguments
{
    public string Configuration { get; init; } = "Debug";

    public bool ConfigurationWasExplicit { get; init; }

    public bool Build { get; init; }

    public bool Publish { get; init; }

    public int Port { get; init; } = 5188;

    public static ScriptArguments Parse(string[] args)
    {
        var result = new ScriptArguments();
        for (var index = 0; index < args.Length; index++)
        {
            var argument = args[index];
            switch (argument)
            {
                case "--configuration":
                    result = result with
                    {
                        Configuration = GetValue(args, ref index, argument),
                        ConfigurationWasExplicit = true
                    };
                    break;
                case "--build":
                    result = result with { Build = true };
                    break;
                case "--publish":
                    result = result with { Publish = true };
                    break;
                case "--port":
                    result = result with { Port = int.Parse(GetValue(args, ref index, argument), System.Globalization.CultureInfo.InvariantCulture) };
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

        if (result.Publish && result.Build)
        {
            throw new InvalidOperationException("Use either --build or --publish, not both.");
        }

        if (result.Publish && !result.ConfigurationWasExplicit)
        {
            result = result with { Configuration = "Release" };
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
        Console.WriteLine("Usage: dotnet run --file scripts/csharp/playground-verify-smoke.cs -- [options]");
        Console.WriteLine("Options:");
        Console.WriteLine("  --build");
        Console.WriteLine("  --publish");
        Console.WriteLine("  --configuration <Debug|Release>");
        Console.WriteLine("  --port <port>");
    }
}

internal static class PlaygroundScriptHelpers
{
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

    public static async Task RunDotNetAsync(
        IReadOnlyList<string> arguments,
        string workdir,
        string dotnetCliHome,
        CancellationToken cancellationToken = default)
    {
        using var process = StartProcess(
            fileName: "dotnet",
            arguments,
            workdir,
            environment:
            [
                new KeyValuePair<string, string?>("DOTNET_CLI_HOME", dotnetCliHome),
                new KeyValuePair<string, string?>("DOTNET_SKIP_FIRST_TIME_EXPERIENCE", "1")
            ]);

        await process.WaitForExitAsync(cancellationToken);
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Process failed with exit code {process.ExitCode}: dotnet {string.Join(' ', arguments)}");
        }
    }

    private static Process StartProcess(
        string fileName,
        IReadOnlyList<string> arguments,
        string workdir,
        IReadOnlyList<KeyValuePair<string, string?>>? environment = null)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            WorkingDirectory = workdir,
            UseShellExecute = false,
            CreateNoWindow = true
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

        return Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start process: " + fileName);
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
        var resolvedRepoRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(repoRoot));
        var resolvedPath = Path.GetFullPath(path);
        if (!resolvedPath.StartsWith(resolvedRepoRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Refusing to delete a directory outside the repository: " + resolvedPath);
        }

        if (Directory.Exists(resolvedPath))
        {
            Directory.Delete(resolvedPath, recursive: true);
        }
    }
}
