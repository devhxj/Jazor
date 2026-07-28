using Microsoft.AspNetCore.Components;

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

    private RenderFragment? HeaderNavigation => Mode == AdminLayoutMode.Top
        ? builder =>
        {
            builder.OpenComponent<TDesignSidebarMenu>(0);
            builder.AddComponentParameter(1, nameof(TDesignSidebarMenu.Items), NavItems);
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
