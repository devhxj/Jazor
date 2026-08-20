#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.IO.Compression;
using System.Xml.Linq;

var options = VerificationOptions.Parse(args);
var repoRoot = ScriptHelpers.RequireRepoRoot();
var sourceWikiRoot = Path.Combine(repoRoot, "samples", "Wiki");
var packageRoot = ScriptHelpers.ResolvePath(repoRoot, options.PackageSource);
var workRoot = Path.Combine(repoRoot, ".tmp", "windows-spa-release-" + Environment.ProcessId);
var consumerRoot = Path.Combine(workRoot, "Wiki");
var publishRoot = Path.Combine(workRoot, "publish");
var restorePackagesRoot = Path.Combine(workRoot, "packages");
var dotnetCliHome = Path.Combine(repoRoot, ".dotnet");
var browserVerificationScript = Path.Combine(repoRoot, "scripts", "csharp", "wiki-verify-browser.cs");

Console.WriteLine("Starting Windows SPA release consumer verification.");

try
{
    ScriptHelpers.DeleteDirectoryWithinRepo(repoRoot, workRoot);
    Directory.CreateDirectory(workRoot);

    if (options.PackPackages)
    {
        ScriptHelpers.DeleteDirectoryWithinRepo(repoRoot, packageRoot);
        Directory.CreateDirectory(packageRoot);

        await ScriptHelpers.RunDotNetAsync(
            [
                "run",
                "--file",
                Path.Combine("scripts", "csharp", "publish-nuget.cs"),
                "--",
                "--configuration",
                "Release",
                "--output-directory",
                packageRoot,
                "--package",
                "jazor",
                "--package",
                "jazor-vue",
                "--package",
                "style",
                "--skip-push"
            ],
            repoRoot,
            dotnetCliHome);
    }

    var packageVersion = PackageVerifier.ResolveSharedPackageVersion(packageRoot);
    PackageVerifier.VerifyRequiredPackages(packageRoot, packageVersion);

    ScriptHelpers.CopyWikiConsumerProps(repoRoot, workRoot);
    ScriptHelpers.CopyWikiSource(sourceWikiRoot, consumerRoot);
    await ScriptHelpers.MaterializeWikiDocsCatalogAsync(repoRoot, consumerRoot, dotnetCliHome);
    ScriptHelpers.AssertDetachedConsumer(consumerRoot, repoRoot);

    var consumerProject = Path.Combine(consumerRoot, "Wiki.csproj");
    await ScriptHelpers.RunDotNetAsync(
        [
            "publish",
            consumerProject,
            "-c",
            "Release",
            "-o",
            publishRoot,
            "/m:1",
            "/p:BuildInParallel=false",
            "/nr:false",
            "-p:UseSharedCompilation=false",
            "-p:WikiUsePackages=true",
            "-p:JazorMode=release",
            "-p:RestoreSources=" + packageRoot,
            "-p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json",
            "-p:RestorePackagesPath=" + restorePackagesRoot,
            "-p:RestoreForce=true",
            "-p:JazorPackageVersion=" + packageVersion
        ],
        consumerRoot,
        dotnetCliHome);

    ReleaseVerifier.VerifyPublishLayout(publishRoot);

    await ScriptHelpers.RunDotNetAsync(
        [
            "run",
            "--file",
            browserVerificationScript,
            "--",
            "--wiki-root",
            consumerRoot,
            "--published-root",
            publishRoot,
            "--path-base",
            options.PathBase,
            "--port",
            options.Port.ToString(),
            "--cdp-port",
            options.CdpPort.ToString(),
            "--startup-timeout-seconds",
            options.StartupTimeoutSeconds.ToString(),
            "--browser-startup-timeout-seconds",
            options.BrowserStartupTimeoutSeconds.ToString()
        ],
        repoRoot,
        dotnetCliHome);

    Console.WriteLine("Windows SPA release consumer verification passed.");
}
finally
{
    if (!options.KeepWorkRoot && Directory.Exists(workRoot))
    {
        await ScriptHelpers.DeleteDirectoryWithRetryAsync(workRoot);
    }
}

internal sealed record VerificationOptions(
    string PackageSource,
    bool PackPackages,
    string PathBase,
    int Port,
    int CdpPort,
    int StartupTimeoutSeconds,
    int BrowserStartupTimeoutSeconds,
    bool KeepWorkRoot)
{
    public static VerificationOptions Parse(IReadOnlyList<string> arguments)
    {
        var packageSource = Path.Combine("artifacts", "packages");
        var packPackages = true;
        var pathBase = "/docs";
        var port = 4296;
        var cdpPort = 9336;
        var startupTimeoutSeconds = 60;
        var browserStartupTimeoutSeconds = 20;
        var keepWorkRoot = false;

        for (var index = 0; index < arguments.Count; index++)
        {
            switch (arguments[index])
            {
                case "--package-source":
                    packageSource = RequireValue(arguments, ref index, "--package-source");
                    break;
                case "--skip-pack":
                    packPackages = false;
                    break;
                case "--path-base":
                    pathBase = RequireValue(arguments, ref index, "--path-base");
                    break;
                case "--port":
                    port = int.Parse(RequireValue(arguments, ref index, "--port"));
                    break;
                case "--cdp-port":
                    cdpPort = int.Parse(RequireValue(arguments, ref index, "--cdp-port"));
                    break;
                case "--startup-timeout-seconds":
                    startupTimeoutSeconds = int.Parse(RequireValue(arguments, ref index, "--startup-timeout-seconds"));
                    break;
                case "--browser-startup-timeout-seconds":
                    browserStartupTimeoutSeconds = int.Parse(RequireValue(arguments, ref index, "--browser-startup-timeout-seconds"));
                    break;
                case "--keep-work-root":
                    keepWorkRoot = true;
                    break;
                case "--help":
                case "-h":
                    WriteUsage();
                    Environment.Exit(0);
                    break;
                default:
                    throw new InvalidOperationException("Unsupported argument: " + arguments[index]);
            }
        }

        return new VerificationOptions(
            packageSource,
            packPackages,
            ScriptHelpers.NormalizePathBase(pathBase),
            port,
            cdpPort,
            startupTimeoutSeconds,
            browserStartupTimeoutSeconds,
            keepWorkRoot);
    }

    private static string RequireValue(IReadOnlyList<string> arguments, ref int index, string option)
    {
        if (++index >= arguments.Count)
        {
            throw new InvalidOperationException("Missing value for " + option + ".");
        }

        return arguments[index];
    }

    private static void WriteUsage()
    {
        Console.WriteLine("Usage: dotnet run --file scripts/csharp/verify-windows-spa-release.cs -- [options]");
        Console.WriteLine("Options:");
        Console.WriteLine("  --package-source <path>                 Default: artifacts/packages");
        Console.WriteLine("  --skip-pack                             Consume packages already in --package-source");
        Console.WriteLine("  --path-base </docs>                     Default: /docs");
        Console.WriteLine("  --port <number>                         Default: 4296");
        Console.WriteLine("  --cdp-port <number>                     Default: 9336");
        Console.WriteLine("  --startup-timeout-seconds <seconds>     Default: 60");
        Console.WriteLine("  --browser-startup-timeout-seconds <seconds> Default: 20");
        Console.WriteLine("  --keep-work-root");
    }
}

internal static class PackageVerifier
{
    private static readonly string[] RequiredPackageIds = ["Jazor", "Jazor.Vue", "ECMAScript.Style"];

    public static string ResolveSharedPackageVersion(string packageRoot)
    {
        if (!Directory.Exists(packageRoot))
        {
            throw new DirectoryNotFoundException("Package source directory was not found: " + packageRoot);
        }

        var versions = RequiredPackageIds
            .Select(packageId => FindPackage(packageRoot, packageId))
            .Select(ReadPackageVersion)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return versions.Length == 1
            ? versions[0]
            : throw new InvalidOperationException("Required local packages do not have one shared version: " + string.Join(", ", versions));
    }

    public static void VerifyRequiredPackages(string packageRoot, string version)
    {
        foreach (var packageId in RequiredPackageIds)
        {
            var packageFile = FindPackage(packageRoot, packageId);
            var packageVersion = ReadPackageVersion(packageFile);
            if (!packageVersion.Equals(version, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Package '{packageId}' has version '{packageVersion}', expected '{version}'.");
            }

            using var archive = ZipFile.OpenRead(packageFile.FullName);
            if (packageId == "Jazor")
            {
                RequireEntry(archive, "buildTransitive/Jazor.props", packageId);
                RequireEntry(archive, "buildTransitive/Jazor.targets", packageId);
                RequireEntry(archive, "tools/net11.0/Jazor.Emit.dll", packageId);
                RequireEntry(archive, "lib/net11.0/Jazor.AspNetCore.dll", packageId);
                RequireEntry(archive, "lib/net11.0/Jazor.AspNetCore.Dev.dll", packageId);
                RequireEntry(archive, "jazor/vue3/manifest.json", packageId);
            }
            else if (packageId == "Jazor.Vue")
            {
                RequireEntry(archive, "buildTransitive/Jazor.Vue.targets", packageId);
                RequireEntry(archive, "analyzers/dotnet/cs/Jazor.RazorVue.dll", packageId);
            }
            else
            {
                RequireEntry(archive, "lib/net11.0/ECMAScript.Style.dll", packageId);
            }
        }
    }

    private static FileInfo FindPackage(string packageRoot, string packageId)
    {
        return new DirectoryInfo(packageRoot)
            .EnumerateFiles("*.nupkg", SearchOption.TopDirectoryOnly)
            .Where(static file => !file.Name.EndsWith(".snupkg", StringComparison.OrdinalIgnoreCase))
            .Where(file => ReadPackageId(file).Equals(packageId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(static file => file.LastWriteTimeUtc)
            .FirstOrDefault()
            ?? throw new InvalidOperationException("Required package was not found: " + packageId + " under " + packageRoot);
    }

    private static string ReadPackageId(FileInfo packageFile)
        => ReadNuspecMetadata(packageFile, "id");

    private static string ReadPackageVersion(FileInfo packageFile)
        => ReadNuspecMetadata(packageFile, "version");

    private static string ReadNuspecMetadata(FileInfo packageFile, string elementName)
    {
        using var archive = ZipFile.OpenRead(packageFile.FullName);
        var nuspecEntry = archive.Entries.SingleOrDefault(static entry => entry.FullName.EndsWith(".nuspec", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("Package does not contain a nuspec manifest: " + packageFile.FullName);
        using var stream = nuspecEntry.Open();
        var nuspec = XDocument.Load(stream);
        var ns = nuspec.Root?.Name.Namespace ?? XNamespace.None;
        return nuspec.Root?
            .Element(ns + "metadata")?
            .Element(ns + elementName)?
            .Value
            ?? throw new InvalidOperationException("Package metadata is missing '" + elementName + "': " + packageFile.FullName);
    }

    private static void RequireEntry(ZipArchive archive, string path, string packageId)
    {
        if (archive.GetEntry(path) is null)
        {
            throw new InvalidOperationException("Package '" + packageId + "' is missing '" + path + "'.");
        }
    }
}

internal static class ReleaseVerifier
{
    public static void VerifyPublishLayout(string publishRoot)
    {
        RequireFile(Path.Combine(publishRoot, "Wiki.dll"), "published Wiki host");
        var jazorRoot = Path.Combine(publishRoot, "jazor");
        RequireFile(Path.Combine(jazorRoot, "bundle.js"), "release browser bundle");
        RequireFile(Path.Combine(jazorRoot, "bundle.js.map"), "release browser bundle source map");

        foreach (var unexpectedPath in new[]
        {
            Path.Combine(jazorRoot, "main.mjs"),
            Path.Combine(jazorRoot, "jazor-manifest.json"),
            Path.Combine(jazorRoot, "style.mjs"),
            Path.Combine(jazorRoot, "components")
        })
        {
            if (File.Exists(unexpectedPath) || Directory.Exists(unexpectedPath))
            {
                throw new InvalidOperationException("Release publish retained a debug artifact: " + unexpectedPath);
            }
        }

        var bundle = File.ReadAllText(Path.Combine(jazorRoot, "bundle.js"));
        RequireContains(bundle, "ecmascript-style:v1", "ECMAScript.Style runtime marker in release bundle");
        RequireContains(bundle, "H() + ECMAScript.Style", "H-function page marker in release bundle");

        var bundleMap = File.ReadAllText(Path.Combine(jazorRoot, "bundle.js.map"));
        RequireContains(bundleMap, "components/wiki-styles.mjs", "Wiki style module source in release source map");
        RequireContains(bundleMap, "main.mjs", "Wiki entry source in release source map");
    }

    private static void RequireFile(string path, string description)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Missing " + description + ": " + path, path);
        }
    }

    private static void RequireContains(string text, string value, string description)
    {
        if (!text.Contains(value, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Missing " + description + ": " + value);
        }
    }
}

internal static class ScriptHelpers
{
    private static readonly StringComparison PathComparison = OperatingSystem.IsWindows()
        ? StringComparison.OrdinalIgnoreCase
        : StringComparison.Ordinal;

    public static string RequireRepoRoot()
    {
        for (var directory = new DirectoryInfo(Directory.GetCurrentDirectory()); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException("Repository root containing Jazor.slnx was not found.");
    }

    public static string ResolvePath(string repoRoot, string path)
        => Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path));

    public static string NormalizePathBase(string pathBase)
    {
        if (string.IsNullOrWhiteSpace(pathBase) || pathBase == "/")
        {
            return string.Empty;
        }

        if (!pathBase.StartsWith('/', StringComparison.Ordinal))
        {
            throw new InvalidOperationException("--path-base must start with '/'.");
        }

        return pathBase.EndsWith('/', StringComparison.Ordinal) ? pathBase[..^1] : pathBase;
    }

    public static void CopyWikiSource(string sourceRoot, string targetRoot)
    {
        if (!Directory.Exists(sourceRoot))
        {
            throw new DirectoryNotFoundException("Wiki source directory was not found: " + sourceRoot);
        }

        foreach (var directory in Directory.EnumerateDirectories(sourceRoot, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceRoot, directory);
            if (IsExcluded(relativePath))
            {
                continue;
            }

            Directory.CreateDirectory(Path.Combine(targetRoot, relativePath));
        }

        foreach (var sourcePath in Directory.EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceRoot, sourcePath);
            if (IsExcluded(relativePath))
            {
                continue;
            }

            var targetPath = Path.Combine(targetRoot, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
            File.Copy(sourcePath, targetPath, overwrite: true);
        }
    }

    public static void CopyWikiConsumerProps(string repoRoot, string consumerRoot)
    {
        var sourcePath = Path.Combine(repoRoot, "samples", "Directory.Build.props");
        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException("Wiki consumer props file was not found.", sourcePath);
        }

        Directory.CreateDirectory(consumerRoot);
        File.Copy(sourcePath, Path.Combine(consumerRoot, "Directory.Build.props"), overwrite: true);
    }

    public static Task MaterializeWikiDocsCatalogAsync(string repoRoot, string consumerRoot, string dotnetCliHome)
    {
        // A detached package consumer has no repository-relative docs/ or importer script.
        // Generate the same catalog before publish, while source builds retain the MSBuild target.
        return RunDotNetAsync(
            [
                "run",
                "--file",
                Path.Combine("scripts", "csharp", "wiki-import-docs.cs"),
                "--",
                "--docs",
                Path.Combine(repoRoot, "docs"),
                "--output",
                Path.Combine(consumerRoot, "obj", "wiki", "WikiDocsContent.g.cs")
            ],
            repoRoot,
            dotnetCliHome);
    }

    public static void AssertDetachedConsumer(string consumerRoot, string repoRoot)
    {
        var projectPath = Path.Combine(consumerRoot, "Wiki.csproj");
        var projectText = File.ReadAllText(projectPath);
        if (!projectText.Contains("WikiUsePackages", StringComparison.Ordinal) ||
            !projectText.Contains("PackageReference Include=\"Jazor\"", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Copied Wiki does not expose the package-consumer configuration.");
        }

        var sourceReferenceMarker = "..\\..\\src\\";
        if (projectText.Contains(sourceReferenceMarker, StringComparison.OrdinalIgnoreCase) &&
            !projectText.Contains("Condition=\"'$(WikiUsePackages)' != 'true'\"", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Copied Wiki contains unconditional repository source references.");
        }

        var resolvedRelativeSourceRoot = Path.GetFullPath(Path.Combine(consumerRoot, "..", "..", "src"));
        if (Directory.Exists(resolvedRelativeSourceRoot))
        {
            throw new InvalidOperationException(
                "Consumer verification root resolves repository source references at '" + resolvedRelativeSourceRoot + "'.");
        }
    }

    public static void DeleteDirectoryWithinRepo(string repoRoot, string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        var fullRoot = Path.GetFullPath(repoRoot);
        var fullPath = Path.GetFullPath(path);
        if (!fullPath.StartsWith(fullRoot + Path.DirectorySeparatorChar, PathComparison))
        {
            throw new InvalidOperationException("Refusing to delete outside the repository: " + fullPath);
        }

        Directory.Delete(fullPath, recursive: true);
    }

    public static async Task DeleteDirectoryWithRetryAsync(string path, int attempts = 6)
    {
        for (var attempt = 0; attempt < attempts; attempt++)
        {
            if (!Directory.Exists(path))
            {
                return;
            }

            try
            {
                Directory.Delete(path, recursive: true);
                return;
            }
            catch when (attempt < attempts - 1)
            {
                await Task.Delay(250);
            }
        }
    }

    public static async Task RunDotNetAsync(IReadOnlyList<string> arguments, string workdir, string dotnetCliHome)
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
            ?? throw new InvalidOperationException("Failed to start dotnet.");
        await process.WaitForExitAsync();
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException("Process failed with exit code " + process.ExitCode + ": dotnet " + string.Join(' ', arguments));
        }
    }

    private static bool IsExcluded(string relativePath)
    {
        var segments = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return segments.Any(segment =>
            segment.Equals("bin", StringComparison.OrdinalIgnoreCase) ||
            segment.Equals("obj", StringComparison.OrdinalIgnoreCase) ||
            segment.Equals("jazor", StringComparison.OrdinalIgnoreCase) ||
            segment.StartsWith(".wiki-", StringComparison.OrdinalIgnoreCase));
    }
}
