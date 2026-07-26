#!/usr/bin/env dotnet run

using System.Diagnostics;

var options = ScriptArguments.Parse(args);
var repoRoot = ScriptHelpers.RequireRepoRoot();
var dotnetCliHome = Path.Combine(repoRoot, ".dotnet");

var compilerTestProject = Path.Combine(repoRoot, "src", "Jazor.CompilerTest", "Jazor.CompilerTest.csproj");
var clrTestProject = Path.Combine(repoRoot, "src", "Jazor.CLR.Test", "Jazor.CLR.Test.csproj");
var piniaTestProject = Path.Combine(repoRoot, "src", "ECMAScript.Pinia.Test", "ECMAScript.Pinia.Test.csproj");
var piniaTestingTestProject = Path.Combine(repoRoot, "src", "ECMAScript.Pinia.Testing.Test", "ECMAScript.Pinia.Testing.Test.csproj");
var vueRouteTestProject = Path.Combine(repoRoot, "src", "ECMAScript.VueRoute.Test", "ECMAScript.VueRoute.Test.csproj");
var razorSgTestProject = Path.Combine(repoRoot, "src", "Jazor.RazorVue.Sg.Test", "Jazor.RazorVue.Sg.Test.csproj");
var emitTestProject = Path.Combine(repoRoot, "src", "Jazor.EmitTest", "Jazor.EmitTest.csproj");
var renderContextTestScript = Path.Combine("scripts", "csharp", "test-render-context.cs");

if (options.Project == "render-context")
{
    if (!string.IsNullOrWhiteSpace(options.Filter))
    {
        throw new InvalidOperationException("--filter is not supported for render-context tests.");
    }

    await ScriptHelpers.RunDotNetAsync(["run", "--file", renderContextTestScript], repoRoot, dotnetCliHome);
    return;
}

if (options.Project is "wiki" or "wiki-publish" or "wiki-browser" or "wiki-browser-publish")
{
    if (!string.IsNullOrWhiteSpace(options.Filter))
    {
        throw new InvalidOperationException("--filter is not supported for Wiki smoke targets.");
    }

    var effectiveWikiConfiguration = options.Project is "wiki-publish" or "wiki-browser-publish" && !options.ConfigurationWasExplicit
        ? "Release"
        : options.Configuration;

    var wikiScriptPath = options.Project is "wiki" or "wiki-publish"
        ? Path.Combine("scripts", "csharp", "wiki-verify-smoke.cs")
        : Path.Combine("scripts", "csharp", "wiki-verify-browser.cs");

    var wikiArgs = new List<string>
    {
        "run",
        "--file",
        wikiScriptPath,
        "--",
        "--configuration",
        effectiveWikiConfiguration
    };

    if (!string.IsNullOrWhiteSpace(options.BaseOutputPath))
    {
        wikiArgs.Add("--base-output-path");
        wikiArgs.Add(options.BaseOutputPath);
    }

    if (!string.IsNullOrWhiteSpace(options.BaseIntermediateOutputPath))
    {
        wikiArgs.Add("--base-intermediate-output-path");
        wikiArgs.Add(options.BaseIntermediateOutputPath);
    }

    wikiArgs.Add(options.Project is "wiki-publish" or "wiki-browser-publish" ? "--publish" : "--build");

    await ScriptHelpers.RunDotNetAsync(wikiArgs, repoRoot, dotnetCliHome);
    return;
}

var testTargets = options.Project switch
{
    "compiler" => new[] { compilerTestProject },
    "clr" => new[] { clrTestProject },
    "pinia" => new[] { piniaTestProject },
    "pinia-testing" => new[] { piniaTestingTestProject },
    "vueroute" => new[] { vueRouteTestProject },
    "razor-sg" => new[] { razorSgTestProject },
    "emit" => new[] { emitTestProject },
    _ => new[]
    {
        compilerTestProject,
        clrTestProject,
        piniaTestProject,
        piniaTestingTestProject,
        vueRouteTestProject,
        razorSgTestProject,
        emitTestProject
    }
};

var sharedBuildPathArguments = ScriptHelpers.GetSharedBuildPathArguments(repoRoot, options.BaseOutputPath, options.BaseIntermediateOutputPath);
var buildTarget = testTargets.Length > 1 ? Path.Combine(repoRoot, "Jazor.slnx") : testTargets[0];

var buildArgs = new List<string> { "build", buildTarget, "-c", options.Configuration, "/m:1", "/p:BuildInParallel=false", "-v", "minimal" };
buildArgs.AddRange(sharedBuildPathArguments);
await ScriptHelpers.RunDotNetAsync(buildArgs, repoRoot, dotnetCliHome);

foreach (var testProject in testTargets)
{
    var testArgs = new List<string> { "test", testProject, "-c", options.Configuration, "--no-build", "--no-restore", "-v", "minimal" };
    testArgs.AddRange(sharedBuildPathArguments);
    if (!string.IsNullOrWhiteSpace(options.Filter))
    {
        testArgs.Add("--filter");
        testArgs.Add(options.Filter);
    }

    await ScriptHelpers.RunDotNetAsync(testArgs, repoRoot, dotnetCliHome);
}

if (options.Project == "all")
{
    await ScriptHelpers.RunDotNetAsync(["run", "--file", renderContextTestScript], repoRoot, dotnetCliHome);
}

internal sealed record ScriptArguments
{
    public string Project { get; init; } = "all";

    public string Configuration { get; init; } = "Debug";

    public bool ConfigurationWasExplicit { get; init; }

    public string Filter { get; init; } = "";

    public string? BaseOutputPath { get; init; }

    public string? BaseIntermediateOutputPath { get; init; }

    public static ScriptArguments Parse(string[] args)
    {
        var result = new ScriptArguments();
        for (var index = 0; index < args.Length; index++)
        {
            var argument = args[index];
            switch (argument)
            {
                case "--project":
                case "-p":
                    result = result with { Project = GetValue(args, ref index, argument) };
                    break;
                case "--configuration":
                case "-c":
                    result = result with
                    {
                        Configuration = GetValue(args, ref index, argument),
                        ConfigurationWasExplicit = true
                    };
                    break;
                case "--filter":
                    result = result with { Filter = GetValue(args, ref index, argument) };
                    break;
                case "--base-output-path":
                    result = result with { BaseOutputPath = GetValue(args, ref index, argument) };
                    break;
                case "--base-intermediate-output-path":
                    result = result with { BaseIntermediateOutputPath = GetValue(args, ref index, argument) };
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

        result = result with { Project = NormalizeProject(result.Project) };
        return result;
    }

    private static string NormalizeProject(string project)
    {
        var normalized = project.Trim().ToLowerInvariant();
        var supported = new HashSet<string>(StringComparer.Ordinal)
        {
            "all", "compiler", "clr", "pinia", "pinia-testing", "vueroute", "razor-sg",
            "emit", "render-context", "wiki", "wiki-publish", "wiki-browser", "wiki-browser-publish"
        };

        if (!supported.Contains(normalized))
        {
            throw new InvalidOperationException("Unsupported project: " + project);
        }

        return normalized;
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
        Console.WriteLine("Usage: dotnet run --file scripts/csharp/test-dotnet.cs -- [options]");
        Console.WriteLine("Options:");
        Console.WriteLine("  --project <all|compiler|clr|pinia|pinia-testing|vueroute|razor-sg|emit|render-context|wiki|wiki-publish|wiki-browser|wiki-browser-publish>");
        Console.WriteLine("  --configuration <Debug|Release>");
        Console.WriteLine("  --filter <expression>");
        Console.WriteLine("  --base-output-path <path>");
        Console.WriteLine("  --base-intermediate-output-path <path>");
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

    public static async Task RunDotNetAsync(IReadOnlyList<string> arguments, string workdir, string dotnetCliHome, CancellationToken cancellationToken = default)
    {
        using var process = StartProcess(
            "dotnet",
            arguments,
            workdir,
            [
                new KeyValuePair<string, string?>("DOTNET_CLI_HOME", dotnetCliHome),
                new KeyValuePair<string, string?>("DOTNET_SKIP_FIRST_TIME_EXPERIENCE", "1"),
                new KeyValuePair<string, string?>("MSBUILDDISABLENODEREUSE", "1"),
                new KeyValuePair<string, string?>("UseSharedCompilation", "false")
            ]);

        await process.WaitForExitAsync(cancellationToken);
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Process failed with exit code {process.ExitCode}: dotnet {string.Join(' ', arguments)}");
        }
    }

    public static Process StartProcess(string fileName, IReadOnlyList<string> arguments, string workdir, IReadOnlyList<KeyValuePair<string, string?>>? environment = null)
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

    public static IReadOnlyList<string> GetSharedBuildPathArguments(string repoRoot, string? baseOutputPath, string? baseIntermediateOutputPath)
    {
        var arguments = new List<string>();

        if (!string.IsNullOrWhiteSpace(baseOutputPath))
        {
            arguments.Add("-p:JazorIsolatedBaseOutputRoot=" + ResolveBuildRoot(repoRoot, baseOutputPath));
        }

        if (!string.IsNullOrWhiteSpace(baseIntermediateOutputPath))
        {
            arguments.Add("-p:JazorIsolatedBaseIntermediateOutputRoot=" + ResolveBuildRoot(repoRoot, baseIntermediateOutputPath));
        }

        arguments.Add("/nr:false");
        arguments.Add("-p:UseSharedCompilation=false");
        return arguments;
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
}
