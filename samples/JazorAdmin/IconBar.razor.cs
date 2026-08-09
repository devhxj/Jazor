using ECMAScript;
using Microsoft.AspNetCore.Components;
using static ECMAScript.VueRoute;

namespace JazorAdmin;

/// <summary>
/// Renders the primary work areas as a collapsed TDesign menu.
/// 一级和二级菜单必须共享同一份 <see cref="AdminNavItems" />，避免路由、标题和选中态漂移。
/// </summary>
[ECMAScriptModule("./components/iconbar")]
public partial class IconBar : AdminComponentBase
{
    [Parameter]
    public AdminNavItems? Items { get; set; }

    [Parameter]
    public string? SelectedKey { get; set; }

    [Parameter]
    public AdminThemeMode Theme { get; set; } = AdminThemeMode.Light;

    [Parameter]
    public string? AriaLabel { get; set; }

    [Parameter]
    public string? QuickActionsLabel { get; set; }

    [Parameter]
    public RenderFragment? QuickActions { get; set; }

    private const string DesktopCssClass = "ja-iconbar ja-iconbar--rail";
    private const string MobileCssClass = "ja-iconbar ja-iconbar--head";
    private const string QuickActionsOverlayClass = "ja-iconbar__quick-popup";

    private static readonly TMenuWidthValue DesktopWidth = (TMenuWidthValue)"64px";

    private AdminNavItem[] EffectiveItems => Items?.AsArray ?? [];

    private TMenuValue? MenuValue
        => string.IsNullOrWhiteSpace(SelectedSectionKey) ? null : (TMenuValue)SelectedSectionKey;

    private string EffectiveAriaLabel => AriaLabel ?? "Primary navigation";

    private string EffectiveQuickActionsLabel => QuickActionsLabel ?? "Quick actions";

    private TMenuThemeValue MenuTheme
        => Theme == AdminThemeMode.Dark ? TMenuThemeValue.Dark : TMenuThemeValue.Light;

    private THeadMenuThemeValue HeadMenuTheme
        => Theme == AdminThemeMode.Dark ? THeadMenuThemeValue.Dark : THeadMenuThemeValue.Light;

    private string SelectedSectionKey
    {
        get
        {
            foreach (var item in EffectiveItems)
            {
                if (ContainsSelectedItem(item, SelectedKey))
                    return item.Key;
            }

            return string.Empty;
        }
    }

    // The builder keeps TMenuItem's route and icon slots on the same code path for rail and head menus.
    private RenderFragment RenderItem(AdminNavItem item) => builder =>
    {
        var route = FindFirstRoute(item);
        if (!route.HasValue)
            return;

        var selected = ContainsSelectedItem(item, SelectedKey);
        var menuRoute = TDesignRouteMapper.MapMenuRoute(route);
        if (!menuRoute.HasValue)
            return;

        builder.OpenComponent<TMenuItem>(0);
        builder.AddComponentParameter(1, nameof(TMenuItem.Value), (TMenuValue)item.Key);
        builder.AddComponentParameter(2, nameof(TMenuItem.To), menuRoute.Value);
        builder.AddComponentParameter(3, nameof(TMenuItem.RouterLink), true);
        builder.AddComponentParameter(4, nameof(TMenuItem.Disabled), item.Disabled ?? false);
        builder.AddComponentParameter(5, "data-iconbar-key", item.Key);
        builder.AddComponentParameter(6, "data-iconbar-selected", selected);
        builder.AddComponentParameter(7, "aria-label", item.Title ?? item.Key);
        builder.AddComponentParameter(8, "aria-current", selected ? "page" : null);
        builder.AddComponentParameter(9, "title", item.Title ?? item.Key);
        builder.AddComponentParameter(
            10,
            nameof(TMenuItem.IconContent),
            (RenderFragment)(childBuilder =>
            {
                // Keep icon rendering inside the TDesign slot: its collapsed-menu tooltip,
                // size and theme handling then remain consistent with Starter.
                childBuilder.OpenComponent<TIcon>(0);
                childBuilder.AddComponentParameter(1, nameof(TIcon.Name), GetIconName(item));
                childBuilder.AddComponentParameter(2, nameof(TIcon.Size), "20px");
                childBuilder.AddComponentParameter(3, "aria-hidden", "true");
                childBuilder.CloseComponent();
            }));
        builder.AddComponentParameter(
            11,
            nameof(TMenuItem.ContentSlot),
            (RenderFragment)(childBuilder => childBuilder.AddContent(0, item.Title ?? item.Key)));
        builder.CloseComponent();
    };

    private static string GetIconName(AdminNavItem item) => item.Icon ?? item.Key switch
    {
        "dashboard" => "dashboard",
        _ => "application"
    };

    private static RouteLocationRaw? FindFirstRoute(AdminNavItem item)
    {
        if (item.RouteTarget.HasValue)
            return item.RouteTarget;

        if (item.Children?.AsArray is not { Length: > 0 } children)
            return null;

        foreach (var child in children)
        {
            var route = FindFirstRoute(child);
            if (route.HasValue)
                return route;
        }

        return null;
    }

    private static bool ContainsSelectedItem(AdminNavItem item, string? selectedKey)
    {
        if (item.Key == selectedKey)
            return true;

        if (item.Children?.AsArray is not { Length: > 0 } children)
            return false;

        foreach (var child in children)
        {
            if (ContainsSelectedItem(child, selectedKey))
                return true;
        }

        return false;
    }
}
