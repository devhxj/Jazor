#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;

var options = SampleBuildOptions.Parse(args);
var scriptPath = ScriptHelpers.GetScriptPath();
var adminRoot = Path.GetDirectoryName(scriptPath)
    ?? throw new InvalidOperationException("Cannot resolve JazorAdmin project root.");
var repoRoot = ScriptHelpers.FindRepositoryRoot(adminRoot);
var projectPath = Path.Combine(adminRoot, "JazorAdmin.csproj");
var injectProjectPath = Path.Combine(adminRoot, "InjectSmoke", "JazorAdmin.InjectSmoke.csproj");
var publishScriptPath = Path.Combine(repoRoot, "scripts", "csharp", "publish-nuget.cs");
var packageOutput = Path.Combine(repoRoot, ".tmp", "nupkg-sample");
var dotnetCliHome = Path.Combine(repoRoot, ".dotnet");

ScriptHelpers.SetCommonEnvironment(dotnetCliHome);
ScriptHelpers.CleanDirectoryWithinRepo(packageOutput, repoRoot);
if (!string.IsNullOrWhiteSpace(options.JazorDir))
{
    ScriptHelpers.CleanDirectoryWithinRepo(ScriptHelpers.ResolvePath(repoRoot, options.JazorDir), repoRoot);
}
if (!string.IsNullOrWhiteSpace(options.InjectJazorDir))
{
    ScriptHelpers.CleanDirectoryWithinRepo(ScriptHelpers.ResolvePath(repoRoot, options.InjectJazorDir), repoRoot);
}

var isolation = ScriptHelpers.GetIsolationArguments(options, repoRoot);
var packArguments = new List<string>
{
    "run",
    "--file",
    publishScriptPath,
    "--",
    "--configuration", options.Configuration,
    "--output-directory", packageOutput,
    "--skip-push",
    "--package", "jazor",
    "--package", "jazor-vue",
    "--package", "style",
    "--package", "vueroute",
    "--package", "admin",
    "--package", "tdesign"
};
if (!string.IsNullOrWhiteSpace(options.PackageVersion))
{
    packArguments.Add("--package-version");
    packArguments.Add(options.PackageVersion);
}
packArguments.AddRange(isolation.PublishArguments);
await ScriptHelpers.RunDotNetAsync(packArguments, repoRoot, dotnetCliHome);

var packageInfo = ScriptHelpers.ResolveLatestPackage(packageOutput);
var restorePackagesPath = Path.Combine(repoRoot, ".tmp", "nuget-sample-packages", $"{packageInfo.Version}-{packageInfo.Stamp}");
var buildArguments = new List<string>
{
    "build",
    projectPath,
    "-c", options.Configuration,
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
buildArguments.AddRange(isolation.BuildArguments);

if (!string.IsNullOrWhiteSpace(options.JazorDir))
{
    buildArguments.Add("-p:JazorDir=" + ScriptHelpers.ResolvePath(repoRoot, options.JazorDir));
}

await ScriptHelpers.RunDotNetAsync(buildArguments, repoRoot, dotnetCliHome);

var injectBuildArguments = new List<string>
{
    "build",
    injectProjectPath,
    "-c", options.Configuration,
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
injectBuildArguments.AddRange(isolation.BuildArguments);

if (!string.IsNullOrWhiteSpace(options.InjectJazorDir))
{
    injectBuildArguments.Add("-p:JazorDir=" + ScriptHelpers.ResolvePath(repoRoot, options.InjectJazorDir));
}

await ScriptHelpers.RunDotNetAsync(injectBuildArguments, repoRoot, dotnetCliHome);

internal sealed record SampleBuildOptions(
    string Configuration,
    string? BaseOutputPath,
    string? BaseIntermediateOutputPath,
    string? JazorDir,
    string? InjectJazorDir,
    string? PackageVersion)
{
    public static SampleBuildOptions Parse(IReadOnlyList<string> arguments)
    {
        var configuration = "Debug";
        string? baseOutputPath = null;
        string? baseIntermediateOutputPath = null;
        string? jazorDir = null;
        string? injectJazorDir = null;
        string? packageVersion = null;

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
                case "--inject-jazor-dir":
                case "-InjectJazorDir":
                    injectJazorDir = RequireValue(arguments, ref index, argument);
                    break;
                case "--package-version":
                case "-PackageVersion":
                    packageVersion = RequireValue(arguments, ref index, argument);
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

        return new SampleBuildOptions(
            configuration,
            baseOutputPath,
            baseIntermediateOutputPath,
            jazorDir,
            injectJazorDir,
            packageVersion);
    }

    private static string RequireValue(IReadOnlyList<string> arguments, ref int index, string option)
    {
        var nextIndex = index + 1;
        if (nextIndex >= arguments.Count)
            throw new InvalidOperationException("Missing value for " + option + ".");

        index = nextIndex;
        return arguments[index];
    }

    private static void WriteUsage()
    {
        Console.WriteLine("Usage: dotnet run --no-launch-profile --file samples/JazorAdmin/build-local.cs -- [options]");
        Console.WriteLine("Options:");
        Console.WriteLine("  --configuration <Debug|Release>");
        Console.WriteLine("  --base-output-path <path>");
        Console.WriteLine("  --base-intermediate-output-path <path>");
        Console.WriteLine("  --jazor-dir <path>");
        Console.WriteLine("  --inject-jazor-dir <path>");
        Console.WriteLine("  --package-version <semver>");
    }
}

internal sealed record PackageInfo(string Version, string Stamp);

internal readonly record struct IsolationArguments(
    IReadOnlyList<string> PublishArguments,
    IReadOnlyList<string> BuildArguments);

internal static class ScriptHelpers
{
    public static string GetScriptPath([CallerFilePath] string path = "")
        => string.IsNullOrWhiteSpace(path)
            ? throw new InvalidOperationException("Cannot resolve script path.")
            : Path.GetFullPath(path);

    public static string FindRepositoryRoot(string startDirectory)
    {
        var current = new DirectoryInfo(Path.GetFullPath(startDirectory));
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Jazor.slnx")))
                return current.FullName;

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
        => Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path));

    public static IsolationArguments GetIsolationArguments(SampleBuildOptions options, string repoRoot)
    {
        var publish = new List<string>();
        var build = new List<string>();
        if (!string.IsNullOrWhiteSpace(options.BaseOutputPath))
        {
            var root = ResolveBuildRoot(repoRoot, options.BaseOutputPath);
            publish.Add("--base-output-path");
            publish.Add(root);
            build.Add("-p:JazorIsolatedBaseOutputRoot=" + root);
        }

        if (!string.IsNullOrWhiteSpace(options.BaseIntermediateOutputPath))
        {
            var root = ResolveBuildRoot(repoRoot, options.BaseIntermediateOutputPath);
            publish.Add("--base-intermediate-output-path");
            publish.Add(root);
            build.Add("-p:JazorIsolatedBaseIntermediateOutputRoot=" + root);
        }

        return new IsolationArguments(publish, build);
    }

    public static PackageInfo ResolveLatestPackage(string packageOutput)
    {
        var packageFile = new DirectoryInfo(packageOutput)
            .EnumerateFiles("Jazor.*.nupkg", SearchOption.TopDirectoryOnly)
            .Where(static file => !file.Name.EndsWith(".snupkg", StringComparison.OrdinalIgnoreCase))
            .Where(static file =>
            {
                const string prefix = "Jazor.";
                return file.Name.Length > prefix.Length && char.IsDigit(file.Name[prefix.Length]);
            })
            .OrderByDescending(static file => file.LastWriteTimeUtc)
            .FirstOrDefault()
            ?? throw new InvalidOperationException("Packed Jazor package not found under '" + packageOutput + "'.");

        var version = Path.GetFileNameWithoutExtension(packageFile.Name).Replace("Jazor.", string.Empty, StringComparison.Ordinal);
        var stamp = packageFile.LastWriteTimeUtc.ToString("yyyyMMddHHmmssffff", CultureInfo.InvariantCulture);
        return new PackageInfo(version, stamp);
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

    public static async Task RunDotNetAsync(
        IReadOnlyList<string> arguments,
        string workdir,
        string dotnetCliHome,
        CancellationToken cancellationToken = default)
    {
        using var process = StartProcess("dotnet", arguments, workdir, dotnetCliHome);
        await process.WaitForExitAsync(cancellationToken);
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"Process failed with exit code {process.ExitCode}: dotnet {string.Join(' ', arguments)}");
    }

    private static Process StartProcess(
        string fileName,
        IReadOnlyList<string> arguments,
        string workdir,
        string dotnetCliHome)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
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

        var process = new Process { StartInfo = startInfo };
        process.Start();
        return process;
    }

    private static string ResolveBuildRoot(string repoRoot, string path)
    {
        var resolved = ResolvePath(repoRoot, path);
        return resolved.EndsWith(Path.DirectorySeparatorChar)
            ? resolved
            : resolved + Path.DirectorySeparatorChar;
    }
}
