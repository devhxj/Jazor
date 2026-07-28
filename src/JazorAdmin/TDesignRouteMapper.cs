using static ECMAScript.VueRoute;

namespace JazorAdmin;

[ECMAScriptModule("components/jazor-admin-tdesign-route-mapper.mjs")]
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

    public static TDesignMenuRouteTarget? MapRoute(RouteLocationRaw? routeTarget)
    {
        if (!routeTarget.HasValue)
            return null;

        var route = routeTarget.Value;
        if (route.AsString is { } routeString)
            return (TDesignMenuRouteTarget)routeString;

        if (route.AsPath is { } pathRoute)
        {
            return (TDesignMenuRouteTarget)new TDesignMenuRoute
            {
                Path = pathRoute.Path,
                Hash = pathRoute.Hash
            };
        }

        if (route.AsRelative is { } relativeRoute)
        {
            return (TDesignMenuRouteTarget)new TDesignMenuRoute
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
