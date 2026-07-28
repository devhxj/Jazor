#!/usr/bin/env dotnet run

using System.Diagnostics;

var options = ScriptArguments.Parse(args);

var repoRoot = WikiScriptHelpers.RequireRepoRoot();
var sampleRoot = Path.Combine(repoRoot, "src", "Wiki");
var hostProject = Path.Combine(sampleRoot, "Wiki.csproj");
var publishRoot = Path.Combine(repoRoot, ".tmp", "wiki-publish-preview", options.Configuration);
var hostRoot = sampleRoot;
var webRoot = Path.Combine(sampleRoot, "wwwroot");
var jazorRoot = Path.Combine(sampleRoot, "wwwroot", "jazor");
var mainModulePath = Path.Combine(jazorRoot, "main.mjs");
var componentModulePath = Path.Combine(jazorRoot, "components", "wiki-home.mjs");
var dotnetCliHome = Path.Combine(repoRoot, ".dotnet");
var configuration = options.Publish && !options.ConfigurationWasExplicit ? "Release" : options.Configuration;
var normalizedPathBase = WikiScriptHelpers.NormalizePathBase(options.PathBase);
var rootUrl = $"http://localhost:{options.Port}";

if (options.Publish && (options.Build || options.BuildLocal))
{
    throw new InvalidOperationException("--publish already performs its own publish build. Do not combine it with --build or --build-local.");
}

if (options.Publish)
{
    publishRoot = Path.Combine(repoRoot, ".tmp", "wiki-publish-preview", configuration);
    WikiScriptHelpers.EnsureDirectoryDeletedWithinRepo(repoRoot, publishRoot);

    var publishArguments = new List<string>
    {
        "publish",
        hostProject,
        "-c",
        configuration,
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

    hostRoot = publishRoot;
    webRoot = Path.Combine(hostRoot, "wwwroot");
    jazorRoot = Path.Combine(webRoot, "jazor");
    mainModulePath = Path.Combine(jazorRoot, "main.mjs");
    componentModulePath = Path.Combine(jazorRoot, "components", "wiki-home.mjs");

    var publishShadowJazorRoot = Path.Combine(hostRoot, "jazor");
    if (Directory.Exists(publishShadowJazorRoot))
    {
        throw new InvalidOperationException("Unexpected publish shadow directory: " + publishShadowJazorRoot + ". Published preview must serve /jazor only from wwwroot/jazor.");
    }
}
else if (options.BuildLocal)
{
    var buildLocalArguments = new List<string>
    {
        "run",
        "--file",
        Path.Combine("scripts", "csharp", "wiki-build-local.cs"),
        "--",
        "--configuration",
        configuration
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
}
else if (options.Build)
{
    var buildArguments = new List<string>
    {
        "build",
        hostProject,
        "-c",
        configuration,
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
}

WikiScriptHelpers.EnsureFileExists(mainModulePath, "emitted main module");
WikiScriptHelpers.EnsureFileExists(componentModulePath, "emitted wiki component module");

var routeUrls = new[]
{
    rootUrl + WikiScriptHelpers.GetExternalPath(normalizedPathBase, "/"),
    rootUrl + WikiScriptHelpers.GetExternalPath(normalizedPathBase, "/guides/getting-started"),
    rootUrl + WikiScriptHelpers.GetExternalPath(normalizedPathBase, "/guides/content-model"),
    rootUrl + WikiScriptHelpers.GetExternalPath(normalizedPathBase, "/guides/navigation-discovery"),
    rootUrl + WikiScriptHelpers.GetExternalPath(normalizedPathBase, "/guides/information-architecture"),
    rootUrl + WikiScriptHelpers.GetExternalPath(normalizedPathBase, "/engineering/h-function-authoring"),
    rootUrl + WikiScriptHelpers.GetExternalPath(normalizedPathBase, "/engineering/compiler-support-boundary"),
    rootUrl + WikiScriptHelpers.GetExternalPath(normalizedPathBase, "/engineering/route-catalog-contract"),
    rootUrl + WikiScriptHelpers.GetExternalPath(normalizedPathBase, "/engineering/host-semantic-seams"),
    rootUrl + WikiScriptHelpers.GetExternalPath(normalizedPathBase, "/engineering/import-emit-contract"),
    rootUrl + WikiScriptHelpers.GetExternalPath(normalizedPathBase, "/engineering/runtime-catalog"),
    rootUrl + WikiScriptHelpers.GetExternalPath(normalizedPathBase, "/operations/content-governance"),
    rootUrl + WikiScriptHelpers.GetExternalPath(normalizedPathBase, "/operations/deployment"),
    rootUrl + WikiScriptHelpers.GetExternalPath(normalizedPathBase, "/operations/testing-verification")
};

if (options.Publish)
{
    Console.WriteLine("Serving published jazor.wiki from: " + webRoot);
    Console.WriteLine("Serving published Jazor modules from: " + jazorRoot);
    Console.WriteLine("Published preview root: " + hostRoot);
}
else
{
    Console.WriteLine("Serving jazor.wiki from: " + webRoot);
    Console.WriteLine("Serving emitted Jazor modules from: " + jazorRoot);
}

Console.WriteLine("Open routes:");
foreach (var routeUrl in routeUrls)
{
    Console.WriteLine(" - " + routeUrl);
}

if (!string.IsNullOrEmpty(normalizedPathBase))
{
    Console.WriteLine("PathBase: " + normalizedPathBase);
}

if (options.DryRun)
{
    Console.WriteLine(options.Publish
        ? "Dry-run mode: published preview artifacts exist and the published host was not started."
        : "Dry-run mode: emitted modules exist and the static server was not started.");
    return;
}

var hostArguments = options.Publish
    ? new[] { "Wiki.dll", "--urls", rootUrl }
    : new[] { "run", "--project", hostProject, "--no-launch-profile", "-c", configuration, "--urls", rootUrl }
        .Concat(options.Build || options.BuildLocal ? new[] { "--no-build", "--no-restore" } : Array.Empty<string>())
        .ToArray();

using var hostProcess = WikiScriptHelpers.StartProcess(
    fileName: "dotnet",
    arguments: hostArguments,
    workdir: options.Publish ? hostRoot : sampleRoot,
    environment:
    [
        new KeyValuePair<string, string?>("DOTNET_CLI_HOME", dotnetCliHome),
        new KeyValuePair<string, string?>("DOTNET_SKIP_FIRST_TIME_EXPERIENCE", "1"),
        new KeyValuePair<string, string?>("ASPNETCORE_URLS", rootUrl),
        new KeyValuePair<string, string?>("ASPNETCORE_ENVIRONMENT", options.Publish ? "Production" : "Development"),
        new KeyValuePair<string, string?>("DOTNET_ENVIRONMENT", options.Publish ? "Production" : "Development"),
        new KeyValuePair<string, string?>("Wiki__PathBase", normalizedPathBase)
    ]);

await hostProcess.WaitForExitAsync();
Environment.ExitCode = hostProcess.ExitCode;

internal sealed record ScriptArguments
{
    public int Port { get; init; } = 4173;

    public string Configuration { get; init; } = "Debug";

    public bool ConfigurationWasExplicit { get; init; }

    public string? BaseOutputPath { get; init; }

    public string? BaseIntermediateOutputPath { get; init; }

    public string? PathBase { get; init; }

    public bool Build { get; init; }

    public bool BuildLocal { get; init; }

    public bool Publish { get; init; }

    public bool DryRun { get; init; }

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
                case "--dry-run":
                    result = result with { DryRun = true };
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
        Console.WriteLine("Usage: dotnet run --file scripts/csharp/wiki-serve.cs -- [options]");
        Console.WriteLine("Options:");
        Console.WriteLine("  --port <number>");
        Console.WriteLine("  --configuration <Debug|Release>");
        Console.WriteLine("  --base-output-path <path>");
        Console.WriteLine("  --base-intermediate-output-path <path>");
        Console.WriteLine("  --path-base </docs>");
        Console.WriteLine("  --build");
        Console.WriteLine("  --build-local");
        Console.WriteLine("  --publish");
        Console.WriteLine("  --dry-run");
    }
}

internal static class WikiScriptHelpers
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

    public static string NormalizePathBase(string? pathBase)
    {
        if (string.IsNullOrWhiteSpace(pathBase) || pathBase == "/")
        {
            return string.Empty;
        }

        if (!pathBase.StartsWith('/', StringComparison.Ordinal))
        {
            throw new InvalidOperationException("--path-base must start with '/'.");
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
        CancellationToken cancellationToken = default)
    {
        using var process = StartProcess(
            fileName: "dotnet",
            arguments: arguments,
            workdir: workdir,
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

    public static Process StartProcess(
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
}
