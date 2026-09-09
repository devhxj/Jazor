namespace Jazor.Admin;

[ECMAScriptModule("./components/admin/header")]
public partial class JHeader : JComponentBase, IVueContainerComponent
{
    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public string? Subtitle { get; set; }

    [Parameter]
    public RenderFragment? Logo { get; set; }

    [Parameter]
    public RenderFragment? Navigation { get; set; }

    [Parameter]
    public RenderFragment? Actions { get; set; }

    [Parameter]
    public RenderFragment? UserRegion { get; set; }

    private string? NormalizedTitle
        => AdminDisplayTextHelper.Normalize(Title);

    private string? NormalizedSubtitle
        => AdminDisplayTextHelper.Normalize(Subtitle);

    private bool HasTitles
        => NormalizedTitle is not null || NormalizedSubtitle is not null;

    private bool HasMainRegion => Logo is not null || HasTitles;

    private bool HasRightRegion => Actions is not null || UserRegion is not null;

    private bool HasContent => HasMainRegion || Navigation is not null || HasRightRegion;

    private VueClassValue RootCssClass
        => BuildCssClass("ja-header");

}
