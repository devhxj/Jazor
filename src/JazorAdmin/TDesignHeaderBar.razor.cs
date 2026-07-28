using Microsoft.AspNetCore.Components;

namespace JazorAdmin;

[ECMAScriptModule("./components/jazor-admin-tdesign-header-bar")]
public partial class TDesignHeaderBar : AdminComponentBase
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
    public RenderFragment? Navigation { get; set; }

    [Parameter]
    public RenderFragment? UserRegion { get; set; }

    private const string RootCssClass = "jazor-admin-tdesign-header";
}
