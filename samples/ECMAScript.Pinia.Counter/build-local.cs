#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.Globalization;

var options = SampleBuildOptions.Parse(args);
var repoRoot = ScriptHelpers.FindRepositoryRoot(Directory.GetCurrentDirectory());
var sampleRoot = Path.Combine(repoRoot, "samples", "ECMAScript.Pinia.Counter");
var hostProject = Path.Combine(sampleRoot, "Pinia.Counter.Host", "Pinia.Counter.Host.csproj");
var packageOutput = Path.Combine(repoRoot, ".tmp", "nupkg-sample");
var packageProject = Path.Combine(repoRoot, "src", "Jazor", "Jazor.csproj");
var dotnetCliHome = Path.Combine(repoRoot, ".dotnet");

ScriptHelpers.SetCommonEnvironment(dotnetCliHome);
ScriptHelpers.CleanDirectoryWithinRepo(packageOutput, repoRoot);

if (!string.IsNullOrWhiteSpace(options.JazorOutDir))
{
    ScriptHelpers.CleanDirectoryWithinRepo(ScriptHelpers.ResolvePath(repoRoot, options.JazorOutDir), repoRoot);
}

var isolationArguments = ScriptHelpers.GetIsolationArguments(options, repoRoot);

var packArguments = new List<string>
{
    "pack",
    packageProject,
    "-c",
    options.Configuration,
    "-o",
    packageOutput,
    "-v",
    "minimal",
    "/nr:false",
    "-p:UseSharedCompilation=false"
};
packArguments.AddRange(isolationArguments);
await ScriptHelpers.RunDotNetAsync(packArguments, repoRoot, dotnetCliHome);

var packageInfo = ScriptHelpers.ResolveLatestPackage(packageOutput);
var restorePackagesPath = Path.Combine(repoRoot, ".tmp", "nuget-sample-packages", $"{packageInfo.Version}-{packageInfo.Stamp}");

var buildArguments = new List<string>
{
    "build",
    hostProject,
    "-c",
    options.Configuration,
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
buildArguments.AddRange(isolationArguments);

if (!string.IsNullOrWhiteSpace(options.JazorOutDir))
{
    buildArguments.Add("-p:JazorOutDir=" + ScriptHelpers.ResolvePath(repoRoot, options.JazorOutDir));
}

await ScriptHelpers.RunDotNetAsync(buildArguments, repoRoot, dotnetCliHome);

internal sealed record SampleBuildOptions(
    string Configuration,
    string? BaseOutputPath,
    string? BaseIntermediateOutputPath,
    string? JazorOutDir)
{
    public static SampleBuildOptions Parse(IReadOnlyList<string> arguments)
    {
        var configuration = "Debug";
        string? baseOutputPath = null;
        string? baseIntermediateOutputPath = null;
        string? jazorOutDir = null;

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
                case "--base-output-path":
                case "-BaseOutputPath":
                    baseOutputPath = RequireValue(arguments, ref index, argument);
                    break;
                case "--base-intermediate-output-path":
                case "-BaseIntermediateOutputPath":
                    baseIntermediateOutputPath = RequireValue(arguments, ref index, argument);
                    break;
                case "--jazor-out-dir":
                case "-JazorOutDir":
                    jazorOutDir = RequireValue(arguments, ref index, argument);
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

        return new SampleBuildOptions(configuration, baseOutputPath, baseIntermediateOutputPath, jazorOutDir);
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
        Console.WriteLine("Usage: dotnet run --file samples/ECMAScript.Pinia.Counter/build-local.cs -- [options]");
        Console.WriteLine("Options:");
        Console.WriteLine("  --configuration <Debug|Release>");
        Console.WriteLine("  --base-output-path <path>");
        Console.WriteLine("  --base-intermediate-output-path <path>");
        Console.WriteLine("  --jazor-out-dir <path>");
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

    public static string ResolvePath(string repoRoot, string path)
    {
        return Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path));
    }

    public static string ResolveBuildRoot(string repoRoot, string path)
    {
        var resolved = ResolvePath(repoRoot, path);
        return resolved.EndsWith(Path.DirectorySeparatorChar)
            ? resolved
            : resolved + Path.DirectorySeparatorChar;
    }

    public static IReadOnlyList<string> GetIsolationArguments(SampleBuildOptions options, string repoRoot)
    {
        var arguments = new List<string>();
        if (!string.IsNullOrWhiteSpace(options.BaseOutputPath))
        {
            arguments.Add("-p:JazorIsolatedBaseOutputRoot=" + ResolveBuildRoot(repoRoot, options.BaseOutputPath));
        }

        if (!string.IsNullOrWhiteSpace(options.BaseIntermediateOutputPath))
        {
            arguments.Add("-p:JazorIsolatedBaseIntermediateOutputRoot=" + ResolveBuildRoot(repoRoot, options.BaseIntermediateOutputPath));
        }

        return arguments;
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
