#!/usr/bin/env dotnet run

using System.Diagnostics;

var options = TestJoltStressOptions.Parse(args);
var repoRoot = ScriptHelpers.RequireRepoRoot();
var dotnetCliHome = Path.Combine(repoRoot, ".dotnet");
var joltTestProject = Path.Combine(repoRoot, "src", "Jolt.Test", "Jolt.Test.csproj");

if (!File.Exists(joltTestProject))
{
    throw new FileNotFoundException("Jolt test project not found: " + joltTestProject, joltTestProject);
}

var runHmrStress = options.Mode is "all" or "hmr";
var runSourceMapMatrix = options.Mode is "all" or "matrix" or "matrix-exception";

var testFilter = options.Mode switch
{
    "hmr" => "FullyQualifiedName~Jolt_DapProcess_RealBrowserCdpAndHmrStress",
    "matrix" => "FullyQualifiedName~Jolt_DapProcess_RealBrowserCdpSourceMap",
    "matrix-exception" => "FullyQualifiedName~Jolt_DapProcess_RealBrowserCdpSourceMapExceptionMatrix",
    _ => "FullyQualifiedName~Jolt_DapProcess_RealBrowserCdp"
};

var environment = new List<KeyValuePair<string, string?>>
{
    new("DOTNET_CLI_HOME", dotnetCliHome),
    new("DOTNET_SKIP_FIRST_TIME_EXPERIENCE", "1"),
    new("JOLT_RUN_REAL_CDP_HMR_STRESS", runHmrStress ? "true" : "false"),
    new("JOLT_RUN_REAL_CDP_SOURCE_MAP_MATRIX", runSourceMapMatrix ? "true" : "false"),
    new("MSBUILDDISABLENODEREUSE", "1"),
    new("UseSharedCompilation", "false")
};

if (!string.IsNullOrWhiteSpace(options.BrowserPath))
{
    var fullBrowserPath = Path.GetFullPath(options.BrowserPath);
    if (!File.Exists(fullBrowserPath))
    {
        throw new FileNotFoundException("Browser executable path does not exist: " + fullBrowserPath, fullBrowserPath);
    }

    environment.Add(new KeyValuePair<string, string?>("JOLT_REAL_BROWSER_PATH", fullBrowserPath));
}

await ScriptHelpers.RunDotNetAsync(
    [
        "build",
        joltTestProject,
        "-c",
        options.Configuration,
        "/m:1",
        "/p:BuildInParallel=false",
        "-v",
        "minimal",
        "/nr:false",
        "-p:UseSharedCompilation=false"
    ],
    repoRoot,
    environment);

await ScriptHelpers.RunDotNetAsync(
    [
        "test",
        joltTestProject,
        "-c",
        options.Configuration,
        "--no-build",
        "--no-restore",
        "--filter",
        testFilter,
        "-v",
        "minimal",
        "/nr:false",
        "-p:UseSharedCompilation=false"
    ],
    repoRoot,
    environment);

internal sealed record TestJoltStressOptions(string Mode, string Configuration, string? BrowserPath)
{
    public static TestJoltStressOptions Parse(IReadOnlyList<string> arguments)
    {
        var mode = "all";
        var configuration = "Debug";
        string? browserPath = null;

        for (var index = 0; index < arguments.Count; index++)
        {
            var argument = arguments[index];
            switch (argument)
            {
                case "--mode":
                case "-Mode":
                    mode = RequireValue(arguments, ref index, argument).Trim().ToLowerInvariant();
                    break;
                case "--configuration":
                case "-Configuration":
                case "-c":
                    configuration = RequireValue(arguments, ref index, argument);
                    break;
                case "--browser-path":
                case "-BrowserPath":
                    browserPath = RequireValue(arguments, ref index, argument);
                    break;
                case "--help":
                case "-h":
                    WriteUsage();
                    Environment.Exit(0);
                    break;
                default:
                    throw new InvalidOperationException("Unsupported argument: " + argument);
            }
        }

        var supportedModes = new HashSet<string>(StringComparer.Ordinal)
        {
            "all",
            "hmr",
            "matrix",
            "matrix-exception"
        };

        if (!supportedModes.Contains(mode))
        {
            throw new InvalidOperationException("Unsupported mode: " + mode);
        }

        return new TestJoltStressOptions(mode, configuration, browserPath);
    }

    static string RequireValue(IReadOnlyList<string> arguments, ref int index, string option)
    {
        var nextIndex = index + 1;
        if (nextIndex >= arguments.Count)
        {
            throw new InvalidOperationException("Missing value for " + option + ".");
        }

        index = nextIndex;
        return arguments[index];
    }

    static void WriteUsage()
    {
        Console.WriteLine("Usage: dotnet run --file scripts/csharp/test-jolt-stress.cs -- [options]");
        Console.WriteLine("Options:");
        Console.WriteLine("  --mode <all|hmr|matrix|matrix-exception>");
        Console.WriteLine("  --configuration <Debug|Release>");
        Console.WriteLine("  --browser-path <path>");
    }
}

internal static class ScriptHelpers
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
        IReadOnlyList<KeyValuePair<string, string?>> environment,
        CancellationToken cancellationToken = default)
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

        foreach (var (key, value) in environment)
        {
            if (value is null)
            {
                startInfo.Environment.Remove(key);
            }
            else
            {
                startInfo.Environment[key] = value;
            }
        }

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start process: dotnet");

        await process.WaitForExitAsync(cancellationToken);
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Process failed with exit code {process.ExitCode}: dotnet {string.Join(' ', arguments)}");
        }
    }
}
