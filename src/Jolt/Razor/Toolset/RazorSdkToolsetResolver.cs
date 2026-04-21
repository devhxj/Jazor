using System.Diagnostics;

namespace Jolt.Razor.Toolset;

internal sealed class RazorSdkToolsetResolver
{
    private const string RazorSdkName = "Microsoft.NET.Sdk.Razor";
    private const string BundledSdkEnvironmentVariable = "JOLT_DOTNET_ROOT";
    private const string BundledSdkVersionEnvironmentVariable = "JOLT_DOTNET_SDK_VERSION";
    private static readonly string[] DotNetRootEnvironmentVariables =
    [
        BundledSdkEnvironmentVariable,
        "DOTNET_ROOT",
        "DOTNET_ROOT_X64",
        "DOTNET_ROOT_ARM64",
        "DOTNET_ROOT(x86)"
    ];
    private static readonly string[] NonWindowsDotNetRoots =
    [
        "/usr/share/dotnet",
        "/usr/local/share/dotnet",
        "/usr/lib/dotnet",
        "/usr/lib64/dotnet",
        "/opt/dotnet"
    ];

    public RazorSdkToolset? Resolve()
    {
        foreach (var candidate in EnumerateRoots())
        {
            var toolset = TryResolveFromRoot(candidate.RootPath, candidate.Version);
            if (toolset is not null)
            {
                return toolset;
            }
        }

        return null;
    }

    private IEnumerable<(string RootPath, string? Version)> EnumerateRoots()
    {
        var seen = new HashSet<string>(GetPathComparer());

        var bundledRoot = Environment.GetEnvironmentVariable(BundledSdkEnvironmentVariable);
        var bundledVersion = Environment.GetEnvironmentVariable(BundledSdkVersionEnvironmentVariable);
        if (TryAddRoot(bundledRoot, bundledVersion, seen, out var bundledCandidate))
        {
            yield return bundledCandidate;
        }

        var appBaseDirectory = AppContext.BaseDirectory;
        if (TryAddRoot(Path.Combine(appBaseDirectory, "dotnet"), version: null, seen, out var localCandidate))
        {
            yield return localCandidate;
        }

        foreach (var dotnetRoot in EnumerateDefaultDotNetRoots())
        {
            if (TryAddRoot(dotnetRoot, version: null, seen, out var globalCandidate))
            {
                yield return globalCandidate;
            }
        }
    }

    private RazorSdkToolset? TryResolveFromRoot(string rootPath, string? preferredVersion)
    {
        var sdkRoot = Path.Combine(rootPath, "sdk");
        if (!Directory.Exists(sdkRoot))
        {
            return null;
        }

        var versions = Directory.GetDirectories(sdkRoot)
            .Select(Path.GetFileName)
            .Where(static version => !string.IsNullOrWhiteSpace(version))
            .Select(static version => version!)
            .OrderByDescending(static version => version, SdkVersionComparer.Instance)
            .ToArray();
        if (versions.Length == 0)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(preferredVersion))
        {
            versions = versions
                .OrderByDescending(version => string.Equals(version, preferredVersion, StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(static version => version, SdkVersionComparer.Instance)
                .ToArray();
        }

        foreach (var version in versions)
        {
            var versionRoot = Path.Combine(sdkRoot, version);
            var razorSdkRoot = Path.Combine(versionRoot, "Sdks", RazorSdkName);
            var sourceGeneratorPath = Path.Combine(razorSdkRoot, "source-generators", "Microsoft.CodeAnalysis.Razor.Compiler.dll");
            var tasksPath = Path.Combine(razorSdkRoot, "tasks", "net10.0", "Microsoft.NET.Sdk.Razor.Tasks.dll");
            var designTimeTargetsPath = Path.Combine(razorSdkRoot, "targets", "Microsoft.NET.Sdk.Razor.DesignTime.targets");
            var componentTargetsPath = Path.Combine(razorSdkRoot, "targets", "Microsoft.NET.Sdk.Razor.Component.targets");

            if (!File.Exists(sourceGeneratorPath)
                || !File.Exists(tasksPath)
                || !File.Exists(designTimeTargetsPath)
                || !File.Exists(componentTargetsPath))
            {
                continue;
            }

            return new RazorSdkToolset(
                rootPath,
                version,
                versionRoot,
                razorSdkRoot,
                sourceGeneratorPath,
                tasksPath,
                designTimeTargetsPath,
                componentTargetsPath);
        }

        return null;
    }

    private static IEnumerable<string> EnumerateDefaultDotNetRoots()
    {
        foreach (var environmentVariable in DotNetRootEnvironmentVariables)
        {
            var envRoot = Environment.GetEnvironmentVariable(environmentVariable);
            if (!string.IsNullOrWhiteSpace(envRoot))
            {
                yield return envRoot;
            }
        }

        if (OperatingSystem.IsWindows())
        {
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            if (!string.IsNullOrWhiteSpace(programFiles))
            {
                yield return Path.Combine(programFiles, "dotnet");
            }

            var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if (!string.IsNullOrWhiteSpace(programFilesX86))
            {
                yield return Path.Combine(programFilesX86, "dotnet");
            }

            yield break;
        }

        var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrWhiteSpace(homeDirectory))
        {
            yield return Path.Combine(homeDirectory, ".dotnet");
        }

        foreach (var root in NonWindowsDotNetRoots)
        {
            yield return root;
        }

        var discoveredRoot = TryGetDotNetRootFromInfo();
        if (!string.IsNullOrWhiteSpace(discoveredRoot))
        {
            yield return discoveredRoot;
        }
    }

    internal static string? TryParseDotNetRootFromInfoOutput(string? infoOutput)
    {
        if (string.IsNullOrWhiteSpace(infoOutput))
        {
            return null;
        }

        foreach (var line in infoOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = line.Trim();
            if (!trimmed.StartsWith("Base Path:", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var basePath = trimmed["Base Path:".Length..].Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(basePath))
            {
                continue;
            }

            var normalizedBasePath = basePath.Replace('\\', '/').TrimEnd('/');
            var sdkSegmentIndex = normalizedBasePath.LastIndexOf("/sdk/", StringComparison.OrdinalIgnoreCase);
            if (sdkSegmentIndex <= 0)
            {
                continue;
            }

            var rootPath = normalizedBasePath[..sdkSegmentIndex];
            return basePath.Contains('\\')
                ? rootPath.Replace('/', '\\')
                : rootPath;
        }

        return null;
    }

    private static string? TryGetDotNetRootFromInfo()
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "--info",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            if (!process.Start())
            {
                return null;
            }

            var standardOutput = process.StandardOutput.ReadToEndAsync();
            var standardError = process.StandardError.ReadToEndAsync();
            if (!process.WaitForExit((int)TimeSpan.FromSeconds(3).TotalMilliseconds))
            {
                TryTerminate(process);
                return null;
            }

            _ = standardError.GetAwaiter().GetResult();
            var output = standardOutput.GetAwaiter().GetResult();
            return TryParseDotNetRootFromInfoOutput(output);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
    }

    private static void TryTerminate(Process process)
    {
        try
        {
            process.Kill(entireProcessTree: true);
        }
        catch (InvalidOperationException)
        {
        }
        catch (NotSupportedException)
        {
        }
    }

    private static bool TryAddRoot(
        string? rootPath,
        string? version,
        ISet<string> seen,
        out (string RootPath, string? Version) candidate)
    {
        candidate = default;
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            return false;
        }

        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(rootPath);
        }
        catch (Exception exception) when (
            exception is ArgumentException
            or IOException
            or NotSupportedException)
        {
            return false;
        }

        if (!Directory.Exists(fullPath) || !seen.Add(fullPath))
        {
            return false;
        }

        candidate = (fullPath, version);
        return true;
    }

    private static StringComparer GetPathComparer()
        => OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;

    private sealed class SdkVersionComparer : IComparer<string>
    {
        public static SdkVersionComparer Instance { get; } = new();

        public int Compare(string? left, string? right)
        {
            if (ReferenceEquals(left, right))
            {
                return 0;
            }

            if (left is null)
            {
                return -1;
            }

            if (right is null)
            {
                return 1;
            }

            var leftParsed = TryParse(left, out var leftVersion, out var leftPrerelease);
            var rightParsed = TryParse(right, out var rightVersion, out var rightPrerelease);
            if (leftParsed && rightParsed)
            {
                var versionComparison = leftVersion!.CompareTo(rightVersion);
                if (versionComparison != 0)
                {
                    return versionComparison;
                }

                if (leftPrerelease.Length == 0 && rightPrerelease.Length > 0)
                {
                    return 1;
                }

                if (leftPrerelease.Length > 0 && rightPrerelease.Length == 0)
                {
                    return -1;
                }

                var prereleaseComparison = string.Compare(leftPrerelease, rightPrerelease, StringComparison.OrdinalIgnoreCase);
                if (prereleaseComparison != 0)
                {
                    return prereleaseComparison;
                }
            }
            else if (leftParsed != rightParsed)
            {
                return leftParsed ? 1 : -1;
            }

            return string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryParse(string versionText, out Version? version, out string prereleaseLabel)
        {
            version = null;
            prereleaseLabel = string.Empty;

            if (string.IsNullOrWhiteSpace(versionText))
            {
                return false;
            }

            var separatorIndex = versionText.IndexOf('-', StringComparison.Ordinal);
            var numericPart = separatorIndex >= 0
                ? versionText[..separatorIndex]
                : versionText;
            prereleaseLabel = separatorIndex >= 0
                ? versionText[(separatorIndex + 1)..]
                : string.Empty;
            return Version.TryParse(numericPart, out version);
        }
    }
}
