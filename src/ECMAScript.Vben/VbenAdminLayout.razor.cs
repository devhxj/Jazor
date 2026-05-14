namespace ECMAScript.Vben;

[ECMAScriptModule("./components/vben-admin-layout")]
public partial class VbenAdminLayout : VbenContentComponentBase, IVueContainerComponent
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

    private VueClassValue RootCssClass => Mode switch
    {
        VbenLayoutMode.Top => BuildCssClass("vben-shell", "vben-shell--top"),
        VbenLayoutMode.Mixed => BuildCssClass("vben-shell", "vben-shell--mixed"),
        _ => BuildCssClass("vben-shell", "vben-shell--sidebar")
    };
}
