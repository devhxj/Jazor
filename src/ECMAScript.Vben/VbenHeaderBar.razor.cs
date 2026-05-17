namespace ECMAScript.Vben;

[ECMAScriptModule("./components/vben-header-bar")]
public partial class VbenHeaderBar : VbenComponentBase, IVueContainerComponent
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

    private string? NormalizedTitle
        => VbenDisplayTextHelper.Normalize(Title);

    private string? NormalizedSubtitle
        => VbenDisplayTextHelper.Normalize(Subtitle);

    private bool HasTitles
        => NormalizedTitle is not null || NormalizedSubtitle is not null;

    private VueClassValue RootCssClass
        => BuildCssClass("vben-header");
}
