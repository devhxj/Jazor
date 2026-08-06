using static ECMAScript.VueRoute;

namespace JazorAdmin;

[ECMAScriptModule("components/tdesign/routes.mjs")]
internal static class TDesignRouteMapper
{
    public static string? MapHref(string? href, RouteLocationRaw? routeTarget)
        => routeTarget.HasValue ? null : Normalize(href);

    public static string? MapActionHref(string? href, RouteLocationRaw? routeTarget)
    {
        if (!routeTarget.HasValue)
            return Normalize(href);

        return routeTarget.Value.AsString;
    }

    // TDesign exposes separate menu and breadcrumb route records. Keep both mappings exact
    // even though their current JavaScript shapes match.
    public static TMenuItemToValue? MapMenuRoute(RouteLocationRaw? routeTarget)
    {
        if (!routeTarget.HasValue)
            return null;

        var route = routeTarget.Value;
        if (route.AsString is { } routeString)
            return (TMenuItemToValue)routeString;

        if (route.AsPath is { } pathRoute)
        {
            return (TMenuItemToValue)new TMenuRoute
            {
                Path = pathRoute.Path,
                Hash = pathRoute.Hash
            };
        }

        if (route.AsRelative is { } relativeRoute)
        {
            return (TMenuItemToValue)new TMenuRoute
            {
                Name = relativeRoute.Name.HasValue ? relativeRoute.Name.Value.AsString : null,
                Hash = relativeRoute.Hash
            };
        }

        return null;
    }

    public static TBreadcrumbItemToValue? MapBreadcrumbRoute(RouteLocationRaw? routeTarget)
    {
        if (!routeTarget.HasValue)
            return null;

        var route = routeTarget.Value;
        if (route.AsString is { } routeString)
            return (TBreadcrumbItemToValue)routeString;

        if (route.AsPath is { } pathRoute)
        {
            return (TBreadcrumbItemToValue)new TRoute
            {
                Path = pathRoute.Path,
                Hash = pathRoute.Hash
            };
        }

        if (route.AsRelative is { } relativeRoute)
        {
            return (TBreadcrumbItemToValue)new TRoute
            {
                Name = relativeRoute.Name.HasValue ? relativeRoute.Name.Value.AsString : null,
                Hash = relativeRoute.Hash
            };
        }

        return null;
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
