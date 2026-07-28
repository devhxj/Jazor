#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.IO.Compression;
using System.Xml.Linq;

var options = VerifyNuGetPackageOptions.Parse(args);
var repoRoot = ScriptHelpers.RequireRepoRoot();
var packageProject = Path.Combine(repoRoot, "src", "Jazor", "Jazor.csproj");
var outputDirectory = ScriptHelpers.ResolvePath(repoRoot, options.OutputDirectory);
var dotnetCliHome = Path.Combine(repoRoot, ".dotnet");

if (!File.Exists(packageProject))
{
    throw new FileNotFoundException("Package project not found: " + packageProject, packageProject);
}

var project = XDocument.Load(packageProject);
var packageId = project.Root?
    .Elements("PropertyGroup")
    .Elements("PackageId")
    .Select(static element => element.Value)
    .FirstOrDefault(static value => !string.IsNullOrWhiteSpace(value));
packageId ??= Path.GetFileNameWithoutExtension(packageProject);

ScriptHelpers.DeleteDirectoryIfExists(outputDirectory);
Directory.CreateDirectory(outputDirectory);

await ScriptHelpers.RunDotNetAsync(
    [
        "run",
        "--file",
        Path.Combine("scripts", "csharp", "publish-nuget.cs"),
        "--",
        "--configuration",
        options.Configuration,
        "--output-directory",
        outputDirectory,
        "--skip-push"
    ],
    repoRoot,
    dotnetCliHome);

var packageFile = new DirectoryInfo(outputDirectory)
    .EnumerateFiles($"{packageId}.*.nupkg", SearchOption.TopDirectoryOnly)
    .Where(static file => !file.Name.EndsWith(".snupkg", StringComparison.OrdinalIgnoreCase))
    .OrderByDescending(static file => file.LastWriteTimeUtc)
    .FirstOrDefault();

if (packageFile is null)
{
    throw new InvalidOperationException("Expected package not found under: " + outputDirectory);
}

var packageVersion = Path.GetFileNameWithoutExtension(packageFile.Name)
    .Replace(packageId + ".", string.Empty, StringComparison.Ordinal);
if (string.IsNullOrWhiteSpace(packageVersion) || packageVersion.Equals(Path.GetFileNameWithoutExtension(packageFile.Name), StringComparison.Ordinal))
{
    throw new InvalidOperationException("Unable to resolve package version from produced package: " + packageFile.FullName);
}

var expandedDirectory = Path.Combine(outputDirectory, "expanded");
ScriptHelpers.DeleteDirectoryIfExists(expandedDirectory);
ZipFile.ExtractToDirectory(packageFile.FullName, expandedDirectory);

var requiredPaths = new[]
{
    "README.md",
    "LICENSE.txt",
    "NOTICE.txt",
    Path.Combine("buildTransitive", "Jazor.props"),
    Path.Combine("buildTransitive", "Jazor.targets"),
    Path.Combine("analyzers", "dotnet", "cs", "Jazor.Analyzer.dll"),
    Path.Combine("analyzers", "dotnet", "cs", "Jazor.Compiler.dll"),
    Path.Combine("lib", "net11.0", "ECMAScript.dll"),
    Path.Combine("lib", "net11.0", "Jazor.Compiler.dll"),
    Path.Combine("tools", "net11.0", "Jazor.Emit.dll"),
    Path.Combine("tools", "net11.0", "runtimes", "win-x64", "native", "deno.exe")
};

foreach (var relativePath in requiredPaths)
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
var metadata = nuspec.Root?.Element("metadata")
    ?? throw new InvalidOperationException("Package nuspec metadata was not found: " + nuspecPath);

var metadataId = metadata.Element("id")?.Value ?? string.Empty;
if (!metadataId.Equals(packageId, StringComparison.Ordinal))
{
    throw new InvalidOperationException($"Unexpected package id in nuspec. Expected '{packageId}', got '{metadataId}'.");
}

var metadataVersion = metadata.Element("version")?.Value ?? string.Empty;
if (!metadataVersion.Equals(packageVersion, StringComparison.Ordinal))
{
    throw new InvalidOperationException($"Unexpected package version in nuspec. Expected '{packageVersion}', got '{metadataVersion}'.");
}

var license = metadata.Element("license");
if (!string.Equals(license?.Attribute("type")?.Value, "file", StringComparison.Ordinal) ||
    !string.Equals(license?.Value, "LICENSE.txt", StringComparison.Ordinal))
{
    throw new InvalidOperationException("Package license metadata is not configured as LICENSE.txt.");
}

var readme = metadata.Element("readme")?.Value ?? string.Empty;
if (!readme.Equals("README.md", StringComparison.Ordinal))
{
    throw new InvalidOperationException("Package readme metadata is not configured as README.md.");
}

Console.WriteLine("Package verification passed: " + packageFile.FullName);

internal sealed record VerifyNuGetPackageOptions(string Configuration, string OutputDirectory)
{
    public static VerifyNuGetPackageOptions Parse(IReadOnlyList<string> arguments)
    {
        var configuration = "Release";
        var outputDirectory = Path.Combine(".verify-out", "nuget-preflight");

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
                case "--help":
                case "-h":
                    WriteUsage();
                    Environment.Exit(0);
                    break;
                default:
                    throw new InvalidOperationException("Unsupported argument: " + argument);
            }
        }

        return new VerifyNuGetPackageOptions(configuration, outputDirectory);
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
