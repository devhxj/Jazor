#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.Xml.Linq;

var options = PublishNuGetOptions.Parse(args);
var repoRoot = ScriptHelpers.RequireRepoRoot();
var packageProject = Path.Combine(repoRoot, "src", "Jazor", "Jazor.csproj");
var dotnetCliHome = Path.Combine(repoRoot, ".dotnet");

if (!File.Exists(packageProject))
{
    throw new FileNotFoundException("Package project not found: " + packageProject, packageProject);
}

var project = XDocument.Load(packageProject);
var packageId = GetProjectPropertyValue(project, "PackageId");
if (string.IsNullOrWhiteSpace(packageId))
{
    packageId = Path.GetFileNameWithoutExtension(packageProject);
}

var resolvedOutputDirectory = ScriptHelpers.ResolvePath(repoRoot, options.OutputDirectory);
Directory.CreateDirectory(resolvedOutputDirectory);

var isolationArguments = ScriptHelpers.GetIsolationArguments(repoRoot, options.BaseOutputPath, options.BaseIntermediateOutputPath);
var packArguments = new List<string>
{
    "pack",
    packageProject,
    "-c",
    options.Configuration,
    "-o",
    resolvedOutputDirectory,
    "-v",
    "minimal",
    "/nr:false",
    "-p:UseSharedCompilation=false"
};
packArguments.AddRange(isolationArguments);

if (options.NoBuild)
{
    AssertNoBuildPackInputsExist(packageProject, project, options.Configuration, repoRoot, options.BaseOutputPath);

    var restoreArguments = new List<string>
    {
        "restore",
        packageProject,
        "-v",
        "minimal",
        "/nr:false",
        "-p:UseSharedCompilation=false"
    };
    restoreArguments.AddRange(isolationArguments);
    await ScriptHelpers.RunDotNetAsync(restoreArguments, repoRoot, dotnetCliHome);

    packArguments.Add("--no-build");
    packArguments.Add("-p:JazorPreparePackageArtifacts=false");
}

await ScriptHelpers.RunDotNetAsync(packArguments, repoRoot, dotnetCliHome);

var packageFile = new DirectoryInfo(resolvedOutputDirectory)
    .EnumerateFiles($"{packageId}.*.nupkg", SearchOption.TopDirectoryOnly)
    .Where(static file => !file.Name.EndsWith(".snupkg", StringComparison.OrdinalIgnoreCase))
    .OrderByDescending(static file => file.LastWriteTimeUtc)
    .FirstOrDefault();

if (packageFile is null)
{
    throw new InvalidOperationException("Packed package not found under '" + resolvedOutputDirectory + "'.");
}

Console.WriteLine("Packed package: " + packageFile.FullName);

if (options.SkipPush)
{
    Console.WriteLine("SkipPush set. Package was not pushed.");
    return;
}

var apiKey = ResolveApiKey(options.ApiKey);
var pushArguments = new List<string>
{
    "nuget",
    "push",
    packageFile.FullName,
    "--source",
    options.Source,
    "--skip-duplicate"
};

if (!string.IsNullOrWhiteSpace(apiKey))
{
    pushArguments.Add("--api-key");
    pushArguments.Add(apiKey);
}

await ScriptHelpers.RunDotNetAsync(pushArguments, repoRoot, dotnetCliHome);
Console.WriteLine("Published package: " + packageFile.FullName);

static string ResolveApiKey(string? explicitApiKey)
{
    if (!string.IsNullOrWhiteSpace(explicitApiKey))
    {
        return explicitApiKey;
    }

    return Environment.GetEnvironmentVariable("NUGET_API_KEY")
        ?? Environment.GetEnvironmentVariable("NUGET_API_KEY", EnvironmentVariableTarget.User)
        ?? Environment.GetEnvironmentVariable("NUGET_API_KEY", EnvironmentVariableTarget.Machine)
        ?? string.Empty;
}

static string GetProjectPropertyValue(XDocument project, string name)
{
    var propertyValue = project.Root?
        .Elements("PropertyGroup")
        .Elements(name)
        .Select(static element => element.Value)
        .FirstOrDefault(static value => !string.IsNullOrWhiteSpace(value));

    return propertyValue ?? string.Empty;
}

static void AssertNoBuildPackInputsExist(
    string packageProjectPath,
    XDocument project,
    string configuration,
    string repoRoot,
    string? baseOutputPath)
{
    var roots = GetNoBuildPackInputRoots(packageProjectPath, configuration, repoRoot, baseOutputPath);
    var missingInputs = new List<string>();

    foreach (var noneItem in project.Root?
                 .Elements("ItemGroup")
                 .Elements("None")
                 .Select(static element => element.Attribute("Include")?.Value)
                 .Where(static include => !string.IsNullOrWhiteSpace(include))
                 .Cast<string>() ?? [])
    {
        if (noneItem.Contains("$([", StringComparison.Ordinal) ||
            noneItem.Contains("$(NuGetPackageRoot)", StringComparison.Ordinal))
        {
            continue;
        }

        if (!noneItem.StartsWith("..\\", StringComparison.Ordinal) &&
            !noneItem.Contains("$(JazorPackageBuildOutputRoot)", StringComparison.Ordinal))
        {
            continue;
        }

        var resolvedPath = ResolveLocalPackInputPath(
            roots.ProjectDirectory,
            noneItem,
            configuration,
            roots.PackageBuildOutputRoot);

        if (!File.Exists(resolvedPath) && !Directory.Exists(resolvedPath))
        {
            missingInputs.Add(resolvedPath);
        }
    }

    if (!Directory.Exists(roots.EmitPublishDirectory))
    {
        missingInputs.Add(roots.EmitPublishDirectory + " (Jazor.Emit publish output directory)");
    }
    else if (!Directory.EnumerateFiles(roots.EmitPublishDirectory, "*", SearchOption.AllDirectories).Any())
    {
        missingInputs.Add(roots.EmitPublishDirectory + " (Jazor.Emit publish output directory is empty)");
    }

    if (missingInputs.Count == 0)
    {
        return;
    }

    var details = string.Join(
        Environment.NewLine,
        missingInputs
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase)
            .Select(static path => " - " + path));

    throw new InvalidOperationException(
        "NoBuild was requested, but required package inputs are missing." + Environment.NewLine +
        details + Environment.NewLine +
        "Run publish-nuget.cs once without --no-build to prepare the full package artifacts.");
}

static NoBuildPackInputRoots GetNoBuildPackInputRoots(
    string packageProjectPath,
    string configuration,
    string repoRoot,
    string? baseOutputPath)
{
    var projectDirectory = Path.GetDirectoryName(packageProjectPath)
        ?? throw new InvalidOperationException("Package project directory could not be resolved: " + packageProjectPath);

    var packageBuildOutputRoot = !string.IsNullOrWhiteSpace(baseOutputPath)
        ? ScriptHelpers.ResolveBuildRoot(repoRoot, baseOutputPath)
        : ScriptHelpers.EnsureTrailingSeparator(Path.GetFullPath(Path.Combine(projectDirectory, "..")));

    var emitPublishDirectory = !string.IsNullOrWhiteSpace(baseOutputPath)
        ? Path.Combine(packageBuildOutputRoot, "Jazor.Emit", "bin", configuration, "net11.0", "publish")
        : Path.GetFullPath(Path.Combine(projectDirectory, "..", "Jazor.Emit", "bin", configuration, "net11.0", "publish"));

    return new NoBuildPackInputRoots(projectDirectory, packageBuildOutputRoot, emitPublishDirectory);
}

static string ResolveLocalPackInputPath(
    string projectDirectory,
    string include,
    string configuration,
    string packageBuildOutputRoot)
{
    var resolved = include
        .Replace("$(Configuration)", configuration, StringComparison.Ordinal)
        .Replace("$(MSBuildThisFileDirectory)", projectDirectory + Path.DirectorySeparatorChar, StringComparison.Ordinal)
        .Replace("$(JazorPackageBuildOutputRoot)", packageBuildOutputRoot, StringComparison.Ordinal);

    if (Path.IsPathRooted(resolved))
    {
        return Path.GetFullPath(resolved);
    }

    return Path.GetFullPath(Path.Combine(projectDirectory, resolved));
}

internal sealed record PublishNuGetOptions(
    string Configuration,
    string OutputDirectory,
    string Source,
    string ApiKey,
    string? BaseOutputPath,
    string? BaseIntermediateOutputPath,
    bool SkipPush,
    bool NoBuild)
{
    public static PublishNuGetOptions Parse(IReadOnlyList<string> arguments)
    {
        var configuration = "Release";
        var outputDirectory = Path.Combine(".artifacts", "packages");
        var source = "https://api.nuget.org/v3/index.json";
        var apiKey = string.Empty;
        string? baseOutputPath = null;
        string? baseIntermediateOutputPath = null;
        var skipPush = false;
        var noBuild = false;

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
                case "--output-directory":
                case "-OutputDirectory":
                case "-o":
                    outputDirectory = RequireValue(arguments, ref index, argument);
                    break;
                case "--source":
                case "-Source":
                    source = RequireValue(arguments, ref index, argument);
                    break;
                case "--api-key":
                case "-ApiKey":
                    apiKey = RequireValue(arguments, ref index, argument);
                    break;
                case "--base-output-path":
                case "-BaseOutputPath":
                    baseOutputPath = RequireValue(arguments, ref index, argument);
                    break;
                case "--base-intermediate-output-path":
                case "-BaseIntermediateOutputPath":
                    baseIntermediateOutputPath = RequireValue(arguments, ref index, argument);
                    break;
                case "--skip-push":
                case "-SkipPush":
                    skipPush = true;
                    break;
                case "--no-build":
                case "-NoBuild":
                    noBuild = true;
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

        return new PublishNuGetOptions(
            configuration,
            outputDirectory,
            source,
            apiKey,
            baseOutputPath,
            baseIntermediateOutputPath,
            skipPush,
            noBuild);
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
        Console.WriteLine("Usage: dotnet run --file scripts/csharp/publish-nuget.cs -- [options]");
        Console.WriteLine("Options:");
        Console.WriteLine("  --configuration <Debug|Release>");
        Console.WriteLine("  --output-directory <path>");
        Console.WriteLine("  --source <nuget-source>");
        Console.WriteLine("  --api-key <value>");
        Console.WriteLine("  --base-output-path <path>");
        Console.WriteLine("  --base-intermediate-output-path <path>");
        Console.WriteLine("  --skip-push");
        Console.WriteLine("  --no-build");
    }
}

internal sealed record NoBuildPackInputRoots(
    string ProjectDirectory,
    string PackageBuildOutputRoot,
    string EmitPublishDirectory);

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

    public static string ResolvePath(string repoRoot, string path)
    {
        return Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path));
    }

    public static string ResolveBuildRoot(string repoRoot, string path)
    {
        if (path.Contains("$(", StringComparison.Ordinal))
        {
            return path;
        }

        return EnsureTrailingSeparator(ResolvePath(repoRoot, path));
    }

    public static string EnsureTrailingSeparator(string path)
    {
        return path.EndsWith(Path.DirectorySeparatorChar)
            ? path
            : path + Path.DirectorySeparatorChar;
    }

    public static IReadOnlyList<string> GetIsolationArguments(string repoRoot, string? baseOutputPath, string? baseIntermediateOutputPath)
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

        return arguments;
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
                if (value is null)
                {
                    startInfo.Environment.Remove(key);
                }
                else
                {
                    startInfo.Environment[key] = value;
                }
            }
        }

        return Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start process: " + fileName);
    }
}
