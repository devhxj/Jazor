namespace ECMAScript.Vben;

[ECMAScriptModule("./components/vben-page-container")]
public partial class VbenPageContainer : VbenContentComponentBase
{
    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public string? Subtitle { get; set; }

    [Parameter]
    public VbenBreadcrumbItem[]? BreadcrumbItems { get; set; }

    [Parameter]
    public VbenPageAction[]? Actions { get; set; }

    [Parameter]
    public RenderFragment? Extra { get; set; }

    private VueClassValue RootCssClass
        => BuildCssClass("vben-page");
}
