#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;

var options = SampleBuildOptions.Parse(args);
var scriptPath = ScriptHelpers.GetScriptPath();
var sampleRoot = Path.GetDirectoryName(scriptPath)
    ?? throw new InvalidOperationException("Cannot resolve ECMAScript.Vben.ElementPlusInject sample root from script path.");
var repoRootOverride = Environment.GetEnvironmentVariable("JAZOR_SAMPLE_REPO_ROOT");
var repoRoot = !string.IsNullOrWhiteSpace(repoRootOverride)
    ? Path.GetFullPath(repoRootOverride)
    : ScriptHelpers.FindRepositoryRoot(sampleRoot);
var sampleWorkspaceRoot = Directory.GetParent(sampleRoot)?.FullName ?? sampleRoot;
var hostProject = Path.Combine(sampleRoot, "Vben.ElementPlusInject.Host", "Vben.ElementPlusInject.Host.csproj");
var elementPlusProject = Path.Combine(repoRoot, "src", "ECMAScript.ElementPlus", "ECMAScript.ElementPlus.csproj");
var vbenProject = Path.Combine(repoRoot, "src", "ECMAScript.Vben", "ECMAScript.Vben.csproj");
var packageProject = Path.Combine(repoRoot, "src", "Jazor", "Jazor.csproj");
var packageOutput = ResolveOptionalOverridePath(Environment.GetEnvironmentVariable("JAZOR_SAMPLE_PACKAGE_OUTPUT"), Path.Combine(repoRoot, ".tmp", "nupkg-sample"));
var restorePackagesRoot = ResolveOptionalOverridePath(Environment.GetEnvironmentVariable("JAZOR_SAMPLE_RESTORE_PACKAGES_ROOT"), Path.Combine(repoRoot, ".tmp", "nuget-sample-packages"));
var dotnetCliHome = Path.Combine(repoRoot, ".dotnet");

var effectiveBaseOutputPath = !string.IsNullOrWhiteSpace(options.BaseOutputPath)
    ? options.BaseOutputPath
    : Path.Combine(repoRoot, ".tmp", "ecmascript-vben-elementplusinject-out");
var effectiveBaseIntermediateOutputPath = !string.IsNullOrWhiteSpace(options.BaseIntermediateOutputPath)
    ? options.BaseIntermediateOutputPath
    : Path.Combine(repoRoot, ".tmp", "ecmascript-vben-elementplusinject-obj");

ScriptHelpers.SetCommonEnvironment(dotnetCliHome);
ScriptHelpers.CleanDirectoryWithinRoots(packageOutput, repoRoot, sampleWorkspaceRoot);

if (!string.IsNullOrWhiteSpace(options.JazorDir))
{
    ScriptHelpers.CleanDirectoryWithinRoots(
        ScriptHelpers.ResolvePath(sampleRoot, options.JazorDir),
        repoRoot,
        sampleWorkspaceRoot,
        sampleRoot);
}
else
{
    ScriptHelpers.CleanDirectoryWithinRoots(
        Path.Combine(sampleRoot, "Vben.ElementPlusInject.Host", "jazor"),
        repoRoot,
        sampleWorkspaceRoot,
        sampleRoot);
}

var isolationArguments = new[]
{
    "-p:JazorIsolatedBaseOutputRoot=" + ScriptHelpers.ResolveBuildRoot(repoRoot, effectiveBaseOutputPath),
    "-p:JazorIsolatedBaseIntermediateOutputRoot=" + ScriptHelpers.ResolveBuildRoot(repoRoot, effectiveBaseIntermediateOutputPath)
};

await ScriptHelpers.RunDotNetAsync(
    [
        "pack",
        packageProject,
        "-c",
        options.Configuration,
        "-o",
        packageOutput,
        "-v",
        "minimal",
        .. isolationArguments,
        "/nr:false",
        "-p:UseSharedCompilation=false"
    ],
    repoRoot,
    dotnetCliHome);

await ScriptHelpers.RunDotNetAsync(
    [
        "pack",
        elementPlusProject,
        "-c",
        options.Configuration,
        "-o",
        packageOutput,
        .. isolationArguments,
        "/nr:false",
        "-p:UseSharedCompilation=false"
    ],
    repoRoot,
    dotnetCliHome);

await ScriptHelpers.RunDotNetAsync(
    [
        "pack",
        vbenProject,
        "-c",
        options.Configuration,
        "-o",
        packageOutput,
        .. isolationArguments,
        "/nr:false",
        "-p:UseSharedCompilation=false"
    ],
    repoRoot,
    dotnetCliHome);

var packageInfo = ScriptHelpers.ResolveLatestPackage(packageOutput);
var restorePackagesPath = Path.Combine(restorePackagesRoot, $"{packageInfo.Version}-{packageInfo.Stamp}");

var buildArguments = new List<string>
{
    "build",
    hostProject,
    "-c",
    options.Configuration,
    "-t:Rebuild",
    "/m:1",
    "/p:BuildInParallel=false",
    $"-p:RestoreSources={packageOutput}",
    $"-p:RestorePackagesPath={restorePackagesPath}",
    "-p:RestoreForce=true",
    $"-p:JazorPackageVersion={packageInfo.Version}",
    "/nr:false",
    "-p:UseSharedCompilation=false"
};
buildArguments.AddRange(isolationArguments);

if (!string.IsNullOrWhiteSpace(options.JazorDir))
{
    buildArguments.Add("-p:JazorDir=" + ScriptHelpers.ResolvePath(repoRoot, options.JazorDir));
}

if (options.Bundle)
{
    buildArguments.Add("-p:JazorMode=release");
}

await ScriptHelpers.RunDotNetAsync(buildArguments, repoRoot, dotnetCliHome);

static string ResolveOptionalOverridePath(string? value, string fallback)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return fallback;
    }

    return Path.GetFullPath(value);
}

internal sealed record SampleBuildOptions(
    string Configuration,
    string? BaseOutputPath,
    string? BaseIntermediateOutputPath,
    string? JazorDir,
    bool Bundle)
{
    public static SampleBuildOptions Parse(IReadOnlyList<string> arguments)
    {
        var configuration = "Debug";
        string? baseOutputPath = null;
        string? baseIntermediateOutputPath = null;
        string? jazorDir = null;
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
                case "--base-output-path":
                case "-BaseOutputPath":
                    baseOutputPath = RequireValue(arguments, ref index, argument);
                    break;
                case "--base-intermediate-output-path":
                case "-BaseIntermediateOutputPath":
                    baseIntermediateOutputPath = RequireValue(arguments, ref index, argument);
                    break;
                case "--jazor-dir":
                case "-JazorDir":
                    jazorDir = RequireValue(arguments, ref index, argument);
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

        return new SampleBuildOptions(configuration, baseOutputPath, baseIntermediateOutputPath, jazorDir, bundle);
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
        Console.WriteLine("Usage: dotnet run --file samples/ECMAScript.Vben.ElementPlusInject/build-local.cs -- [options]");
        Console.WriteLine("Options:");
        Console.WriteLine("  --configuration <Debug|Release>");
        Console.WriteLine("  --base-output-path <path>");
        Console.WriteLine("  --base-intermediate-output-path <path>");
        Console.WriteLine("  --jazor-dir <path>");
        Console.WriteLine("  --bundle");
    }
}

internal sealed record PackageInfo(string Version, string Stamp);

internal static class ScriptHelpers
{
    public static string GetScriptPath([CallerFilePath] string path = "")
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new InvalidOperationException("Cannot resolve script path.");
        }

        return Path.GetFullPath(path);
    }

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

    public static PackageInfo ResolveLatestPackage(string packageOutput)
    {
        var packageFile = new DirectoryInfo(packageOutput)
            .EnumerateFiles("Jazor.*.nupkg", SearchOption.TopDirectoryOnly)
            .Where(file => !file.Name.EndsWith(".snupkg", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .FirstOrDefault();

        if (packageFile is null)
        {
            throw new InvalidOperationException("Packed Jazor package not found under '" + packageOutput + "'.");
        }

        var packageVersion = Path.GetFileNameWithoutExtension(packageFile.Name).Replace("Jazor.", "", StringComparison.Ordinal);
        var packageStamp = packageFile.LastWriteTimeUtc.ToString("yyyyMMddHHmmssffff", CultureInfo.InvariantCulture);
        return new PackageInfo(packageVersion, packageStamp);
    }

    public static void CleanDirectoryWithinRoots(string path, params string[] allowedRoots)
    {
        var fullPath = Path.GetFullPath(path);
        if (!allowedRoots.Any(root => fullPath.StartsWith(Path.GetFullPath(root), StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Refusing to delete a path outside allowed roots: " + fullPath);
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
            foreach (var (key, value) in environment)
            {
                startInfo.Environment[key] = value;
            }
        }

        var process = new Process
        {
            StartInfo = startInfo
        };
        process.Start();
        return process;
    }
}
