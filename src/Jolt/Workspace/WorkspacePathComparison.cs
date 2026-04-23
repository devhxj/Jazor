namespace Jolt.Workspace;

internal static class WorkspacePathComparison
{
    public static bool IsCaseSensitivePlatform { get; } = !OperatingSystem.IsWindows();

    public static StringComparer StringComparer { get; } = CreateStringComparer(IsCaseSensitivePlatform);

    public static StringComparison StringComparison { get; } = CreateStringComparison(IsCaseSensitivePlatform);

    public static StringComparer CreateStringComparer(bool isCaseSensitive)
        => isCaseSensitive
            ? global::System.StringComparer.Ordinal
            : global::System.StringComparer.OrdinalIgnoreCase;

    public static StringComparison CreateStringComparison(bool isCaseSensitive)
        => isCaseSensitive
            ? global::System.StringComparison.Ordinal
            : global::System.StringComparison.OrdinalIgnoreCase;

    public static bool Equals(string? left, string? right)
        => string.Equals(left, right, StringComparison);

    public static bool StartsWith(string value, string prefix)
        => value.StartsWith(prefix, StringComparison);
}
