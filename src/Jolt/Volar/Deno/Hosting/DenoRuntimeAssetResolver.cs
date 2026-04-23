using System.Runtime.InteropServices;

namespace Jolt.Volar.Deno.Hosting;

internal static class DenoRuntimeAssetResolver
{
    private static readonly string[] PortableRuntimeIdentifiers =
    [
        "win-x64",
        "win-arm64",
        "linux-x64",
        "linux-arm64",
        "osx-x64",
        "osx-arm64"
    ];

    public static string ResolveBundledExecutablePath(string? baseDirectory = null)
    {
        if (TryResolveBundledExecutablePath(baseDirectory, out var executablePath))
            return executablePath;

        return GetExpectedBundledExecutablePath(baseDirectory);
    }

    public static bool TryResolveBundledExecutablePath(string? baseDirectory, out string executablePath)
    {
        var resolvedBaseDirectory = ResolveBaseDirectory(baseDirectory);
        var fileName = GetExecutableFileName();

        foreach (var runtimeIdentifier in EnumerateRuntimeIdentifiers())
        {
            var candidate = Path.Combine(
                resolvedBaseDirectory,
                "runtimes",
                runtimeIdentifier,
                "native",
                fileName);
            if (File.Exists(candidate))
            {
                executablePath = candidate;
                return true;
            }
        }

        executablePath = GetExpectedBundledExecutablePath(resolvedBaseDirectory);
        return false;
    }

    public static string GetExpectedBundledExecutablePath(string? baseDirectory = null)
        => Path.Combine(
            ResolveBaseDirectory(baseDirectory),
            "runtimes",
            GetPortableRuntimeIdentifier(),
            "native",
            GetExecutableFileName());

    public static string ResolveWorkerPath(string? baseDirectory = null)
    {
        var resolvedBaseDirectory = ResolveBaseDirectory(baseDirectory);
        var outputWorkerPath = Path.GetFullPath(Path.Combine(
            resolvedBaseDirectory,
            "Volar",
            "Deno",
            "Worker",
            "volar-worker.ts"));
        if (File.Exists(outputWorkerPath))
        {
            return outputWorkerPath;
        }

        var sourceWorkerPath = Path.GetFullPath(Path.Combine(
            resolvedBaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "src",
            "Jolt",
            "Volar",
            "Deno",
            "Worker",
            "volar-worker.ts"));
        if (File.Exists(sourceWorkerPath))
        {
            return sourceWorkerPath;
        }

        return outputWorkerPath;
    }

    public static string? ResolveWorkingDirectory(string? explicitWorkingDirectory, string workerPath)
    {
        if (!string.IsNullOrWhiteSpace(explicitWorkingDirectory))
            return explicitWorkingDirectory;

        return string.IsNullOrWhiteSpace(workerPath)
            ? null
            : Path.GetDirectoryName(workerPath);
    }

    public static string ResolveCacheDirectory(string? baseDirectory = null)
        => Path.GetFullPath(Path.Combine(
            ResolveBaseDirectory(baseDirectory),
            "Volar",
            "Deno",
            "Cache"));

    public static bool HasReadyWorkerDependencies(string workerPath, string? cacheDirectory)
    {
        var workerDirectory = string.IsNullOrWhiteSpace(workerPath)
            ? null
            : Path.GetDirectoryName(workerPath);
        if (!string.IsNullOrWhiteSpace(workerDirectory)
            && Directory.Exists(Path.Combine(workerDirectory, "node_modules", "@volar"))
            && Directory.Exists(Path.Combine(workerDirectory, "node_modules", "@vue")))
        {
            return true;
        }

        return HasReadyDependencyCache(cacheDirectory);
    }

    public static bool HasReadyDependencyCache(string? cacheDirectory)
    {
        if (string.IsNullOrWhiteSpace(cacheDirectory))
        {
            return false;
        }

        var registryCacheDirectory = Path.Combine(cacheDirectory, "npm", "registry.npmjs.org");
        return Directory.Exists(Path.Combine(registryCacheDirectory, "@volar"))
            && Directory.Exists(Path.Combine(registryCacheDirectory, "@vue"));
    }

    public static string CreateMissingRuntimeMessage(string executablePath)
        => $"Failed to locate the packaged Deno runtime for Jolt at '{executablePath}'. Ensure DenoHost runtime assets are available for the current RID and restore/build Jolt before starting the Volar worker.";

    private static string ResolveBaseDirectory(string? baseDirectory)
        => string.IsNullOrWhiteSpace(baseDirectory)
            ? AppContext.BaseDirectory
            : Path.GetFullPath(baseDirectory);

    private static IEnumerable<string> EnumerateRuntimeIdentifiers()
    {
        var runtimeIdentifier = RuntimeInformation.RuntimeIdentifier;
        if (!string.IsNullOrWhiteSpace(runtimeIdentifier))
            yield return runtimeIdentifier;

        var portableRuntimeIdentifier = GetPortableRuntimeIdentifier();
        if (!string.Equals(runtimeIdentifier, portableRuntimeIdentifier, StringComparison.OrdinalIgnoreCase))
            yield return portableRuntimeIdentifier;

        foreach (var candidate in PortableRuntimeIdentifiers)
        {
            if (!string.Equals(candidate, runtimeIdentifier, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(candidate, portableRuntimeIdentifier, StringComparison.OrdinalIgnoreCase))
            {
                yield return candidate;
            }
        }
    }

    private static string GetPortableRuntimeIdentifier()
    {
        var platform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "win"
            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? "osx"
                : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                    ? "linux"
                    : throw new PlatformNotSupportedException("Jolt Deno runtime is only packaged for Windows, Linux, and macOS.");

        var architecture = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.Arm64 => "arm64",
            _ => throw new PlatformNotSupportedException($"Unsupported Deno runtime architecture '{RuntimeInformation.ProcessArchitecture}'.")
        };

        return $"{platform}-{architecture}";
    }

    private static string GetExecutableFileName()
        => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "deno.exe"
            : "deno";
}
