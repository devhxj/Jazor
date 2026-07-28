using Microsoft.AspNetCore.Components;

namespace JazorAdmin;

[ECMAScriptModule("./components/jazor-admin-tdesign-page-container")]
public partial class TDesignPageContainer : AdminContentComponentBase
{
    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public string? Subtitle { get; set; }

    [Parameter]
    public AdminBreadcrumbItem[]? BreadcrumbItems { get; set; }

    [Parameter]
    public AdminPageAction[]? Actions { get; set; }

    [Parameter]
    public RenderFragment? Extra { get; set; }

    private const string RootCssClass = "jazor-admin-tdesign-page-container";

    private static TDesignButtonTheme? MapTheme(AdminPageActionKind? kind) => kind switch
    {
        AdminPageActionKind.Primary => TDesignButtonTheme.Primary,
        AdminPageActionKind.Danger => TDesignButtonTheme.Danger,
        AdminPageActionKind.Secondary => TDesignButtonTheme.Default,
        AdminPageActionKind.Link => TDesignButtonTheme.Default,
        _ => TDesignButtonTheme.Default
    };

    private static string? MapHref(string? href, RouteLocationRaw? routeTarget)
        => TDesignRouteMapper.MapHref(href, routeTarget);

    private static TDesignMenuRouteTarget? MapRoute(RouteLocationRaw? routeTarget)
        => TDesignRouteMapper.MapRoute(routeTarget);

    private static string? MapActionHref(AdminPageAction action)
        => TDesignRouteMapper.MapActionHref(action.Href, action.RouteTarget);

    private static Task InvokeAction(AdminPageAction action)
        => action.Click.InvokeAsync();
}
