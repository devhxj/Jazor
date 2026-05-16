namespace ECMAScript.Vben;

internal static class VbenNavigationTargetResolver
{
    public static string? TryResolveHref(VbenNavTarget? target)
    {
        if (target?.AsHref is { Length: > 0 } href)
        {
            return href;
        }

        if (target?.AsRoute is not { } route)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(route.Path))
        {
            return string.IsNullOrWhiteSpace(route.Hash)
                ? route.Path
                : $"{route.Path}{EnsureHashPrefix(route.Hash)}";
        }

        if (!string.IsNullOrWhiteSpace(route.Hash))
        {
            return EnsureHashPrefix(route.Hash);
        }

        return null;
    }

    private static string EnsureHashPrefix(string hash)
        => hash.StartsWith("#", StringComparison.Ordinal) ? hash : $"#{hash}";
}
