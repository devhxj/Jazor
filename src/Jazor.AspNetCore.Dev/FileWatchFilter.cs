namespace Jazor.AspNetCore.Dev;

/// <summary>Rejects generated, dependency, and editor directories from reload observation.</summary>
internal static class FileWatchFilter
{
    private static readonly HashSet<string> IgnoredDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git",
        ".vs",
        ".vscode",
        ".idea",
        "node_modules",
        "bin",
        "obj",
        "dist",
        "artifacts",
        ".artifacts",
        ".dotnet",
        "TestResults"
    };

    /// <summary>Returns whether a path is inside the root and outside ignored tooling directories.</summary>
    public static bool ShouldObserve(string rootDirectory, string path)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory) || string.IsNullOrWhiteSpace(path))
            return false;

        var fullRootDirectory = Path.GetFullPath(rootDirectory);
        var fullPath = Path.GetFullPath(path);
        var relativePath = Path.GetRelativePath(fullRootDirectory, fullPath);
        if (!IsInsideRoot(relativePath))
            return false;

        foreach (var segment in relativePath.Split(['\\', '/'], StringSplitOptions.RemoveEmptyEntries))
        {
            if (IgnoredDirectories.Contains(segment))
                return false;
        }

        return true;
    }

    /// <summary>Returns whether a directory name is excluded from recursive observation.</summary>
    public static bool IsIgnoredDirectoryName(string? directoryName)
        => !string.IsNullOrWhiteSpace(directoryName)
            && IgnoredDirectories.Contains(directoryName);

    private static bool IsInsideRoot(string relativePath)
        => string.Equals(relativePath, ".", StringComparison.Ordinal)
            || (!string.Equals(relativePath, "..", StringComparison.Ordinal)
                && !relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                && !relativePath.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal)
                && !Path.IsPathRooted(relativePath));
}
