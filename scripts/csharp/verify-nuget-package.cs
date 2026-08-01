#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.IO.Compression;
using System.Xml.Linq;

var options = VerifyNuGetPackageOptions.Parse(args);
var repoRoot = ScriptHelpers.RequireRepoRoot();
var outputDirectory = ScriptHelpers.ResolvePath(repoRoot, options.OutputDirectory);
var dotnetCliHome = Path.Combine(repoRoot, ".dotnet");

ScriptHelpers.DeleteDirectoryIfExists(outputDirectory);
Directory.CreateDirectory(outputDirectory);

var publishArguments = new List<string>
{
    "run",
    "--file",
    Path.Combine("scripts", "csharp", "publish-nuget.cs"),
        "--",
        "--configuration",
        options.Configuration,
    "--output-directory",
    outputDirectory,
    "--skip-push"
};

foreach (var packageId in options.PackageIds)
{
    publishArguments.Add("--package");
    publishArguments.Add(packageId);
}

if (!string.IsNullOrWhiteSpace(options.PackageVersion))
{
    publishArguments.Add("--package-version");
    publishArguments.Add(options.PackageVersion);
}

await ScriptHelpers.RunDotNetAsync(
    publishArguments,
    repoRoot,
    dotnetCliHome);

foreach (var packageId in options.PackageIds)
{
    var packageFile = FindPackageById(outputDirectory, packageId);
    VerifyPackage(packageFile, packageId, outputDirectory);
}

static FileInfo FindPackageById(string outputDirectory, string packageId)
{
    var matches = new DirectoryInfo(outputDirectory)
        .EnumerateFiles("*.nupkg", SearchOption.TopDirectoryOnly)
        .Where(static file => !file.Name.EndsWith(".snupkg", StringComparison.OrdinalIgnoreCase))
        .Where(file => HasNuspecEntry(file, packageId))
        .ToArray();

    return matches.Length switch
    {
        1 => matches[0],
        0 => throw new InvalidOperationException(
            "Expected package '" + packageId + "' was not found under: " + outputDirectory),
        _ => throw new InvalidOperationException(
            "Multiple packages with id '" + packageId + "' were found under: " + outputDirectory)
    };
}

static bool HasNuspecEntry(FileInfo packageFile, string packageId)
{
    using var archive = ZipFile.OpenRead(packageFile.FullName);
    return archive.GetEntry(packageId + ".nuspec") is not null;
}

static void VerifyPackage(FileInfo packageFile, string packageId, string outputDirectory)
{
    var packageVersion = Path.GetFileNameWithoutExtension(packageFile.Name)
        .Replace(packageId + ".", string.Empty, StringComparison.Ordinal);
    if (string.IsNullOrWhiteSpace(packageVersion) || packageVersion.Equals(Path.GetFileNameWithoutExtension(packageFile.Name), StringComparison.Ordinal))
    {
        throw new InvalidOperationException("Unable to resolve package version from produced package: " + packageFile.FullName);
    }

    var expandedDirectory = Path.Combine(outputDirectory, "expanded", packageId);
    ScriptHelpers.DeleteDirectoryIfExists(expandedDirectory);
    ZipFile.ExtractToDirectory(packageFile.FullName, expandedDirectory);

    foreach (var relativePath in GetRequiredPaths(packageId))
    {
        var fullPath = Path.Combine(expandedDirectory, relativePath);
        if (!File.Exists(fullPath) && !Directory.Exists(fullPath))
        {
            throw new InvalidOperationException("Required package entry is missing: " + relativePath.Replace(Path.DirectorySeparatorChar, '\\'));
        }
    }

    var nuspecPath = Path.Combine(expandedDirectory, packageId + ".nuspec");
    if (!File.Exists(nuspecPath))
    {
        throw new InvalidOperationException("Nuspec not found after package expansion: " + nuspecPath);
    }

    var nuspec = XDocument.Load(nuspecPath);
    var packageNamespace = nuspec.Root?.Name.Namespace ?? XNamespace.None;
    var metadata = nuspec.Root?.Element(packageNamespace + "metadata")
        ?? throw new InvalidOperationException("Package nuspec metadata was not found: " + nuspecPath);

    var metadataId = metadata.Element(packageNamespace + "id")?.Value ?? string.Empty;
    if (!metadataId.Equals(packageId, StringComparison.Ordinal))
    {
        throw new InvalidOperationException($"Unexpected package id in nuspec. Expected '{packageId}', got '{metadataId}'.");
    }

    var metadataVersion = metadata.Element(packageNamespace + "version")?.Value ?? string.Empty;
    if (!metadataVersion.Equals(packageVersion, StringComparison.Ordinal))
    {
        throw new InvalidOperationException($"Unexpected package version in nuspec. Expected '{packageVersion}', got '{metadataVersion}'.");
    }

    var license = metadata.Element(packageNamespace + "license");
    if (!string.Equals(license?.Attribute("type")?.Value, "file", StringComparison.Ordinal) ||
        !string.Equals(license?.Value, "LICENSE.txt", StringComparison.Ordinal))
    {
        throw new InvalidOperationException("Package license metadata is not configured as LICENSE.txt.");
    }

    var readme = metadata.Element(packageNamespace + "readme")?.Value ?? string.Empty;
    if (!readme.Equals("README.md", StringComparison.Ordinal))
    {
        throw new InvalidOperationException("Package readme metadata is not configured as README.md.");
    }

    Console.WriteLine("Package verification passed: " + packageFile.FullName);
}

static IReadOnlyList<string> GetRequiredPaths(string packageId)
{
    var commonPaths = new[]
    {
        "README.md",
        "LICENSE.txt",
        "NOTICE.txt"
    };

    return packageId switch
    {
        "Jazor" => commonPaths.Concat(
        [
            Path.Combine("buildTransitive", "Jazor.props"),
            Path.Combine("buildTransitive", "Jazor.targets"),
            Path.Combine("analyzers", "dotnet", "cs", "Jazor.Analyzer.dll"),
            Path.Combine("analyzers", "dotnet", "cs", "Jazor.Compiler.dll"),
            Path.Combine("lib", "net11.0", "ECMAScript.dll"),
            Path.Combine("lib", "net11.0", "Jazor.Compiler.dll"),
            Path.Combine("tools", "net11.0", "Jazor.Emit.dll"),
            Path.Combine("tools", "net11.0", "runtimes", "win-x64", "native", "deno.exe")
        ]).ToArray(),
        "Jazor.Vue" => commonPaths.Concat(
        [
            Path.Combine("analyzers", "dotnet", "cs", "Jazor.RazorVue.dll")
        ]).ToArray(),
        _ => throw new InvalidOperationException("Package verification is not defined for: " + packageId)
    };
}

internal sealed record VerifyNuGetPackageOptions(
    string Configuration,
    string OutputDirectory,
    string? PackageVersion,
    IReadOnlyList<string> PackageIds)
{
    public static VerifyNuGetPackageOptions Parse(IReadOnlyList<string> arguments)
    {
        var configuration = "Release";
        var outputDirectory = Path.Combine(".verify-out", "nuget-preflight");
        string? packageVersion = null;
        var packageIds = new List<string>();

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
                case "--package-version":
                    packageVersion = RequireValue(arguments, ref index, argument);
                    break;
                case "--package":
                    packageIds.Add(RequireValue(arguments, ref index, argument));
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

        if (packageIds.Count == 0)
        {
            packageIds.Add("Jazor");
        }

        return new VerifyNuGetPackageOptions(configuration, outputDirectory, packageVersion, packageIds);
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
        Console.WriteLine("Usage: dotnet run --file scripts/csharp/verify-nuget-package.cs -- [options]");
        Console.WriteLine("Options:");
        Console.WriteLine("  --configuration <Debug|Release>");
        Console.WriteLine("  --output-directory <path>");
        Console.WriteLine("  --package-version <semver>");
        Console.WriteLine("  --package <Jazor|Jazor.Vue>");
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

    public static void DeleteDirectoryIfExists(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        var fullPath = Path.GetFullPath(path);
        var rootPath = Path.GetPathRoot(fullPath) ?? string.Empty;
        if (string.Equals(fullPath.TrimEnd(Path.DirectorySeparatorChar), rootPath.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Refusing to delete a filesystem root path: " + fullPath);
        }

        Directory.Delete(fullPath, recursive: true);
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

    static Process StartProcess(
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
