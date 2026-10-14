using static ECMAScript.VueRoute;

namespace JazorAdmin;

[ECMAScriptModule("components/tdesign/routes.js")]
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

    // Raw route object branches erase to indistinguishable objects. Let Vue Router resolve
    // named routes, params, query and hash before adapting the concrete URL to TDesign.
    public static TMenuItemToValue? MapMenuRoute(RouteLocationRaw? routeTarget)
    {
        if (!routeTarget.HasValue)
            return null;

        var route = routeTarget.Value;
        return (TMenuItemToValue)(route.AsString ?? UseRouter().Resolve(route).FullPath);
    }

    public static TBreadcrumbItemToValue? MapBreadcrumbRoute(RouteLocationRaw? routeTarget)
    {
        if (!routeTarget.HasValue)
            return null;

        var route = routeTarget.Value;
        return (TBreadcrumbItemToValue)(route.AsString ?? UseRouter().Resolve(route).FullPath);
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
