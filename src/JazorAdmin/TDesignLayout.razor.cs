// Extends the TDesign layout hierarchy with a fixed icon rail and scoped secondary navigation.
// 在 TDesign 布局层级上增加固定图标 rail 与范围化二级导航。
using ECMAScript;
using Microsoft.AspNetCore.Components;
using static ECMAScript.VueRoute;

namespace JazorAdmin;

[ECMAScriptModule("./components/jazor-admin-tdesign-admin-layout")]
public partial class TDesignLayout : AdminContentComponentBase
{
    [Parameter]
    public AdminLayoutMode Mode { get; set; } = AdminLayoutMode.Sidebar;

    [Parameter]
    public bool Collapsed { get; set; }

    [Parameter]
    public EventCallback<bool> CollapsedChanged { get; set; }

    [Parameter]
    public string? CollapseLabel { get; set; }

    [Parameter]
    public string? ExpandLabel { get; set; }

    [Parameter]
    public string? SelectedKey { get; set; }

    [Parameter]
    public EventCallback<string> SelectedKeyChanged { get; set; }

    [Parameter]
    public string[]? ExpandedKeys { get; set; }

    [Parameter]
    public EventCallback<string[]> ExpandedKeysChanged { get; set; }

    [Parameter]
    public AdminNavItems? NavItems { get; set; }

    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public string? Subtitle { get; set; }

    [Parameter]
    public RenderFragment? Logo { get; set; }

    [Parameter]
    public RenderFragment? Header { get; set; }

    [Parameter]
    public RenderFragment? Sidebar { get; set; }

    [Parameter]
    public RenderFragment? HeaderActions { get; set; }

    [Parameter]
    public RenderFragment? UserRegion { get; set; }

    private bool IsSidebarLayout => Mode != AdminLayoutMode.Top;

    private string LayoutContractValue => Mode switch
    {
        AdminLayoutMode.Top => "top",
        AdminLayoutMode.Mixed => "mixed",
        _ => "sidebar"
    };

    private string RootCssClass => Mode switch
    {
        AdminLayoutMode.Top => "jazor-admin-tdesign-layout jazor-admin-tdesign-layout--top",
        AdminLayoutMode.Mixed => "jazor-admin-tdesign-layout jazor-admin-tdesign-layout--mixed",
        _ => "jazor-admin-tdesign-layout jazor-admin-tdesign-layout--sidebar"
    };

    private string EffectiveCollapseLabel
        => Collapsed ? ExpandLabel ?? "Expand sidebar" : CollapseLabel ?? "Collapse sidebar";

    private static RouteLocationRaw DashboardRoute => (RouteLocationRaw)"/";

    private static RouteLocationRaw OrganizationsRoute => (RouteLocationRaw)"/organizations/structure";

    private static RouteLocationRaw AuthorizationRoute => (RouteLocationRaw)"/authorization/roles";

    private static RouteLocationRaw AccountsRoute => (RouteLocationRaw)"/accounts";

    private static RouteLocationRaw ConfigurationRoute => (RouteLocationRaw)"/configuration/clients";

    private string DashboardRailClass => GetRailItemClass("dashboard");

    private string OrganizationsRailClass => GetRailItemClass("organizations");

    private string AuthorizationRailClass => GetRailItemClass("authorization");

    private string AccountsRailClass => GetRailItemClass("accounts");

    private string ConfigurationRailClass => GetRailItemClass("configuration");

    // RouterLink is emitted through the component builder because Razor's tag parser
    // does not bind this proxy consistently in the package-consumer compilation.
    private RenderFragment SidebarRail => builder =>
    {
        builder.OpenElement(0, "nav");
        builder.AddAttribute(1, "class", "jazor-admin-tdesign-sidebar-rail");
        builder.AddAttribute(2, "aria-label", "Primary navigation");
        builder.AddContent(3, RenderRailItem(DashboardRoute, DashboardRailClass, "dashboard", "Dashboard"));
        builder.AddContent(4, RenderRailItem(OrganizationsRoute, OrganizationsRailClass, "organizations", "Organizations"));
        builder.AddContent(5, RenderRailItem(AuthorizationRoute, AuthorizationRailClass, "authorization", "Authorization"));
        builder.AddContent(6, RenderRailItem(AccountsRoute, AccountsRailClass, "accounts", "Accounts"));
        builder.AddContent(7, RenderRailItem(ConfigurationRoute, ConfigurationRailClass, "configuration", "Configuration"));
        builder.CloseElement();
    };

    private static RenderFragment RenderRailItem(
        RouteLocationRaw route,
        string cssClass,
        string section,
        string label) => builder =>
    {
        builder.OpenComponent<VueRouterLink>(0);
        builder.AddComponentParameter(1, nameof(VueRouterLink.To), route);
        builder.AddComponentParameter(2, nameof(VueRouterLink.CssClass), (VueClassValue)cssClass);
        builder.AddComponentParameter(3, "data-rail-section", section);
        builder.AddComponentParameter(4, "aria-label", label);
        builder.AddComponentParameter(5, "title", label);
        builder.AddComponentParameter(
            6,
            nameof(VueRouterLink.ChildContent),
            (RenderFragment)(childBuilder =>
            {
                childBuilder.OpenElement(0, "span");
                childBuilder.AddAttribute(1, "class", "jazor-admin-tdesign-sidebar-rail__icon");
                childBuilder.AddAttribute(2, "data-rail-icon", section);
                childBuilder.AddAttribute(3, "aria-hidden", "true");
                childBuilder.CloseElement();
            }));
        builder.CloseComponent();
    };

    private string PrimarySection => SelectedKey switch
    {
        "organizations.structure" or "organizations.members" => "organizations",
        "authorization.roles" or "authorization.resources" => "authorization",
        "accounts" => "accounts",
        "configuration.clients" or "configuration.scopes" => "configuration",
        _ => "dashboard"
    };

    private string SecondaryTitle => PrimarySection switch
    {
        "organizations" => "Organizations",
        "authorization" => "Authorization",
        "accounts" => "Accounts",
        "configuration" => "Configuration",
        _ => "Overview"
    };

    private AdminNavItems? SecondaryNavItems
    {
        get
        {
            if (NavItems?.AsArray is not { Length: > 0 } items)
                return null;

            foreach (var item in items)
            {
                if (item.Key != PrimarySection)
                    continue;

                return item.Children?.AsArray is { Length: > 0 } children
                    ? children
                    : new AdminNavItem[] { item };
            }

            return NavItems;
        }
    }

    // The rail selects a complete work area. Its menu is deliberately derived from the same
    // route catalog so the two navigation layers cannot drift into incompatible targets.
    // rail 选择完整工作域；二级菜单同源于路由目录，避免两层导航出现不一致的目标。
    private string GetRailItemClass(string section)
    {
        var isSelected = section == PrimarySection;

        return isSelected
            ? "jazor-admin-tdesign-sidebar-rail__link is-selected"
            : "jazor-admin-tdesign-sidebar-rail__link";
    }

    private RenderFragment? HeaderNavigation => Mode == AdminLayoutMode.Top
        ? builder =>
        {
            builder.OpenComponent<TDesignSidebarMenu>(0);
            builder.AddComponentParameter(1, nameof(TDesignSidebarMenu.Items), SecondaryNavItems);
            builder.AddComponentParameter(2, nameof(TDesignSidebarMenu.SelectedKey), SelectedKey);
            builder.AddComponentParameter(3, nameof(TDesignSidebarMenu.ExpandedKeys), ExpandedKeys);
            builder.AddComponentParameter(4, nameof(TDesignSidebarMenu.SelectedKeyChanged), SelectedKeyChanged);
            builder.AddComponentParameter(5, nameof(TDesignSidebarMenu.ExpandedKeysChanged), ExpandedKeysChanged);
            builder.AddComponentParameter(6, nameof(TDesignSidebarMenu.Horizontal), true);
            builder.CloseComponent();
        }
        : null;

    private Task ToggleCollapsed(MouseEvent _)
        => CollapsedChanged.InvokeAsync(!Collapsed);
}
