namespace Jazor.Admin;

[ECMAScriptModule("./components/admin/layout")]
public partial class AdminLayout : AdminContentComponentBase, IVueContainerComponent
{
    [Parameter]
    public AdminLayoutMode Mode { get; set; } = AdminLayoutMode.Sidebar;

    [Parameter]
    public bool Collapsed { get; set; }

    [Parameter]
    public EventCallback<bool> CollapsedChanged { get; set; }

    [Parameter]
    public string CollapseLabel { get; set; } = "Collapse sidebar";

    [Parameter]
    public string ExpandLabel { get; set; } = "Expand sidebar";

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

    // 与 AdminStyleSheet 的 mobile media query 断点保持一致；在此宽度以下侧栏是
    // overlay drawer，不再是可折叠的 grid 列。类效果被 media query 限定，桌面布局忽略该状态。
    private const string MobileBreakpointQuery = "(max-width: 760px)";

    private bool mobileSidebarOpen;

    private bool IsSidebarLayout => Mode != AdminLayoutMode.Top;

    // nav-item.mjs 只导出成员函数；渲染位直接引用 AdminNavItemRenderHelper 会触发 phantom
    // 类名导入，因此经成员位置间接判定导航存在性。
    private bool HasNavigationItems
        => AdminNavItemRenderHelper.BuildEffectiveItems(NavItems?.AsArray).Length > 0;

    private bool IsMobileViewport => Global.Window.MatchMedia(MobileBreakpointQuery).Matches;

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

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var logo = Logo;
        var header = Header;
        var sidebar = Sidebar;
        var headerActions = HeaderActions;
        var userRegion = UserRegion;
        var hasNavigationItems = HasNavigationItems;
        var hasDefaultSidebarContent = logo is not null || hasNavigationItems;
        var hasSidebarRegion = IsSidebarLayout && (sidebar is not null || hasDefaultSidebarContent);
        var defaultHeaderLogo = IsSidebarLayout ? null : logo;
        var hasDefaultHeaderContent =
            !string.IsNullOrWhiteSpace(Title)
            || !string.IsNullOrWhiteSpace(Subtitle)
            || defaultHeaderLogo is not null
            || (!IsSidebarLayout && hasNavigationItems)
            || headerActions is not null
            || userRegion is not null;
        var hasHeaderRegion = header is not null || hasDefaultHeaderContent;

        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", RootCssClass);
        builder.AddAttribute(2, "style", CssStyle);
        builder.AddMultipleAttributes(3, AdditionalAttributes);

        if (hasSidebarRegion)
        {
            if (mobileSidebarOpen)
            {
                // Backdrop 仅在 mobile media query 内可见；桌面视口下保持 display:none。
                builder.OpenElement(44, "div");
                builder.AddAttribute(45, "class", "ja-shell__mobile-backdrop");
                builder.AddAttribute(46, "aria-hidden", true);
                builder.AddAttribute(47, "onclick", EventCallback.Factory.Create(this, CloseMobileSidebar));
                builder.CloseElement();
            }

            builder.OpenElement(4, "aside");
            builder.AddAttribute(5, "class", "ja-shell__sidebar");
            if (sidebar is not null)
            {
                builder.AddContent(6, sidebar);
            }
            else
            {
                builder.OpenComponent<SidebarMenu>(7);
                builder.AddComponentParameter(8, nameof(SidebarMenu.Items), NavItems);
                builder.AddComponentParameter(9, nameof(SidebarMenu.Collapsed), Collapsed);
                builder.AddComponentParameter(10, nameof(SidebarMenu.SelectedKey), SelectedKey);
                builder.AddComponentParameter(11, nameof(SidebarMenu.ExpandedKeys), ExpandedKeys);
                // 导航选中后关闭移动端抽屉，同时保持对外 SelectedKeyChanged 契约不变。
                builder.AddComponentParameter(12, nameof(SidebarMenu.SelectedKeyChanged), EventCallback.Factory.Create<string>(this, OnNavigationSelected));
                builder.AddComponentParameter(13, nameof(SidebarMenu.ExpandedKeysChanged), ExpandedKeysChanged);
                builder.AddComponentParameter(14, nameof(SidebarMenu.Logo), logo);
                builder.CloseComponent();
            }
            builder.CloseElement();
        }

        builder.OpenElement(15, "div");
        builder.AddAttribute(16, "class", "ja-shell__main");

        if (hasHeaderRegion)
        {
            builder.OpenElement(17, "header");
            builder.AddAttribute(18, "class", "ja-shell__header");
            if (header is not null)
            {
                builder.AddContent(19, header);
            }
            else
            {
                if (IsSidebarLayout)
                {
                    builder.OpenElement(20, "button");
                    builder.AddAttribute(21, "type", "button");
                    builder.AddAttribute(22, "class", "ja-shell__sidebar-toggle");
                    builder.AddAttribute(23, "data-shell-command", "toggle-sidebar");
                    builder.AddAttribute(24, "aria-label", SidebarToggleLabel);
                    builder.AddAttribute(25, "title", SidebarToggleLabel);
                    builder.AddAttribute(26, "aria-expanded", mobileSidebarOpen || !Collapsed);
                    builder.AddAttribute(27, "onclick", EventCallback.Factory.Create(this, ToggleSidebar));
                    builder.CloseElement();
                }

                if (!IsSidebarLayout && hasNavigationItems)
                {
                    builder.OpenComponent<HeaderBar>(28);
                    builder.SetKey(Mode);
                    builder.AddComponentParameter(29, nameof(HeaderBar.Title), Title);
                    builder.AddComponentParameter(30, nameof(HeaderBar.Subtitle), Subtitle);
                    builder.AddComponentParameter(31, nameof(HeaderBar.Logo), defaultHeaderLogo);
                    builder.AddComponentParameter(32, nameof(HeaderBar.Navigation), (RenderFragment)(navigationBuilder =>
                    {
                        navigationBuilder.OpenComponent<SidebarMenu>(0);
                        navigationBuilder.AddComponentParameter(1, nameof(AdminComponentBase.CssClass), (VueClassValue)"ja-sidebar--horizontal");
                        navigationBuilder.AddComponentParameter(2, nameof(SidebarMenu.Items), NavItems);
                        navigationBuilder.AddComponentParameter(3, nameof(SidebarMenu.SelectedKey), SelectedKey);
                        navigationBuilder.AddComponentParameter(4, nameof(SidebarMenu.ExpandedKeys), ExpandedKeys);
                        navigationBuilder.AddComponentParameter(5, nameof(SidebarMenu.SelectedKeyChanged), SelectedKeyChanged);
                        navigationBuilder.AddComponentParameter(6, nameof(SidebarMenu.ExpandedKeysChanged), ExpandedKeysChanged);
                        navigationBuilder.CloseComponent();
                    }));
                    builder.AddComponentParameter(33, nameof(HeaderBar.Actions), headerActions);
                    builder.AddComponentParameter(34, nameof(HeaderBar.UserRegion), userRegion);
                    builder.CloseComponent();
                }
                else
                {
                    builder.OpenComponent<HeaderBar>(35);
                    builder.SetKey(Mode);
                    builder.AddComponentParameter(36, nameof(HeaderBar.Title), Title);
                    builder.AddComponentParameter(37, nameof(HeaderBar.Subtitle), Subtitle);
                    builder.AddComponentParameter(38, nameof(HeaderBar.Logo), defaultHeaderLogo);
                    builder.AddComponentParameter(39, nameof(HeaderBar.Actions), headerActions);
                    builder.AddComponentParameter(40, nameof(HeaderBar.UserRegion), userRegion);
                    builder.CloseComponent();
                }
            }
            builder.CloseElement();
        }

        builder.OpenElement(41, "main");
        builder.AddAttribute(42, "class", "ja-shell__content");
        builder.AddContent(43, ChildContent);
        builder.CloseElement();

        builder.CloseElement();
        builder.CloseElement();
    }

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
