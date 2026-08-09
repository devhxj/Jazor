// Composes a Starter-style IconBar with a scoped TDesign secondary menu.
// IconBar 与二级菜单共享导航目录，Layout 只负责布局和折叠状态。
using Microsoft.AspNetCore.Components;

namespace JazorAdmin;

[ECMAScriptModule("./components/tdesign/layout")]
public partial class TDesignLayout : AdminContentComponentBase
{
    [Parameter]
    public AdminLayoutMode Mode { get; set; } = AdminLayoutMode.Sidebar;

    [Parameter]
    public AdminThemeMode Theme { get; set; } = AdminThemeMode.Light;

    [Parameter]
    public AdminThemeMode SidebarTheme { get; set; } = AdminThemeMode.Light;

    [Parameter]
    public bool SplitMenu { get; set; }

    [Parameter]
    public bool SidebarFixed { get; set; } = true;

    [Parameter]
    public bool ShowHeader { get; set; } = true;

    [Parameter]
    public bool ShowBreadcrumb { get; set; } = true;

    [Parameter]
    public bool ShowTabs { get; set; } = true;

    [Parameter]
    public bool ShowFooter { get; set; }

    [Parameter]
    public bool MenuAutoCollapsed { get; set; }

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
    public RenderFragment? Tabs { get; set; }

    [Parameter]
    public RenderFragment? Breadcrumb { get; set; }

    [Parameter]
    public RenderFragment? Footer { get; set; }

    [Parameter]
    public RenderFragment? Sidebar { get; set; }

    [Parameter]
    public RenderFragment? HeaderActions { get; set; }

    [Parameter]
    public RenderFragment? IconBarActions { get; set; }

    [Parameter]
    public string? QuickActionsLabel { get; set; }

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
        AdminLayoutMode.Top => "ja-tdesign-layout ja-tdesign-layout--top",
        AdminLayoutMode.Mixed => "ja-tdesign-layout ja-tdesign-layout--mixed",
        _ => "ja-tdesign-layout ja-tdesign-layout--sidebar"
    };

    private VueStyleValue LayoutCssStyle => SurfaceStyle;

    private VueStyleValue ContentCssStyle => SurfaceStyle;

    private string SidebarWidth => Mode == AdminLayoutMode.Mixed
        ? Collapsed ? "64px" : "296px"
        : Collapsed ? "64px" : "232px";

    private VueStyleValue SurfaceStyle => Theme switch
    {
        AdminThemeMode.Dark => "background: #181818;",
        AdminThemeMode.Light => "background: #f3f3f3;",
        _ => "background: var(--app-bg);"
    };

    private string EffectiveCollapseLabel
        => Collapsed ? ExpandLabel ?? "Expand sidebar" : CollapseLabel ?? "Collapse sidebar";

    private AdminNavItem? ActivePrimaryItem
    {
        get
        {
            if (NavItems?.AsArray is not { Length: > 0 } items)
                return null;

            foreach (var item in items)
            {
                if (ContainsSelectedItem(item, SelectedKey))
                    return item;
            }

            return items[0];
        }
    }

    private string ActivePrimaryTitle
    {
        get
        {
            var item = ActivePrimaryItem;
            if (item is null)
                return Title ?? string.Empty;

            return string.IsNullOrWhiteSpace(item.Title) ? item.Key : item.Title;
        }
    }

    private AdminNavItems? SecondaryNavItems
    {
        get
        {
            var item = ActivePrimaryItem;
            if (item is null)
                return null;

            if (!SplitMenu)
                return NavItems;

            return item.Children?.AsArray is { Length: > 0 } children
                ? children
                : new AdminNavItem[] { item };
        }
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

    private RenderFragment? HeaderNavigation => Mode == AdminLayoutMode.Top
        ? builder =>
        {
            builder.OpenComponent<TDesignSidebarMenu>(0);
            builder.AddComponentParameter(1, nameof(TDesignSidebarMenu.Items), NavItems);
            builder.AddComponentParameter(2, nameof(TDesignSidebarMenu.SelectedKey), SelectedKey);
            builder.AddComponentParameter(3, nameof(TDesignSidebarMenu.ExpandedKeys), ExpandedKeys);
            builder.AddComponentParameter(4, nameof(TDesignSidebarMenu.SelectedKeyChanged), SelectedKeyChanged);
            builder.AddComponentParameter(5, nameof(TDesignSidebarMenu.ExpandedKeysChanged), ExpandedKeysChanged);
            builder.AddComponentParameter(6, nameof(TDesignSidebarMenu.Inline), true);
            builder.AddComponentParameter(7, nameof(TDesignSidebarMenu.Theme), Theme);
            builder.CloseComponent();
        }
        : null;

    private TMenuValue? HeaderNavigationValue
        => Mode != AdminLayoutMode.Top || SelectedKey is null ? null : (TMenuValue)SelectedKey;

    private TMenuValue[]? HeaderNavigationExpanded
        => Mode != AdminLayoutMode.Top || ExpandedKeys is null
            ? null
            : Array.ConvertAll(ExpandedKeys, static key => (TMenuValue)key);

    private Task OnHeaderNavigationChanged(TMenuValue value)
        => value.Value is string key
            ? SelectedKeyChanged.InvokeAsync(key)
            : Task.CompletedTask;

    private Task OnHeaderNavigationExpanded(TMenuValue[] values)
    {
        var expandedKeys = new List<string>();
        foreach (var value in values)
        {
            if (value.Value is string key && !string.IsNullOrWhiteSpace(key))
                expandedKeys.Add(key);
        }

        return ExpandedKeysChanged.InvokeAsync(expandedKeys.ToArray());
    }

    private RenderFragment? HeaderLeading => IsSidebarLayout
        ? builder =>
        {
            builder.OpenComponent<TButton>(0);
            builder.AddComponentParameter(1, nameof(TButton.Variant), TButtonVariantValue.Text);
            builder.AddComponentParameter(2, "data-shell-command", "toggle-sidebar");
            builder.AddComponentParameter(3, "aria-label", EffectiveCollapseLabel);
            builder.AddComponentParameter(4, "title", EffectiveCollapseLabel);
            builder.AddComponentParameter(5, "aria-expanded", !Collapsed);
            builder.AddComponentParameter(6, nameof(TButton.OnClick), EventCallback.Factory.Create<MouseEvent>(this, ToggleCollapsed));
            builder.AddComponentParameter(
                7,
                nameof(TButton.IconContent),
                (RenderFragment)(iconBuilder =>
                {
                    iconBuilder.OpenComponent<TIcon>(0);
                    iconBuilder.AddComponentParameter(1, nameof(TIcon.Name), Collapsed ? "menu-unfold" : "menu-fold");
                    iconBuilder.AddComponentParameter(2, nameof(TIcon.Size), "20px");
                    iconBuilder.AddComponentParameter(3, "aria-hidden", "true");
                    iconBuilder.CloseComponent();
                }));
            builder.CloseComponent();
        }
        : null;

    private Task ToggleCollapsed(MouseEvent _)
        => CollapsedChanged.InvokeAsync(!Collapsed);
}
