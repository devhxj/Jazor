using Microsoft.AspNetCore.Components;

namespace JazorAdmin;

/// <summary>
/// Renders the current route hierarchy between the tab strip and page content.
/// </summary>
[ECMAScriptModule("./components/route-breadcrumb")]
public partial class RouteBreadcrumb : AdminComponentBase
{
    [Parameter]
    public AdminBreadcrumbItem[] Items { get; set; } = [];

    private const string RootCssClass = "ja-route-breadcrumb";

    private bool IsCurrent(AdminBreadcrumbItem item)
        => Items.Length > 0 && Items[Items.Length - 1].Key == item.Key;

    private static string? MapHref(string? href, RouteLocationRaw? routeTarget)
        => TDesignRouteMapper.MapHref(href, routeTarget);

    private static TBreadcrumbItemToValue? MapRoute(RouteLocationRaw? routeTarget)
        => TDesignRouteMapper.MapBreadcrumbRoute(routeTarget);
}
