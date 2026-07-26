using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vben.TDesign;

[ECMAScriptModule("./components/vben-tdesign-admin-layout")]
public partial class VbenTDesignAdminLayout : VbenContentComponentBase
{
    [Parameter]
    public VbenLayoutMode Mode { get; set; } = VbenLayoutMode.Sidebar;

    [Parameter]
    public bool Collapsed { get; set; }

    [Parameter]
    public EventCallback<bool> CollapsedChanged { get; set; }

    [Parameter]
    public string? SelectedKey { get; set; }

    [Parameter]
    public EventCallback<string> SelectedKeyChanged { get; set; }

    [Parameter]
    public string[]? ExpandedKeys { get; set; }

    [Parameter]
    public EventCallback<string[]> ExpandedKeysChanged { get; set; }

    [Parameter]
    public VbenNavItems? NavItems { get; set; }

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

    private bool IsSidebarLayout => Mode != VbenLayoutMode.Top;

    private VueClassValue RootCssClass => Mode switch
    {
        VbenLayoutMode.Top => BuildCssClass("vben-tdesign-layout", "vben-tdesign-layout--top"),
        VbenLayoutMode.Mixed => BuildCssClass("vben-tdesign-layout", "vben-tdesign-layout--mixed"),
        _ => BuildCssClass("vben-tdesign-layout", "vben-tdesign-layout--sidebar")
    };
}
