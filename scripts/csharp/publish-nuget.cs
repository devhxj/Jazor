#!/usr/bin/env dotnet run

using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;

var options = PublishNuGetOptions.Parse(args);
var repoRoot = ScriptHelpers.RequireRepoRoot();
var dotnetCliHome = Path.Combine(repoRoot, ".dotnet");
var resolvedOutputDirectory = ScriptHelpers.ResolvePath(repoRoot, options.OutputDirectory);
Directory.CreateDirectory(resolvedOutputDirectory);

var selectedPackages = PackageCatalog.ResolveSelectedPackages(repoRoot, options.Packages);
Console.WriteLine("Selected packages: " + string.Join(", ", selectedPackages.Select(static package => package.PackageId)));

var isolationArguments = ScriptHelpers.GetIsolationArguments(repoRoot, options.BaseOutputPath, options.BaseIntermediateOutputPath);
var packageVersionArguments = ScriptHelpers.GetPackageVersionArguments(options.PackageVersion);
var packedPackageFiles = new List<FileInfo>(selectedPackages.Count);

foreach (var package in selectedPackages)
{
    if (!File.Exists(package.ProjectPath))
    {
        throw new FileNotFoundException("Package project not found: " + package.ProjectPath, package.ProjectPath);
    }

    var project = XDocument.Load(package.ProjectPath);
    if (options.NoBuild)
    {
        AssertNoBuildPackInputsExist(package, project, options.Configuration, repoRoot, options.BaseOutputPath);

        var restoreArguments = new List<string>
        {
            "restore",
            package.ProjectPath,
            "-v",
            "minimal",
            "/nr:false",
            "-p:UseSharedCompilation=false"
        };
        restoreArguments.AddRange(isolationArguments);
        restoreArguments.AddRange(packageVersionArguments);
        await ScriptHelpers.RunDotNetAsync(restoreArguments, repoRoot, dotnetCliHome);
    }

    var packArguments = new List<string>
    {
        "pack",
        package.ProjectPath,
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
    packArguments.AddRange(packageVersionArguments);

    if (options.NoBuild)
    {
        packArguments.Add("--no-build");
        packArguments.Add("-p:JazorVuePreparePackageArtifacts=false");

        if (package.DisableJazorPreparePackageArtifactsOnNoBuild)
        {
            packArguments.Add("-p:JazorPreparePackageArtifacts=false");
        }
    }

    await ScriptHelpers.RunDotNetAsync(packArguments, repoRoot, dotnetCliHome);

    var packageFile = GetMostRecentPackageFile(resolvedOutputDirectory, package.PackageId);
    packedPackageFiles.Add(packageFile);
    Console.WriteLine("Packed package: " + packageFile.FullName);
}

if (options.SkipPush)
{
    Console.WriteLine("SkipPush set. Packages were not pushed.");
    return;
}

var apiKey = ResolveApiKey(options.ApiKey);
foreach (var packageFile in packedPackageFiles
             .GroupBy(static file => file.FullName, StringComparer.OrdinalIgnoreCase)
             .Select(static group => group.First()))
{
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
}

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

static FileInfo GetMostRecentPackageFile(string outputDirectory, string packageId)
{
    return new DirectoryInfo(outputDirectory)
        .EnumerateFiles($"{packageId}.*.nupkg", SearchOption.TopDirectoryOnly)
        .Where(static file => !file.Name.EndsWith(".snupkg", StringComparison.OrdinalIgnoreCase))
        .OrderByDescending(static file => file.LastWriteTimeUtc)
        .FirstOrDefault()
        ?? throw new InvalidOperationException(
            "Packed package '" + packageId + "' was not found under '" + outputDirectory + "'.");
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
    PackageDefinition package,
    XDocument project,
    string configuration,
    string repoRoot,
    string? baseOutputPath)
{
    var roots = GetNoBuildPackInputRoots(package.ProjectPath, project, configuration, repoRoot, baseOutputPath);
    var missingInputs = new List<string>();
    var packageReadmeFile = GetProjectPropertyValue(project, "PackageReadmeFile");

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
            !noneItem.Contains("$(JazorPackageBuildOutputRoot)", StringComparison.Ordinal) &&
            !noneItem.Contains("$(JazorVuePackageBuildOutputRoot)", StringComparison.Ordinal))
        {
            continue;
        }

        var resolvedPath = ResolvePackInputPath(roots, noneItem, configuration, packageReadmeFile);
        if (!File.Exists(resolvedPath) && !Directory.Exists(resolvedPath))
        {
            missingInputs.Add(resolvedPath);
        }
    }

    var nuspecFile = GetProjectPropertyValue(project, "NuspecFile");
    if (!string.IsNullOrWhiteSpace(nuspecFile))
    {
        AssertNuspecNoBuildPackInputsExist(roots, nuspecFile, configuration, packageReadmeFile, missingInputs);
    }

    if (package.RequiresJazorEmitPublishOutput)
    {
        if (string.IsNullOrWhiteSpace(roots.EmitPublishDirectory) || !Directory.Exists(roots.EmitPublishDirectory))
        {
            missingInputs.Add((roots.EmitPublishDirectory ?? "(missing emit publish directory)") + " (Jazor.Emit publish output directory)");
        }
        else if (!Directory.EnumerateFiles(roots.EmitPublishDirectory, "*", SearchOption.AllDirectories).Any())
        {
            missingInputs.Add(roots.EmitPublishDirectory + " (Jazor.Emit publish output directory is empty)");
        }
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

static void AssertNuspecNoBuildPackInputsExist(
    NoBuildPackInputRoots roots,
    string nuspecFile,
    string configuration,
    string packageReadmeFile,
    List<string> missingInputs)
{
    var nuspecPath = Path.IsPathRooted(nuspecFile)
        ? Path.GetFullPath(nuspecFile)
        : Path.GetFullPath(Path.Combine(roots.ProjectDirectory, nuspecFile));

    if (!File.Exists(nuspecPath))
    {
        missingInputs.Add(nuspecPath);
        return;
    }

    var nuspec = XDocument.Load(nuspecPath);
    var ns = nuspec.Root?.Name.Namespace ?? XNamespace.None;

    foreach (var fileElement in nuspec.Root?
                 .Elements(ns + "files")
                 .Elements(ns + "file") ?? [])
    {
        var src = fileElement.Attribute("src")?.Value;
        if (string.IsNullOrWhiteSpace(src))
        {
            continue;
        }

        var resolvedPath = ResolvePackInputPath(roots, src, configuration, packageReadmeFile);
        if (!File.Exists(resolvedPath) && !Directory.Exists(resolvedPath))
        {
            missingInputs.Add(resolvedPath);
        }
    }
}

static NoBuildPackInputRoots GetNoBuildPackInputRoots(
    string packageProjectPath,
    XDocument project,
    string configuration,
    string repoRoot,
    string? baseOutputPath)
{
    var projectDirectory = Path.GetDirectoryName(packageProjectPath)
        ?? throw new InvalidOperationException("Package project directory could not be resolved: " + packageProjectPath);

    var packageBuildOutputRoot = !string.IsNullOrWhiteSpace(baseOutputPath)
        ? ScriptHelpers.ResolveBuildRoot(repoRoot, baseOutputPath)
        : ScriptHelpers.EnsureTrailingSeparator(Path.GetFullPath(Path.Combine(projectDirectory, "..")));

    var projectName = Path.GetFileNameWithoutExtension(packageProjectPath);
    var targetFramework = GetPrimaryTargetFramework(project);
    var buildOutputDirectory = !string.IsNullOrWhiteSpace(baseOutputPath)
        ? Path.Combine(packageBuildOutputRoot, projectName, "bin", configuration, targetFramework)
        : Path.GetFullPath(Path.Combine(projectDirectory, "bin", configuration, targetFramework));

    var emitPublishDirectory = !string.IsNullOrWhiteSpace(baseOutputPath)
        ? Path.Combine(packageBuildOutputRoot, "Jazor.Emit", "bin", configuration, "net11.0", "publish")
        : Path.GetFullPath(Path.Combine(projectDirectory, "..", "Jazor.Emit", "bin", configuration, "net11.0", "publish"));

    return new NoBuildPackInputRoots(
        projectDirectory,
        packageBuildOutputRoot,
        buildOutputDirectory,
        emitPublishDirectory);
}

static string GetPrimaryTargetFramework(XDocument project)
{
    var targetFramework = GetProjectPropertyValue(project, "TargetFramework");
    if (!string.IsNullOrWhiteSpace(targetFramework))
    {
        return targetFramework;
    }

    var targetFrameworks = GetProjectPropertyValue(project, "TargetFrameworks");
    var firstTargetFramework = targetFrameworks
        .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .FirstOrDefault();

    if (!string.IsNullOrWhiteSpace(firstTargetFramework))
    {
        return firstTargetFramework;
    }

    throw new InvalidOperationException("Cannot resolve TargetFramework/TargetFrameworks for project: " + project);
}

static string ResolvePackInputPath(
    NoBuildPackInputRoots roots,
    string include,
    string configuration,
    string packageReadmeFile)
{
    var resolved = include
        .Replace("$(Configuration)", configuration, StringComparison.Ordinal)
        .Replace("$(MSBuildThisFileDirectory)", roots.ProjectDirectory + Path.DirectorySeparatorChar, StringComparison.Ordinal)
        .Replace("$(JazorPackageBuildOutputRoot)", roots.PackageBuildOutputRoot, StringComparison.Ordinal)
        .Replace("$(JazorVuePackageBuildOutputRoot)", roots.PackageBuildOutputRoot, StringComparison.Ordinal)
        .Replace("$buildOutputDir$", ScriptHelpers.EnsureTrailingSeparator(roots.BuildOutputDirectory), StringComparison.Ordinal)
        .Replace("$projectDir$", roots.ProjectDirectory + Path.DirectorySeparatorChar, StringComparison.Ordinal)
        .Replace("$packageReadmeFile$", packageReadmeFile, StringComparison.Ordinal);

    if (Path.IsPathRooted(resolved))
    {
        return Path.GetFullPath(resolved);
    }

    return Path.GetFullPath(Path.Combine(roots.ProjectDirectory, resolved));
}

internal sealed record PublishNuGetOptions(
    string Configuration,
    string OutputDirectory,
    string Source,
    string ApiKey,
    string? BaseOutputPath,
    string? BaseIntermediateOutputPath,
    string? PackageVersion,
    IReadOnlyList<string> Packages,
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
        string? packageVersion = null;
        var packages = new List<string>();
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
                case "--package-version":
                case "-PackageVersion":
                    packageVersion = RequireValue(arguments, ref index, argument);
                    break;
                case "--package":
                case "-Package":
                    packages.Add(RequireValue(arguments, ref index, argument));
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
            packageVersion,
            packages,
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
        Console.WriteLine("  --package-version <semver>");
        Console.WriteLine("  --package <jazor|jazor-vue|jazor-style|admin|pinia|pinia-testing|vueroute|vuetify|tdesign|elementplus|PackageId>");
        Console.WriteLine("    Default package set: Jazor, Jazor.Vue, Jazor.Style, Jazor.Admin, ECMAScript.Pinia, ECMAScript.Pinia.Testing, ECMAScript.VueRoute, ECMAScript.Vuetify, ECMAScript.TDesign");
        Console.WriteLine("  --skip-push");
        Console.WriteLine("  --no-build");
    }
}

internal sealed record PackageDefinition(
    string PackageId,
    string ProjectPath,
    bool RequiresJazorEmitPublishOutput,
    bool DisableJazorPreparePackageArtifactsOnNoBuild);

internal sealed record NoBuildPackInputRoots(
    string ProjectDirectory,
    string PackageBuildOutputRoot,
    string BuildOutputDirectory,
    string? EmitPublishDirectory);

internal static class PackageCatalog
{
    private static readonly string[] DefaultPublicPackageIds =
    [
        "Jazor",
        "Jazor.Vue",
        "Jazor.Style",
        "Jazor.Admin",
        "ECMAScript.Pinia",
        "ECMAScript.Pinia.Testing",
        "ECMAScript.VueRoute",
        "ECMAScript.Vuetify",
        "ECMAScript.TDesign"
    ];

    private static readonly Dictionary<string, string> PackageAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["jazor"] = "Jazor",
        ["Jazor"] = "Jazor",
        ["jazor-vue"] = "Jazor.Vue",
        ["jazor.vue"] = "Jazor.Vue",
        ["Jazor.Vue"] = "Jazor.Vue",
        ["jazor-style"] = "Jazor.Style",
        ["jazor.style"] = "Jazor.Style",
        ["Jazor.Style"] = "Jazor.Style",
        ["pinia"] = "ECMAScript.Pinia",
        ["ECMAScript.Pinia"] = "ECMAScript.Pinia",
        ["pinia-testing"] = "ECMAScript.Pinia.Testing",
        ["ECMAScript.Pinia.Testing"] = "ECMAScript.Pinia.Testing",
        ["vueroute"] = "ECMAScript.VueRoute",
        ["ECMAScript.VueRoute"] = "ECMAScript.VueRoute",
        ["vuetify"] = "ECMAScript.Vuetify",
        ["ECMAScript.Vuetify"] = "ECMAScript.Vuetify",
        ["admin"] = "Jazor.Admin",
        ["Jazor.Admin"] = "Jazor.Admin",
        ["elementplus"] = "ECMAScript.ElementPlus",
        ["ECMAScript.ElementPlus"] = "ECMAScript.ElementPlus",
        ["tdesign"] = "ECMAScript.TDesign",
        ["ECMAScript.TDesign"] = "ECMAScript.TDesign"
    };

    public static IReadOnlyList<PackageDefinition> ResolveSelectedPackages(string repoRoot, IReadOnlyList<string> packageSelectors)
    {
        var catalog = CreateCatalog(repoRoot);
        var resolvedPackageIds = packageSelectors.Count == 0
            ? DefaultPublicPackageIds
            : packageSelectors.Select(NormalizePackageSelector).ToArray();

        var selectedPackages = new List<PackageDefinition>(resolvedPackageIds.Length);
        foreach (var packageId in resolvedPackageIds)
        {
            if (!catalog.TryGetValue(packageId, out var package))
            {
                throw new InvalidOperationException(
                    "Unsupported package selector '" + packageId + "'. Supported packages: " +
                    string.Join(", ", DefaultPublicPackageIds));
            }

            if (selectedPackages.Any(existing => existing.PackageId.Equals(package.PackageId, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            selectedPackages.Add(package);
        }

        return selectedPackages;
    }

    private static string NormalizePackageSelector(string selector)
    {
        var trimmedSelector = selector.Trim();
        if (PackageAliases.TryGetValue(trimmedSelector, out var packageId))
        {
            return packageId;
        }

        throw new InvalidOperationException(
            "Unsupported package selector: " + selector + ". Supported selectors: " +
            "jazor, jazor-vue, jazor-style, admin, pinia, pinia-testing, vueroute, vuetify, elementplus, tdesign.");
    }

    private static Dictionary<string, PackageDefinition> CreateCatalog(string repoRoot)
    {
        return new Dictionary<string, PackageDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["Jazor"] = new(
                "Jazor",
                Path.Combine(repoRoot, "src", "Jazor", "Jazor.csproj"),
                RequiresJazorEmitPublishOutput: true,
                DisableJazorPreparePackageArtifactsOnNoBuild: true),
            ["Jazor.Vue"] = new(
                "Jazor.Vue",
                Path.Combine(repoRoot, "src", "Jazor.Vue", "Jazor.Vue.csproj"),
                RequiresJazorEmitPublishOutput: false,
                DisableJazorPreparePackageArtifactsOnNoBuild: false),
            ["Jazor.Style"] = new(
                "Jazor.Style",
                Path.Combine(repoRoot, "src", "Jazor.Style", "Jazor.Style.csproj"),
                RequiresJazorEmitPublishOutput: false,
                DisableJazorPreparePackageArtifactsOnNoBuild: false),
            ["ECMAScript.Pinia"] = new(
                "ECMAScript.Pinia",
                Path.Combine(repoRoot, "src", "ECMAScript.Pinia", "ECMAScript.Pinia.csproj"),
                RequiresJazorEmitPublishOutput: false,
                DisableJazorPreparePackageArtifactsOnNoBuild: false),
            ["ECMAScript.Pinia.Testing"] = new(
                "ECMAScript.Pinia.Testing",
                Path.Combine(repoRoot, "src", "ECMAScript.Pinia.Testing", "ECMAScript.Pinia.Testing.csproj"),
                RequiresJazorEmitPublishOutput: false,
                DisableJazorPreparePackageArtifactsOnNoBuild: false),
            ["ECMAScript.VueRoute"] = new(
                "ECMAScript.VueRoute",
                Path.Combine(repoRoot, "src", "ECMAScript.VueRoute", "ECMAScript.VueRoute.csproj"),
                RequiresJazorEmitPublishOutput: false,
                DisableJazorPreparePackageArtifactsOnNoBuild: false),
            ["ECMAScript.Vuetify"] = new(
                "ECMAScript.Vuetify",
                Path.Combine(repoRoot, "src", "ECMAScript.Vuetify", "ECMAScript.Vuetify.csproj"),
                RequiresJazorEmitPublishOutput: false,
                DisableJazorPreparePackageArtifactsOnNoBuild: false),
            ["Jazor.Admin"] = new(
                "Jazor.Admin",
                Path.Combine(repoRoot, "src", "Jazor.Admin", "Jazor.Admin.csproj"),
                RequiresJazorEmitPublishOutput: false,
                DisableJazorPreparePackageArtifactsOnNoBuild: false),
            ["ECMAScript.ElementPlus"] = new(
                "ECMAScript.ElementPlus",
                Path.Combine(repoRoot, "src", "ECMAScript.ElementPlus", "ECMAScript.ElementPlus.csproj"),
                RequiresJazorEmitPublishOutput: false,
                DisableJazorPreparePackageArtifactsOnNoBuild: false),
            ["ECMAScript.TDesign"] = new(
                "ECMAScript.TDesign",
                Path.Combine(repoRoot, "src", "ECMAScript.TDesign", "ECMAScript.TDesign.csproj"),
                RequiresJazorEmitPublishOutput: false,
                DisableJazorPreparePackageArtifactsOnNoBuild: false)
        };
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

    public static IReadOnlyList<string> GetPackageVersionArguments(string? packageVersion)
    {
        if (string.IsNullOrWhiteSpace(packageVersion))
        {
            return [];
        }

        return
        [
            "-p:MinVerVersionOverride=" + packageVersion,
            "-p:JazorPackageVersion=" + packageVersion
        ];
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
