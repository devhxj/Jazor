using Microsoft.AspNetCore.Components;

namespace ECMAScript.TDesign;

[VueLibraryComponent("tdesign-vue-next", "Card")]
public sealed class TCard : TContentComponentBase
{
    [Parameter]
    public bool Bordered { get; set; }

    [Parameter]
    [ECMAScriptName("bodyClassName")]
    public string? BodyCssClass { get; set; }

    [Parameter]
    [ECMAScriptName("bodyStyle")]
    public TStyles? BodyCssStyle { get; set; }

    [Parameter]
    public string? Description { get; set; }

    [Parameter]
    [ECMAScriptName("footerClassName")]
    public string? FooterCssClass { get; set; }

    [Parameter]
    [ECMAScriptName("footerStyle")]
    public TStyles? FooterCssStyle { get; set; }

    [Parameter]
    [ECMAScriptName("headerClassName")]
    public string? HeaderCssClass { get; set; }

    [Parameter]
    [ECMAScriptName("headerStyle")]
    public TStyles? HeaderCssStyle { get; set; }

    [Parameter]
    public bool HeaderBordered { get; set; }

    [Parameter]
    public bool HoverShadow { get; set; }

    [Parameter]
    public bool Loading { get; set; }

    [Parameter]
    public bool Shadow { get; set; }

    [Parameter]
    public TCardSize? Size { get; set; }

    [Parameter]
    public string? Status { get; set; }

    [Parameter]
    public string? Subtitle { get; set; }

    [Parameter]
    public TCardTheme? Theme { get; set; }

    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public RenderFragment? Header { get; set; }

    [Parameter]
    public RenderFragment? Footer { get; set; }

    [Parameter]
    public RenderFragment? Actions { get; set; }

    [Parameter]
    public RenderFragment? Avatar { get; set; }
}
