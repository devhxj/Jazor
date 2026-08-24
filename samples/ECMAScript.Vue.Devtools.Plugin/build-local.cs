#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.Globalization;

var options = SampleBuildOptions.Parse(args);
var repoRoot = ScriptHelpers.FindRepositoryRoot(Directory.GetCurrentDirectory());
var sampleRoot = Path.Combine(repoRoot, "samples", "ECMAScript.Vue.Devtools.Plugin");
var hostProject = Path.Combine(sampleRoot, "Devtools.Plugin.Host", "Devtools.Plugin.Host.csproj");
var packageOutput = Path.Combine(repoRoot, ".tmp", "nupkg-devtools-sample");
var artifactRoot = Path.Combine(repoRoot, ".tmp", "sample-devtools-plugin", options.Configuration);
var restorePackagesPath = Path.Combine(repoRoot, ".tmp", "nuget-devtools-sample-packages");
var publishScript = Path.Combine(repoRoot, "scripts", "csharp", "publish-nuget.cs");
var dotnetCliHome = Path.Combine(repoRoot, ".dotnet");

ScriptHelpers.SetCommonEnvironment(dotnetCliHome);
ScriptHelpers.CleanDirectoryWithinRepo(packageOutput, repoRoot);
ScriptHelpers.CleanDirectoryWithinRepo(artifactRoot, repoRoot);

await ScriptHelpers.RunDotNetAsync(
    [
        "run", "--file", publishScript, "--",
        "--configuration", options.Configuration,
        "--output-directory", packageOutput,
        "--skip-push",
        "--package", "jazor",
        "--package", "jazor-vue",
        "--package", "devtools"
    ],
    repoRoot,
    dotnetCliHome);

var packageInfo = ScriptHelpers.ResolveLatestPackage(packageOutput);
var packageCache = Path.Combine(restorePackagesPath, packageInfo.Version + "-" + packageInfo.Stamp);
await ScriptHelpers.RunDotNetAsync(
    [
        "build", hostProject,
        "-c", options.Configuration,
        "-t:Rebuild",
        "/m:1",
        "/p:BuildInParallel=false",
        "-p:RestoreAdditionalProjectSources=" + packageOutput,
        "-p:RestorePackagesPath=" + packageCache,
        "-p:RestoreForce=true",
        "-p:JazorPackageVersion=" + packageInfo.Version,
        "-p:JazorDir=" + Path.Combine(artifactRoot, "jazor"),
        "/nr:false",
        "-p:UseSharedCompilation=false"
    ],
    repoRoot,
    dotnetCliHome);

Console.WriteLine("Generated sample artifacts: " + Path.Combine(artifactRoot, "jazor"));

internal sealed record SampleBuildOptions(string Configuration)
{
    public static SampleBuildOptions Parse(IReadOnlyList<string> arguments)
    {
        var configuration = "Debug";
        for (var index = 0; index < arguments.Count; index++)
        {
            var argument = arguments[index];
            switch (argument)
            {
                case "--configuration":
                case "-c":
                    configuration = RequireValue(arguments, ref index, argument);
                    break;
                case "--help":
                case "-h":
                    Console.WriteLine("Usage: dotnet run --file samples/ECMAScript.Vue.Devtools.Plugin/build-local.cs -- [--configuration <Debug|Release>]");
                    Environment.Exit(0);
                    break;
                default:
                    throw new InvalidOperationException("Unsupported argument: " + argument);
            }
        }

        return new SampleBuildOptions(configuration);
    }

    private static string RequireValue(IReadOnlyList<string> arguments, ref int index, string option)
    {
        if (index + 1 >= arguments.Count)
            throw new InvalidOperationException("Missing value for " + option + ".");

        index++;
        return arguments[index];
    }
}

internal sealed record PackageInfo(string Version, string Stamp);

internal static class ScriptHelpers
{
    public static string FindRepositoryRoot(string startDirectory)
    {
        for (var current = new DirectoryInfo(Path.GetFullPath(startDirectory)); current is not null; current = current.Parent)
        {
            if (File.Exists(Path.Combine(current.FullName, "Jazor.slnx")))
                return current.FullName;
        }

        throw new InvalidOperationException("Cannot locate repository root (Jazor.slnx).");
    }

    public static void SetCommonEnvironment(string dotnetCliHome)
    {
        Environment.SetEnvironmentVariable("DOTNET_CLI_HOME", dotnetCliHome);
        Environment.SetEnvironmentVariable("DOTNET_SKIP_FIRST_TIME_EXPERIENCE", "1");
        Environment.SetEnvironmentVariable("MSBUILDDISABLENODEREUSE", "1");
        Environment.SetEnvironmentVariable("UseSharedCompilation", "false");
    }

    public static void CleanDirectoryWithinRepo(string path, string repoRoot)
    {
        var fullPath = Path.GetFullPath(path);
        var fullRoot = Path.GetFullPath(repoRoot);
        if (!fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Refusing to delete a path outside the repository root: " + fullPath);

        if (Directory.Exists(fullPath))
            Directory.Delete(fullPath, recursive: true);
    }

    public static PackageInfo ResolveLatestPackage(string packageOutput)
    {
        var packageFile = new DirectoryInfo(packageOutput)
            .EnumerateFiles("Jazor.*.nupkg", SearchOption.TopDirectoryOnly)
            .Where(static file => !file.Name.EndsWith(".snupkg", StringComparison.OrdinalIgnoreCase))
            // Jazor.Vue shares the Jazor.* prefix; only a numeric suffix is the core package version.
            .Where(static file => file.Name.Length > "Jazor.".Length && char.IsDigit(file.Name["Jazor.".Length]))
            .OrderByDescending(static file => file.LastWriteTimeUtc)
            .FirstOrDefault()
            ?? throw new InvalidOperationException("Packed Jazor package was not found under '" + packageOutput + "'.");

        var version = Path.GetFileNameWithoutExtension(packageFile.Name).Replace("Jazor.", string.Empty, StringComparison.Ordinal);
        return new PackageInfo(version, packageFile.LastWriteTimeUtc.ToString("yyyyMMddHHmmssffff", CultureInfo.InvariantCulture));
    }

    public static async Task RunDotNetAsync(IReadOnlyList<string> arguments, string workdir, string dotnetCliHome)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = workdir,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);

        startInfo.Environment["DOTNET_CLI_HOME"] = dotnetCliHome;
        startInfo.Environment["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1";
        startInfo.Environment["MSBUILDDISABLENODEREUSE"] = "1";
        startInfo.Environment["UseSharedCompilation"] = "false";

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start dotnet.");
        await process.WaitForExitAsync();
        if (process.ExitCode != 0)
            throw new InvalidOperationException("Process failed with exit code " + process.ExitCode + ": dotnet " + string.Join(' ', arguments));
    }
}
