using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vben.TDesign;

[ECMAScriptModule("./components/vben-tdesign-header-bar")]
public partial class VbenTDesignHeaderBar : VbenComponentBase
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
    {
        get
        {
            if (CssClass is null)
                return "vben-tdesign-header";

            return new VueValue[]
            {
                "vben-tdesign-header",
                CssClass
            };
        }
    }
}
