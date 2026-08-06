namespace Jazor.Admin;

[ECMAScriptModule("components/admin/navigation-target.mjs")]
internal static class AdminNavigationTargetResolver
{
    public static ResolvedNavigationTarget Resolve(
        string? href,
        RouteLocationRaw? routeTarget)
    {
        if (routeTarget.HasValue)
        {
            return ResolvedNavigationTarget.ForRoute(routeTarget.Value);
        }

        var normalizedHref = NormalizeOptional(href);
        if (normalizedHref is not null)
        {
            return ResolvedNavigationTarget.ForHref(normalizedHref);
        }

        return ResolvedNavigationTarget.Empty;
    }

    public static string? TryResolveHref(string? href)
    {
        return NormalizeOptional(href);
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    [ECMAScript]
    internal sealed class ResolvedNavigationTarget
    {
        public ResolvedNavigationTarget(string? href, RouteLocationRaw route, bool hasRoute)
        {
            Href = href;
            Route = route;
            HasRoute = hasRoute;
        }

        public static ResolvedNavigationTarget ForHref(string href)
            => new(href, string.Empty, false);

        public static ResolvedNavigationTarget ForRoute(RouteLocationRaw route)
            => new(null, route, true);

        public static ResolvedNavigationTarget Empty
            => new(null, string.Empty, false);

        public string? Href { get; }

        public RouteLocationRaw Route { get; }

        public bool HasHref
            => !string.IsNullOrWhiteSpace(Href);

        public bool HasRoute { get; }

        public bool IsNavigable
            => HasHref || HasRoute;
    }
}
