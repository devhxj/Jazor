#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

var options = BuildOptions.Parse(args);
var repoRoot = ScriptHelpers.FindRepositoryRoot(Directory.GetCurrentDirectory());
var sampleRoot = Path.Combine(repoRoot, "samples", "RazorVue.Authoring");
var projectPath = Path.Combine(sampleRoot, "RazorVue.Authoring.csproj");
var publishScriptPath = Path.Combine(repoRoot, "scripts", "csharp", "publish-nuget.cs");
var workRoot = ScriptHelpers.ResolvePath(repoRoot, options.WorkRoot ?? Path.Combine(".tmp", "sample-build", "RazorVue.Authoring", options.Configuration));
var packageOutput = ScriptHelpers.ResolvePath(repoRoot, options.PackageOutput ?? Path.Combine(".tmp", "nupkg-sample", "RazorVue.Authoring"));
var sourceJazorRoot = ScriptHelpers.ResolvePath(repoRoot, options.SourceJazorRoot ?? Path.Combine(workRoot, "source-jazor"));
var packageJazorRoot = ScriptHelpers.ResolvePath(repoRoot, options.PackageJazorRoot ?? Path.Combine(workRoot, "package-jazor"));
var releaseJazorRoot = ScriptHelpers.ResolvePath(repoRoot, options.ReleaseJazorRoot ?? Path.Combine(workRoot, "release-jazor"));
var publishOutputRoot = ScriptHelpers.ResolvePath(repoRoot, options.PublishOutputRoot ?? Path.Combine(workRoot, "package-build-out"));
var publishIntermediateRoot = ScriptHelpers.ResolvePath(repoRoot, options.PublishIntermediateRoot ?? Path.Combine(workRoot, "package-build-obj"));
var sourceOutputRoot = ScriptHelpers.ResolvePath(repoRoot, options.SourceOutputRoot ?? Path.Combine(workRoot, "source-build-out"));
var sourceIntermediateRoot = ScriptHelpers.ResolvePath(repoRoot, options.SourceIntermediateRoot ?? Path.Combine(workRoot, "source-build-obj"));
var consumerOutputRoot = ScriptHelpers.ResolvePath(repoRoot, options.ConsumerOutputRoot ?? Path.Combine(workRoot, "consumer-build-out"));
var consumerIntermediateRoot = ScriptHelpers.ResolvePath(repoRoot, options.ConsumerIntermediateRoot ?? Path.Combine(workRoot, "consumer-build-obj"));

ScriptHelpers.SetCommonEnvironment(Path.Combine(repoRoot, ".dotnet"));
ScriptHelpers.EnsureInsideRepository(repoRoot, workRoot, packageOutput, sourceJazorRoot, packageJazorRoot, releaseJazorRoot,
    publishOutputRoot, publishIntermediateRoot, sourceOutputRoot, sourceIntermediateRoot, consumerOutputRoot, consumerIntermediateRoot);
foreach (var directory in new[]
{
    workRoot, packageOutput, sourceJazorRoot, packageJazorRoot, releaseJazorRoot,
    publishOutputRoot, publishIntermediateRoot, sourceOutputRoot, sourceIntermediateRoot,
    consumerOutputRoot, consumerIntermediateRoot
})
    ScriptHelpers.CleanDirectoryWithinRepo(directory, repoRoot);

var stopwatch = Stopwatch.StartNew();
PackageInfo? packageInfo = null;

if (!options.SourceOnly)
{
    var packArguments = new List<string>
    {
        "run", "--no-launch-profile", "--file", publishScriptPath, "--",
        "--configuration", options.Configuration,
        "--output-directory", packageOutput,
        "--skip-push",
        "--package", "jazor",
        "--package", "jazor-vue",
        "--package", "tdesign",
        "--package", "style",
        "--base-output-path", publishOutputRoot,
        "--base-intermediate-output-path", publishIntermediateRoot
    };
    if (!string.IsNullOrWhiteSpace(options.PackageVersion))
    {
        packArguments.Add("--package-version");
        packArguments.Add(options.PackageVersion);
    }

    await ScriptHelpers.RunProcessAsync("dotnet", packArguments, repoRoot);
    packageInfo = ScriptHelpers.ResolveLatestPackage(packageOutput);
    Console.WriteLine($"Local package version: {packageInfo.Value.Version}");
}

if (!options.PackageOnly)
{
    var sourceElapsed = Stopwatch.StartNew();
    await ScriptHelpers.RunProcessAsync(
        "dotnet",
        ScriptHelpers.BuildArguments(
            projectPath,
            options.Configuration,
            sourceJazorRoot,
            usePackages: false,
            packageInfo: null,
            restorePackagesPath: null,
            sourceFeed: null,
            sourceOutputRoot,
            sourceIntermediateRoot),
        repoRoot);
    sourceElapsed.Stop();
    var sourceSymbols = ScriptHelpers.CountAuthoringInternalSymbols(sampleRoot);
    Console.WriteLine($"Source authoring build: {sourceElapsed.Elapsed.TotalSeconds.ToString("F2", CultureInfo.InvariantCulture)}s; internal symbols: {sourceSymbols}");
}

if (!options.SourceOnly)
{
    var resolvedPackage = packageInfo ?? throw new InvalidOperationException("The local package version was not resolved.");
    var restorePackagesPath = Path.Combine(repoRoot, ".tmp", "nuget-sample-packages", $"{resolvedPackage.Version}-{resolvedPackage.Stamp}");

    if (!options.ReleaseOnly)
    {
        var consumerElapsed = Stopwatch.StartNew();
        await ScriptHelpers.RunProcessAsync(
            "dotnet",
            ScriptHelpers.BuildArguments(
                projectPath,
                options.Configuration,
                packageJazorRoot,
                usePackages: true,
                packageInfo: resolvedPackage,
                restorePackagesPath,
                packageOutput,
                consumerOutputRoot,
                consumerIntermediateRoot),
            repoRoot);
        consumerElapsed.Stop();
        Console.WriteLine($"Package consumer build: {consumerElapsed.Elapsed.TotalSeconds.ToString("F2", CultureInfo.InvariantCulture)}s");
    }

    if (!options.SkipRelease)
    {
        var releaseElapsed = Stopwatch.StartNew();
        var releaseArguments = ScriptHelpers.BuildArguments(
            projectPath,
            options.Configuration,
            releaseJazorRoot,
            usePackages: true,
            packageInfo: resolvedPackage,
            restorePackagesPath,
            packageOutput,
            consumerOutputRoot,
            consumerIntermediateRoot);
        releaseArguments.Add("-p:JazorMode=release");
        await ScriptHelpers.RunProcessAsync("dotnet", releaseArguments, repoRoot);
        releaseElapsed.Stop();
        Console.WriteLine($"Release artifact build: {releaseElapsed.Elapsed.TotalSeconds.ToString("F2", CultureInfo.InvariantCulture)}s");
    }
}

stopwatch.Stop();
var resultPath = Path.Combine(workRoot, "build-result.json");
Directory.CreateDirectory(workRoot);
var buildRecord = new JsonObject
{
    ["configuration"] = options.Configuration,
    ["packageVersion"] = packageInfo?.Version,
    ["packageStamp"] = packageInfo?.Stamp,
    ["sourceJazorRoot"] = sourceJazorRoot,
    ["packageJazorRoot"] = packageJazorRoot,
    ["releaseJazorRoot"] = releaseJazorRoot,
    ["packageOutput"] = packageOutput,
    ["elapsedSeconds"] = Math.Round(stopwatch.Elapsed.TotalSeconds, 2),
    ["authoringInternalSymbols"] = ScriptHelpers.CountAuthoringInternalSymbols(sampleRoot)
};
await File.WriteAllTextAsync(resultPath, buildRecord.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

Console.WriteLine($"RazorVue.Authoring local build passed in {stopwatch.Elapsed.TotalSeconds.ToString("F2", CultureInfo.InvariantCulture)}s.");
Console.WriteLine($"Build record: {resultPath}");

internal sealed record BuildOptions(
    string Configuration,
    bool SourceOnly,
    bool PackageOnly,
    bool ReleaseOnly,
    bool SkipRelease,
    string? PackageVersion,
    string? WorkRoot,
    string? PackageOutput,
    string? SourceJazorRoot,
    string? PackageJazorRoot,
    string? ReleaseJazorRoot,
    string? PublishOutputRoot,
    string? PublishIntermediateRoot,
    string? SourceOutputRoot,
    string? SourceIntermediateRoot,
    string? ConsumerOutputRoot,
    string? ConsumerIntermediateRoot)
{
    public static BuildOptions Parse(IReadOnlyList<string> arguments)
    {
        var configuration = "Release";
        var sourceOnly = false;
        var packageOnly = false;
        var releaseOnly = false;
        var skipRelease = false;
        string? packageVersion = null;
        string? workRoot = null;
        string? packageOutput = null;
        string? sourceJazorRoot = null;
        string? packageJazorRoot = null;
        string? releaseJazorRoot = null;
        string? publishOutputRoot = null;
        string? publishIntermediateRoot = null;
        string? sourceOutputRoot = null;
        string? sourceIntermediateRoot = null;
        string? consumerOutputRoot = null;
        string? consumerIntermediateRoot = null;

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
                case "--source-only":
                    sourceOnly = true;
                    break;
                case "--package-only":
                    packageOnly = true;
                    break;
                case "--release-only":
                    releaseOnly = true;
                    break;
                case "--skip-release":
                    skipRelease = true;
                    break;
                case "--package-version":
                    packageVersion = RequireValue(arguments, ref index, argument);
                    break;
                case "--work-root":
                    workRoot = RequireValue(arguments, ref index, argument);
                    break;
                case "--package-output":
                    packageOutput = RequireValue(arguments, ref index, argument);
                    break;
                case "--source-jazor-dir":
                    sourceJazorRoot = RequireValue(arguments, ref index, argument);
                    break;
                case "--jazor-dir":
                case "--package-jazor-dir":
                    packageJazorRoot = RequireValue(arguments, ref index, argument);
                    break;
                case "--release-jazor-dir":
                    releaseJazorRoot = RequireValue(arguments, ref index, argument);
                    break;
                case "--publish-output-root":
                    publishOutputRoot = RequireValue(arguments, ref index, argument);
                    break;
                case "--publish-intermediate-root":
                    publishIntermediateRoot = RequireValue(arguments, ref index, argument);
                    break;
                case "--source-output-root":
                    sourceOutputRoot = RequireValue(arguments, ref index, argument);
                    break;
                case "--source-intermediate-root":
                    sourceIntermediateRoot = RequireValue(arguments, ref index, argument);
                    break;
                case "--consumer-output-root":
                    consumerOutputRoot = RequireValue(arguments, ref index, argument);
                    break;
                case "--consumer-intermediate-root":
                    consumerIntermediateRoot = RequireValue(arguments, ref index, argument);
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

        if (sourceOnly && packageOnly)
            throw new InvalidOperationException("--source-only and --package-only cannot be combined.");
        if (releaseOnly)
            packageOnly = true;

        return new BuildOptions(
            configuration, sourceOnly, packageOnly, releaseOnly, skipRelease, packageVersion,
            workRoot, packageOutput, sourceJazorRoot, packageJazorRoot, releaseJazorRoot,
            publishOutputRoot, publishIntermediateRoot, sourceOutputRoot, sourceIntermediateRoot,
            consumerOutputRoot, consumerIntermediateRoot);
    }

    private static string RequireValue(IReadOnlyList<string> arguments, ref int index, string option)
    {
        var next = index + 1;
        if (next >= arguments.Count)
            throw new InvalidOperationException("Missing value for " + option + ".");
        index = next;
        return arguments[index];
    }

    private static void WriteUsage()
    {
        Console.WriteLine("Usage: dotnet run --file samples/RazorVue.Authoring/build-local.cs -- [options]");
        Console.WriteLine("  -c|--configuration <Debug|Release> (default Release)");
        Console.WriteLine("  --source-only | --package-only | --release-only");
        Console.WriteLine("  --skip-release");
        Console.WriteLine("  --package-version <version>");
        Console.WriteLine("  --jazor-dir <path>   package-consumer debug artifacts");
        Console.WriteLine("  --release-jazor-dir <path>   package-consumer release artifacts");
        Console.WriteLine("  --work-root <path>   isolated build record and intermediate roots");
    }
}

internal readonly record struct PackageInfo(string Version, string Stamp);

internal static class ScriptHelpers
{
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

    public static string ResolvePath(string repoRoot, string path)
        => Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path));

    public static void EnsureInsideRepository(string repoRoot, params string[] paths)
    {
        var fullRoot = Path.GetFullPath(repoRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var root = EnsureTrailingSeparator(fullRoot);
        foreach (var path in paths)
        {
            var full = Path.GetFullPath(path);
            if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(full.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), fullRoot, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("A build path must stay inside the repository: " + full);
        }
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
        EnsureInsideRepository(repoRoot, path);
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
    }

    public static List<string> BuildArguments(
        string projectPath,
        string configuration,
        string jazorRoot,
        bool usePackages,
        PackageInfo? packageInfo,
        string? restorePackagesPath,
        string? sourceFeed,
        string outputRoot,
        string intermediateRoot)
    {
        var arguments = new List<string>
        {
            "build", projectPath,
            "-c", configuration,
            "-t:Rebuild",
            "/m:1",
            "/p:BuildInParallel=false",
            "/nr:false",
            "-p:UseSharedCompilation=false",
            "-p:JazorDir=" + jazorRoot,
            "-p:JazorIsolatedBaseOutputRoot=" + EnsureTrailingSeparator(outputRoot),
            "-p:JazorIsolatedBaseIntermediateOutputRoot=" + EnsureTrailingSeparator(intermediateRoot),
            "-p:AuthoringUsePackages=" + (usePackages ? "true" : "false")
        };

        if (usePackages)
        {
            if (packageInfo is null || string.IsNullOrWhiteSpace(restorePackagesPath) || string.IsNullOrWhiteSpace(sourceFeed))
                throw new InvalidOperationException("Package builds require a local feed, package version, and restore path.");
            arguments.Add("-p:RestoreAdditionalProjectSources=" + sourceFeed);
            arguments.Add("-p:RestorePackagesPath=" + restorePackagesPath);
            arguments.Add("-p:RestoreForce=true");
            arguments.Add("-p:JazorPackageVersion=" + packageInfo.Value.Version);
        }

        return arguments;
    }

    public static PackageInfo ResolveLatestPackage(string packageOutput)
    {
        var packageFile = new DirectoryInfo(packageOutput)
            .EnumerateFiles("Jazor.*.nupkg", SearchOption.TopDirectoryOnly)
            .Where(static file => !file.Name.EndsWith(".snupkg", StringComparison.OrdinalIgnoreCase))
            .Where(static file => file.Name.Length > "Jazor.".Length && char.IsDigit(file.Name["Jazor.".Length]))
            .OrderByDescending(static file => file.LastWriteTimeUtc)
            .FirstOrDefault()
            ?? throw new FileNotFoundException("No Jazor package was produced under " + packageOutput);

        var version = Path.GetFileNameWithoutExtension(packageFile.Name)["Jazor.".Length..];
        return new PackageInfo(version, packageFile.LastWriteTimeUtc.ToString("yyyyMMddHHmmssffff", CultureInfo.InvariantCulture));
    }

    public static int CountAuthoringInternalSymbols(string sampleRoot)
    {
        var forbidden = new[] { "BuildRenderTree", "RenderTreeBuilder", "AdminInput", "AdminForm", "VueProp" };
        return Directory.EnumerateFiles(sampleRoot, "*.*", SearchOption.AllDirectories)
            .Where(static path => path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains(Path.DirectorySeparatorChar + "jazor" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            .Where(path => !string.Equals(Path.GetFileName(path), "build-local.cs", StringComparison.OrdinalIgnoreCase))
            .Where(path => !string.Equals(Path.GetFileName(path), "verify-smoke.cs", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadLines(path).Select(line => (Path: path, Line: line)))
            .Sum(item => forbidden.Count(token => item.Line.Contains(token, StringComparison.Ordinal)) +
                (IsRouteHostFraming(item.Path) || !item.Line.Contains("VueSlot", StringComparison.Ordinal) ? 0 : 1));
    }

    // The app-owned route host is the one explicit Vue framing boundary in this sample.
    // Razor pages themselves remain free of Vue slot types.
    private static bool IsRouteHostFraming(string path)
        => string.Equals(Path.GetFileName(path), "Bootstrap.cs", StringComparison.OrdinalIgnoreCase);

    public static async Task RunProcessAsync(string fileName, IReadOnlyList<string> arguments, string workingDirectory)
    {
        var startInfo = new ProcessStartInfo(fileName)
        {
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);
        startInfo.Environment["DOTNET_CLI_HOME"] = Environment.GetEnvironmentVariable("DOTNET_CLI_HOME") ?? string.Empty;
        startInfo.Environment["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1";
        startInfo.Environment["MSBUILDDISABLENODEREUSE"] = "1";
        startInfo.Environment["UseSharedCompilation"] = "false";

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Could not start " + fileName + ".");
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        var output = await stdout;
        var error = await stderr;
        if (!string.IsNullOrWhiteSpace(output))
            Console.Write(output);
        if (!string.IsNullOrWhiteSpace(error))
            Console.Error.Write(error);
        if (process.ExitCode != 0)
        {
            var tail = string.Join(Environment.NewLine, (output + Environment.NewLine + error).Split('\n').TakeLast(40));
            throw new InvalidOperationException($"{fileName} exited with code {process.ExitCode}." + Environment.NewLine + tail);
        }
    }

    private static string EnsureTrailingSeparator(string path)
        => path.EndsWith(Path.DirectorySeparatorChar) ? path : path + Path.DirectorySeparatorChar;
}
