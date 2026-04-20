namespace Jolt.Razor.Toolset;

internal sealed class RazorSdkToolsetResolver
{
    private const string RazorSdkName = "Microsoft.NET.Sdk.Razor";
    private const string RazorSourceGeneratorRelativePath = "source-generators\\Microsoft.CodeAnalysis.Razor.Compiler.dll";
    private const string RazorTasksRelativePath = "tasks\\net10.0\\Microsoft.NET.Sdk.Razor.Tasks.dll";
    private const string RazorDesignTimeTargetsRelativePath = "targets\\Microsoft.NET.Sdk.Razor.DesignTime.targets";
    private const string RazorComponentTargetsRelativePath = "targets\\Microsoft.NET.Sdk.Razor.Component.targets";
    private const string BundledSdkEnvironmentVariable = "JOLT_DOTNET_ROOT";
    private const string BundledSdkVersionEnvironmentVariable = "JOLT_DOTNET_SDK_VERSION";

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

        var dotnetRoot = GetDotNetRoot();
        if (TryAddRoot(dotnetRoot, version: null, seen, out var globalCandidate))
        {
            yield return globalCandidate;
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
            .OrderByDescending(static version => version, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (versions.Length == 0)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(preferredVersion))
        {
            versions = versions
                .OrderByDescending(version => string.Equals(version, preferredVersion, StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(static version => version, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        foreach (var version in versions)
        {
            var versionRoot = Path.Combine(sdkRoot, version);
            var razorSdkRoot = Path.Combine(versionRoot, "Sdks", RazorSdkName);
            var sourceGeneratorPath = Path.Combine(razorSdkRoot, RazorSourceGeneratorRelativePath);
            var tasksPath = Path.Combine(razorSdkRoot, RazorTasksRelativePath);
            var designTimeTargetsPath = Path.Combine(razorSdkRoot, RazorDesignTimeTargetsRelativePath);
            var componentTargetsPath = Path.Combine(razorSdkRoot, RazorComponentTargetsRelativePath);

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

    private static string? GetDotNetRoot()
    {
        var envRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");
        if (!string.IsNullOrWhiteSpace(envRoot))
        {
            return envRoot;
        }

        return OperatingSystem.IsWindows()
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet")
            : "/usr/share/dotnet";
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

        var fullPath = Path.GetFullPath(rootPath);
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
}
