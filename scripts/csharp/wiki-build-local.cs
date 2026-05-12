#!/usr/bin/env dotnet run

using System.Diagnostics;

var options = ScriptArguments.Parse(args);

var repoRoot = WikiScriptHelpers.RequireRepoRoot();
var sampleRoot = Path.Combine(repoRoot, "src", "Wiki");
var hostProject = Path.Combine(sampleRoot, "Wiki.csproj");
var jazorRoot = Path.Combine(sampleRoot, "jazor");
var mainModulePath = Path.Combine(jazorRoot, "main.mjs");
var componentModulePath = Path.Combine(jazorRoot, "components", "wiki-home.mjs");
var dotnetCliHome = Path.Combine(repoRoot, ".dotnet");

var arguments = new List<string>
{
    "build",
    hostProject,
    "-c",
    options.Configuration,
    "/m:1",
    "/p:BuildInParallel=false",
    "/nr:false",
    "-p:UseSharedCompilation=false"
};

if (options.Bundle)
{
    arguments.Add("-p:JazorBundle=true");
}

if (!string.IsNullOrWhiteSpace(options.BaseOutputPath))
{
    arguments.Add("-p:JazorIsolatedBaseOutputRoot=" + WikiScriptHelpers.ResolveBuildRoot(repoRoot, options.BaseOutputPath));
}

if (!string.IsNullOrWhiteSpace(options.BaseIntermediateOutputPath))
{
    arguments.Add("-p:JazorIsolatedBaseIntermediateOutputRoot=" + WikiScriptHelpers.ResolveBuildRoot(repoRoot, options.BaseIntermediateOutputPath));
}

await WikiScriptHelpers.RunDotNetAsync(
    arguments,
    workdir: repoRoot,
    dotnetCliHome: dotnetCliHome);

WikiScriptHelpers.EnsureFileExists(mainModulePath, "emitted main module");
WikiScriptHelpers.EnsureFileExists(componentModulePath, "emitted wiki component module");

internal sealed record ScriptArguments
{
    public string Configuration { get; init; } = "Debug";

    public string? BaseOutputPath { get; init; }

    public string? BaseIntermediateOutputPath { get; init; }

    public bool Bundle { get; init; }

    public static ScriptArguments Parse(string[] args)
    {
        var result = new ScriptArguments();
        for (var index = 0; index < args.Length; index++)
        {
            var argument = args[index];
            switch (argument)
            {
                case "--configuration":
                    result = result with { Configuration = GetValue(args, ref index, argument) };
                    break;
                case "--base-output-path":
                    result = result with { BaseOutputPath = GetValue(args, ref index, argument) };
                    break;
                case "--base-intermediate-output-path":
                    result = result with { BaseIntermediateOutputPath = GetValue(args, ref index, argument) };
                    break;
                case "--bundle":
                    result = result with { Bundle = true };
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
        Console.WriteLine("Usage: dotnet run --file scripts/csharp/wiki-build-local.cs -- [options]");
        Console.WriteLine("Options:");
        Console.WriteLine("  --configuration <Debug|Release>");
        Console.WriteLine("  --base-output-path <path>");
        Console.WriteLine("  --base-intermediate-output-path <path>");
        Console.WriteLine("  --bundle");
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
}
