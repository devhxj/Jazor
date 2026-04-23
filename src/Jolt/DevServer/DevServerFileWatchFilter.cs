using Jolt.Workspace;

namespace Jolt.DevServer;

internal static class DevServerFileWatchFilter
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jazor",
        ".vue",
        ".ts",
        ".js",
        ".css",
        ".html",
        ".json"
    };

    private static readonly HashSet<string> IgnoredDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        "node_modules",
        ".git",
        "bin",
        "obj",
        "dist",
        "artifacts",
        ".artifacts",
        ".jazor",
        ".dotnet",
        "TestResults",
        ".vs",
        ".deno"
    };

    public static bool ShouldObserve(string rootDirectory, string path)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory) || string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var fullRootDirectory = Path.GetFullPath(rootDirectory);
        var fullPath = Path.GetFullPath(path);
        var relativePath = Path.GetRelativePath(fullRootDirectory, fullPath);
        if (!IsInsideRoot(relativePath))
        {
            return false;
        }

        foreach (var segment in relativePath.Split(['\\', '/'], StringSplitOptions.RemoveEmptyEntries))
        {
            if (IgnoredDirectories.Contains(segment))
            {
                return false;
            }
        }

        var extension = Path.GetExtension(fullPath);
        if (!SupportedExtensions.Contains(extension))
        {
            return string.Equals(extension, ".cs", StringComparison.OrdinalIgnoreCase)
                && JoltWorkspaceResolver.TryResolveOwningJazorPath(fullPath, out _);
        }

        return true;
    }

    internal static bool IsIgnoredDirectoryName(string? directoryName)
        => !string.IsNullOrWhiteSpace(directoryName)
            && IgnoredDirectories.Contains(directoryName);

    private static bool IsInsideRoot(string relativePath)
        => string.Equals(relativePath, ".", StringComparison.Ordinal)
            || (!string.Equals(relativePath, "..", StringComparison.Ordinal)
                && !relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                && !relativePath.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal)
                && !Path.IsPathRooted(relativePath));
}
