using ECMAScript;
using Microsoft.AspNetCore.Components;
using static ECMAScript.VueRoute;

namespace JazorAdmin;

/// <summary>
/// Renders one icon-only entry for each top-level administration work area.
/// 一级入口和二级菜单必须共享同一份 <see cref="AdminNavItems" />，避免路由、标题和选中态漂移。
/// </summary>
[ECMAScriptModule("./components/iconbar")]
public partial class IconBar : AdminComponentBase
{
    [Parameter]
    public AdminNavItems? Items { get; set; }

    [Parameter]
    public string? SelectedKey { get; set; }

    [Parameter]
    public string? AriaLabel { get; set; }

    private const string RootCssClass = "jazor-admin-iconbar";

    private AdminNavItem[] EffectiveItems => Items?.AsArray ?? [];

    private string EffectiveAriaLabel => AriaLabel ?? "Primary navigation";

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

    // VueRouterLink is emitted through the component builder because Razor's tag parser
    // does not bind this proxy consistently in the package-consumer compilation.
    private RenderFragment RenderItem(AdminNavItem item) => builder =>
    {
        var route = FindFirstRoute(item);
        if (!route.HasValue)
            return;

        var selected = ContainsSelectedItem(item, SelectedKey);
        builder.OpenComponent<VueRouterLink>(0);
        builder.AddComponentParameter(1, nameof(VueRouterLink.To), route.Value);
        builder.AddComponentParameter(
            2,
            nameof(VueRouterLink.CssClass),
            (VueClassValue)(selected ? "jazor-admin-iconbar__link is-selected" : "jazor-admin-iconbar__link"));
        builder.AddComponentParameter(3, "data-iconbar-key", item.Key);
        builder.AddComponentParameter(4, "data-iconbar-selected", selected);
        builder.AddComponentParameter(5, "aria-label", item.Title ?? item.Key);
        builder.AddComponentParameter(6, "aria-current", selected ? "true" : null);
        builder.AddComponentParameter(7, "title", item.Title ?? item.Key);
        builder.AddComponentParameter(
            8,
            nameof(VueRouterLink.ChildContent),
            (RenderFragment)(childBuilder =>
            {
                childBuilder.OpenElement(0, "span");
                childBuilder.AddAttribute(1, "class", "jazor-admin-iconbar__icon");
                childBuilder.AddAttribute(2, "data-iconbar-icon", item.Icon ?? item.Key);
                childBuilder.AddAttribute(3, "aria-hidden", "true");
                childBuilder.CloseElement();
            }));
        builder.CloseComponent();
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
