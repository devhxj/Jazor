namespace Jazor.Admin;

[ECMAScriptModule("./components/admin/layout")]
public partial class JLayout : JContentComponentBase, IVueContainerComponent
{
    [Parameter]
    /// <summary>Controls whether navigation is rendered in top, sidebar, or mixed mode.</summary>
    public AdminLayoutMode Mode { get; set; } = AdminLayoutMode.Sidebar;

    [Parameter]
    /// <summary>Controlled desktop sidebar state; mobile uses the internal overlay state.</summary>
    public bool Collapsed { get; set; }

    [Parameter]
    public EventCallback<bool> CollapsedChanged { get; set; }

    [Parameter]
    public string CollapseLabel { get; set; } = "Collapse sidebar";

    [Parameter]
    public string ExpandLabel { get; set; } = "Expand sidebar";

    [Parameter]
    /// <summary>Currently selected navigation key.</summary>
    public string? SelectedKey { get; set; }

    [Parameter]
    public EventCallback<string> SelectedKeyChanged { get; set; }

    [Parameter]
    public string[]? ExpandedKeys { get; set; }

    [Parameter]
    public EventCallback<string[]> ExpandedKeysChanged { get; set; }

    [Parameter]
    /// <summary>Navigation tree used when the <see cref="Sidebar"/> slot is not supplied.</summary>
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

    /// <summary>Accessible name applied to the default sidebar navigation landmark.</summary>
    [Parameter]
    public string NavigationLabel { get; set; } = "Primary navigation";

    // 与 AdminStyleSheet 的 mobile media query 断点保持一致；在此宽度以下侧栏是
    // overlay drawer，不再是可折叠的 grid 列。类效果被 media query 限定，桌面布局忽略该状态。
    private const string MobileBreakpointQuery = "(max-width: 760px)";

    private bool mobileSidebarOpen;

    private bool IsSidebarLayout => Mode != AdminLayoutMode.Top;

    // nav-item.mjs 只导出成员函数；渲染位直接引用 AdminNavItemRenderHelper 会触发 phantom
    // 类名导入，因此经成员位置间接判定导航存在性。
    private bool HasNavigationItems
        => AdminNavItemRenderHelper.BuildEffectiveItems(NavItems?.AsArray).Length > 0;

    private bool IsMobileViewport => Window.MatchMedia(MobileBreakpointQuery).Matches;

    private VueClassValue RootCssClass
    {
        get
        {
            var classes = Mode switch
            {
                AdminLayoutMode.Top => new[] { "ja-shell", "ja-shell--top" },
                AdminLayoutMode.Mixed when Collapsed => new[] { "ja-shell", "ja-shell--mixed", "ja-shell--collapsed" },
                AdminLayoutMode.Mixed => new[] { "ja-shell", "ja-shell--mixed" },
                _ when Collapsed => new[] { "ja-shell", "ja-shell--sidebar", "ja-shell--collapsed" },
                _ => new[] { "ja-shell", "ja-shell--sidebar" }
            };

            if (mobileSidebarOpen)
            {
                return BuildCssClass([.. classes, "ja-shell--mobile-open"]);
            }

            return BuildCssClass(classes);
        }
    }

    private string SidebarToggleLabel
        => Collapsed
            ? AdminDisplayTextHelper.Normalize(ExpandLabel) ?? "Expand sidebar"
            : AdminDisplayTextHelper.Normalize(CollapseLabel) ?? "Collapse sidebar";

    private VueClassValue HorizontalSidebarCssClass
        => (VueClassValue)"ja-sidebar--horizontal";

    // The top-mode header owns the horizontal navigation slot; keep this fragment in code
    // behind so the Razor template remains declarative while the component parameters stay
    // explicitly typed for Razor SG.
    private RenderFragment HorizontalNavigation => builder =>
    {
        builder.OpenComponent<JSidebar>(0);
        builder.AddComponentParameter(1, nameof(JComponentBase.CssClass), HorizontalSidebarCssClass);
        builder.AddComponentParameter(2, nameof(JSidebar.Items), NavItems);
        builder.AddComponentParameter(3, nameof(JSidebar.NavigationLabel), NavigationLabel);
        builder.AddComponentParameter(4, nameof(JSidebar.ExpandLabel), ExpandLabel);
        builder.AddComponentParameter(5, nameof(JSidebar.CollapseLabel), CollapseLabel);
        builder.AddComponentParameter(6, nameof(JSidebar.SelectedKey), SelectedKey);
        builder.AddComponentParameter(7, nameof(JSidebar.ExpandedKeys), ExpandedKeys);
        builder.AddComponentParameter(8, nameof(JSidebar.SelectedKeyChanged), SelectedKeyChanged);
        builder.AddComponentParameter(9, nameof(JSidebar.ExpandedKeysChanged), ExpandedKeysChanged);
        builder.CloseComponent();
    };

    private Task ToggleSidebar()
    {
        // 窄视口下同一个按钮驱动 overlay drawer；桌面视口维持原折叠列契约。
        if (IsMobileViewport)
        {
            mobileSidebarOpen = !mobileSidebarOpen;
            return Task.CompletedTask;
        }

        return CollapsedChanged.InvokeAsync(!Collapsed);
    }

    private Task CloseMobileSidebar()
    {
        mobileSidebarOpen = false;
        return Task.CompletedTask;
    }

    private async Task OnNavigationSelected(string key)
    {
        mobileSidebarOpen = false;
        await SelectedKeyChanged.InvokeAsync(key);
    }
}
