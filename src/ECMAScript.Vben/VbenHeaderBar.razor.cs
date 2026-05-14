namespace ECMAScript.Vben;

[ECMAScriptModule("./components/vben-header-bar")]
public partial class VbenHeaderBar : VbenComponentBase
{
    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public string? Subtitle { get; set; }

    [Parameter]
    public RenderFragment? Logo { get; set; }

    [Parameter]
    public RenderFragment? Actions { get; set; }

    [Parameter]
    public RenderFragment? UserRegion { get; set; }

    private VueClassValue RootCssClass
        => BuildCssClass("vben-header");
}
