#!/usr/bin/env dotnet run

using System.Diagnostics;

var options = SampleBuildOptions.Parse(args);
var repoRoot = ScriptHelpers.FindRepositoryRoot(Directory.GetCurrentDirectory());
var sampleRoot = Path.Combine(repoRoot, "samples", "ECMAScript.VueDataUi.Dashboard");
var hostProject = Path.Combine(sampleRoot, "DataUi.Dashboard.Host", "DataUi.Dashboard.Host.csproj");
var packageOutput = Path.Combine(repoRoot, ".tmp", "nupkg-dataui-sample");
var artifactRoot = Path.Combine(repoRoot, ".tmp", "sample-dataui-dashboard", options.Configuration);
var restorePackagesPath = Path.Combine(repoRoot, ".tmp", "nuget-dataui-sample-packages");
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
        "--package", "dataui"
    ],
    repoRoot,
    dotnetCliHome);

var packageInfo = ScriptHelpers.ResolveJazorPackage(packageOutput);
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

ScriptHelpers.AssertGeneratedArtifacts(Path.Combine(artifactRoot, "jazor"));
Console.WriteLine("Generated dashboard artifacts: " + Path.Combine(artifactRoot, "jazor"));

internal sealed record SampleBuildOptions(string Configuration)
{
    public static SampleBuildOptions Parse(IReadOnlyList<string> arguments)
    {
        var configuration = "Debug";
        for (var index = 0; index < arguments.Count; index++)
        {
            switch (arguments[index])
            {
                case "--configuration":
                case "-c":
                    configuration = RequireValue(arguments, ref index);
                    break;
                case "--help":
                case "-h":
                    Console.WriteLine("Usage: dotnet run --file samples/ECMAScript.VueDataUi.Dashboard/build-local.cs -- [--configuration <Debug|Release>]");
                    Environment.Exit(0);
                    break;
                default:
                    throw new InvalidOperationException("Unsupported argument: " + arguments[index]);
            }
        }

        return new SampleBuildOptions(configuration);
    }

    private static string RequireValue(IReadOnlyList<string> arguments, ref int index)
    {
        if (++index >= arguments.Count)
            throw new InvalidOperationException("Missing value for " + arguments[index - 1] + ".");

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
        var fullRoot = Path.GetFullPath(repoRoot).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Refusing to delete a path outside the repository root: " + fullPath);

        if (Directory.Exists(fullPath))
            Directory.Delete(fullPath, recursive: true);
    }

    public static PackageInfo ResolveJazorPackage(string packageOutput)
    {
        var package = new DirectoryInfo(packageOutput)
            .EnumerateFiles("Jazor.*.nupkg", SearchOption.TopDirectoryOnly)
            .Where(static file => !file.Name.EndsWith(".snupkg", StringComparison.OrdinalIgnoreCase))
            // Jazor.Vue shares the prefix; NuGet package versions always begin with a digit.
            .Where(static file => file.Name.Length > "Jazor.".Length && char.IsDigit(file.Name["Jazor.".Length]))
            .OrderByDescending(static file => file.LastWriteTimeUtc)
            .FirstOrDefault()
            ?? throw new FileNotFoundException("Jazor package was not produced under " + packageOutput + ".");
        var prefix = "Jazor.";
        var suffix = ".nupkg";
        var version = package.Name[prefix.Length..^suffix.Length];

        return new PackageInfo(version, package.LastWriteTimeUtc.Ticks.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    public static void AssertGeneratedArtifacts(string artifactRoot)
    {
        var modulePath = Path.Combine(artifactRoot, "dashboard", "revenue.mjs");
        if (!File.Exists(modulePath))
            throw new FileNotFoundException("RazorVue dashboard module was not emitted.", modulePath);

        var moduleText = File.ReadAllText(modulePath);
        foreach (var import in new[]
                 {
                     "vue-data-ui/vue-ui-donut",
                     "vue-data-ui/vue-ui-gauge",
                     "vue-data-ui/vue-ui-sparkline"
                 })
        {
            if (!moduleText.Contains("from \"" + import + "\"", StringComparison.Ordinal))
                throw new InvalidOperationException("Dashboard module is missing chart import: " + import);
        }

        if (moduleText.Contains("from \"vue-data-ui\"", StringComparison.Ordinal))
            throw new InvalidOperationException("Dashboard must not import the aggregate vue-data-ui root entry.");

        var libraryRoot = Path.Combine(artifactRoot, "vendor", "vue-data-ui", "3.23.4", "dist");
        foreach (var entry in new[] { "vue-ui-donut.js", "vue-ui-gauge.js", "vue-ui-sparkline.js" })
        {
            var entryPath = Path.Combine(libraryRoot, "components", entry);
            if (!File.Exists(entryPath))
                throw new FileNotFoundException("Selected chart ESM entry was not materialized.", entryPath);
        }

        var stylePath = Path.Combine(libraryRoot, "style.css");
        if (!File.Exists(stylePath))
            throw new FileNotFoundException("vue-data-ui stylesheet was not materialized.", stylePath);
    }

    public static async Task RunDotNetAsync(
        IReadOnlyList<string> arguments,
        string workingDirectory,
        string dotnetCliHome)
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        startInfo.Environment["DOTNET_CLI_HOME"] = dotnetCliHome;
        startInfo.Environment["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1";
        startInfo.Environment["MSBUILDDISABLENODEREUSE"] = "1";
        startInfo.Environment["UseSharedCompilation"] = "false";
        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start dotnet.");
        await process.WaitForExitAsync();
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"dotnet {string.Join(' ', arguments)} failed with exit code {process.ExitCode}.");
    }
}
