using Microsoft.AspNetCore.Components;

namespace ECMAScript.TDesign;

[VueLibraryComponent("tdesign-vue-next", "Card")]
[VueLibraryStyle("tdesign-vue-next/es/style/index.css")]
[VueLibraryPluginRequirement("tdesign")]
[VueProp(nameof(CssClass), Name = "class")]
[VueProp(nameof(CssStyle), Name = "style")]
[VueProp(nameof(BodyCssClass), Name = "bodyClassName")]
[VueProp(nameof(HeaderCssClass), Name = "headerClassName")]
[VueProp(nameof(FooterCssClass), Name = "footerClassName")]
[VueProp(nameof(BodyCssStyle), Name = "bodyStyle")]
[VueProp(nameof(HeaderCssStyle), Name = "headerStyle")]
[VueProp(nameof(FooterCssStyle), Name = "footerStyle")]
[VueSlot(nameof(Header), Name = "header")]
[VueSlot(nameof(Footer), Name = "footer")]
[VueSlot(nameof(Actions), Name = "actions")]
[VueSlot(nameof(Avatar), Name = "avatar")]
[VueSlot(nameof(ChildContent), IsDefault = true)]
public sealed class TCard : TDesignContentComponentBase
{
    [Parameter]
    public bool Bordered { get; set; }

    [Parameter]
    public string? BodyCssClass { get; set; }

    [Parameter]
    public TDesignStyles? BodyCssStyle { get; set; }

    [Parameter]
    public string? Description { get; set; }

    [Parameter]
    public string? FooterCssClass { get; set; }

    [Parameter]
    public TDesignStyles? FooterCssStyle { get; set; }

    [Parameter]
    public string? HeaderCssClass { get; set; }

    [Parameter]
    public TDesignStyles? HeaderCssStyle { get; set; }

    [Parameter]
    public bool HeaderBordered { get; set; }

    [Parameter]
    public bool HoverShadow { get; set; }

    [Parameter]
    public bool Loading { get; set; }

    [Parameter]
    public bool Shadow { get; set; }

    [Parameter]
    public TDesignCardSize? Size { get; set; }

    [Parameter]
    public string? Status { get; set; }

    [Parameter]
    public string? Subtitle { get; set; }

    [Parameter]
    public TDesignCardTheme? Theme { get; set; }

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
