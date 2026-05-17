namespace ECMAScript.Vben;

internal readonly record struct VbenResolvedNavigationTarget(
    string? Href,
    VbenRouteLocation? Route)
{
    public bool HasHref
        => !string.IsNullOrWhiteSpace(Href);

    public bool HasRoute
        => Route is not null;

    public bool IsNavigable
        => HasHref || HasRoute;
}

internal static class VbenNavigationTargetResolver
{
    public static VbenResolvedNavigationTarget Resolve(VbenNavTarget? target)
    {
        var href = NormalizeOptional(target?.AsHref);
        if (href is not null)
        {
            return new(href, null);
        }

        if (target?.AsRoute is { } route && TryNormalizeRoute(route, out var normalizedRoute))
        {
            return new(null, normalizedRoute);
        }

        return default;
    }

    public static string? TryResolveHref(VbenNavTarget? target)
    {
        var resolved = Resolve(target);
        return resolved.HasHref ? resolved.Href : null;
    }

    public static VbenRouteLocation? TryResolveRoute(VbenNavTarget? target)
    {
        var resolved = Resolve(target);
        return resolved.HasRoute ? resolved.Route : null;
    }

    private static bool TryNormalizeRoute(
        VbenRouteLocation route,
        out VbenRouteLocation? normalizedRoute)
    {
        var path = NormalizeOptional(route.Path);
        var hash = NormalizeHash(route.Hash);
        if (path is not null)
        {
            normalizedRoute = new VbenRouteLocation
            {
                Path = path,
                Hash = hash
            };

            return true;
        }

        var name = NormalizeOptional(route.Name);
        if (name is not null || hash is not null)
        {
            normalizedRoute = new VbenRouteLocation
            {
                Name = name,
                Hash = hash
            };

            return true;
        }

        normalizedRoute = null;
        return false;
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeHash(string? hash)
    {
        var normalizedHash = NormalizeOptional(hash);
        return normalizedHash is null ? null : EnsureHashPrefix(normalizedHash);
    }

    private static string EnsureHashPrefix(string hash)
        => hash.StartsWith("#", StringComparison.Ordinal) ? hash : $"#{hash}";
}
