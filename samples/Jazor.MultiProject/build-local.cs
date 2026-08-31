#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.Globalization;

var options = SampleBuildOptions.Parse(args);
var repoRoot = ScriptHelpers.FindRepositoryRoot(Directory.GetCurrentDirectory());
var sampleRoot = Path.Combine(repoRoot, "samples", "Jazor.MultiProject");
var runtimeProject = Path.Combine(repoRoot, "src", "ECMAScript", "ECMAScript.csproj");
var analyzerProject = Path.Combine(repoRoot, "src", "Jazor.Analyzer", "Jazor.Analyzer.csproj");
var emitProject = Path.Combine(repoRoot, "src", "Jazor.Emit", "Jazor.Emit.csproj");
var packageProject = Path.Combine(repoRoot, "src", "Jazor", "Jazor.csproj");
var hostProject = Path.Combine(sampleRoot, "Sample.Host", "Sample.Host.csproj");
var packageOutput = Path.Combine(repoRoot, ".tmp", "nupkg-sample");
var dotnetCliHome = Path.Combine(repoRoot, ".dotnet");

ScriptHelpers.SetCommonEnvironment(dotnetCliHome);
ScriptHelpers.CleanDirectoryWithinRepo(packageOutput, repoRoot);

var emitPublishDirectory = Path.Combine(repoRoot, "src", "Jazor.Emit", "bin", options.Configuration, "net11.0", "publish");
var sharedArguments = new List<string> { "/m:1", "/p:BuildInParallel=false", "/nr:false", "-p:UseSharedCompilation=false" };

await ScriptHelpers.RunDotNetAsync(["build", runtimeProject, "-c", options.Configuration, .. sharedArguments], repoRoot, dotnetCliHome);
await ScriptHelpers.RunDotNetAsync(["build", analyzerProject, "-c", options.Configuration, .. sharedArguments], repoRoot, dotnetCliHome);
await ScriptHelpers.RunDotNetAsync(["publish", emitProject, "-c", options.Configuration, "-o", emitPublishDirectory, .. sharedArguments], repoRoot, dotnetCliHome);
await ScriptHelpers.RunDotNetAsync(["pack", packageProject, "-c", options.Configuration, "--no-build", "-o", packageOutput, "/nr:false", "-p:UseSharedCompilation=false"], repoRoot, dotnetCliHome);

var packageInfo = ScriptHelpers.ResolveLatestPackage(packageOutput);
var restorePackagesPath = Path.Combine(repoRoot, ".tmp", "nuget-sample-packages", $"{packageInfo.Version}-{packageInfo.Stamp}");

var buildArguments = new List<string>
{
    "build",
    hostProject,
    "-t:Rebuild",
    "/m:1",
    "/p:BuildInParallel=false",
    $"-p:RestoreAdditionalProjectSources={packageOutput}",
    $"-p:RestorePackagesPath={restorePackagesPath}",
    "-p:RestoreForce=true",
    $"-p:JazorPackageVersion={packageInfo.Version}",
    "/nr:false",
    "-p:UseSharedCompilation=false"
};

if (options.Bundle)
{
    buildArguments.Add("-p:JazorMode=release");
}

await ScriptHelpers.RunDotNetAsync(buildArguments, repoRoot, dotnetCliHome);

internal sealed record SampleBuildOptions(string Configuration, bool Bundle)
{
    public static SampleBuildOptions Parse(IReadOnlyList<string> arguments)
    {
        var configuration = "Debug";
        var bundle = false;

        for (var index = 0; index < arguments.Count; index++)
        {
            var argument = arguments[index];
            switch (argument)
            {
                case "--configuration":
                case "-Configuration":
                case "-c":
                    configuration = RequireValue(arguments, ref index, argument);
                    break;
                case "--bundle":
                case "-Bundle":
                    bundle = true;
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

        return new SampleBuildOptions(configuration, bundle);
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
        Console.WriteLine("Usage: dotnet run --file samples/Jazor.MultiProject/build-local.cs -- [options]");
        Console.WriteLine("Options:");
        Console.WriteLine("  --configuration <Debug|Release>");
        Console.WriteLine("  --bundle");
    }
}

internal sealed record PackageInfo(string Version, string Stamp);

internal static class ScriptHelpers
{
    public static string FindRepositoryRoot(string startDirectory)
    {
        var current = new DirectoryInfo(Path.GetFullPath(startDirectory));
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Jazor.slnx")))
            {
                return current.FullName;
            }

            current = current.Parent;
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

    public static PackageInfo ResolveLatestPackage(string packageOutput)
    {
        var packageFile = new DirectoryInfo(packageOutput)
            .EnumerateFiles("Jazor.*.nupkg", SearchOption.TopDirectoryOnly)
            .Where(static file => !file.Name.EndsWith(".snupkg", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(static file => file.LastWriteTimeUtc)
            .FirstOrDefault();

        if (packageFile is null)
        {
            throw new InvalidOperationException($"Packed Jazor package not found under '{packageOutput}'.");
        }

        var version = Path.GetFileNameWithoutExtension(packageFile.Name).Replace("Jazor.", string.Empty, StringComparison.Ordinal);
        var stamp = packageFile.LastWriteTimeUtc.ToString("yyyyMMddHHmmssffff", CultureInfo.InvariantCulture);
        return new PackageInfo(version, stamp);
    }

    public static void CleanDirectoryWithinRepo(string path, string repoRoot)
    {
        var fullPath = Path.GetFullPath(path);
        var fullRoot = Path.GetFullPath(repoRoot);
        if (!fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Refusing to delete a path outside the repository root: " + fullPath);
        }

        if (Directory.Exists(fullPath))
        {
            Directory.Delete(fullPath, recursive: true);
        }
    }

    public static async Task RunDotNetAsync(
        IReadOnlyList<string> arguments,
        string workdir,
        string dotnetCliHome,
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

        startInfo.Environment["DOTNET_CLI_HOME"] = dotnetCliHome;
        startInfo.Environment["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1";
        startInfo.Environment["MSBUILDDISABLENODEREUSE"] = "1";
        startInfo.Environment["UseSharedCompilation"] = "false";

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start process: dotnet");

        await process.WaitForExitAsync(cancellationToken);
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Process failed with exit code {process.ExitCode}: dotnet {string.Join(' ', arguments)}");
        }
    }
}
