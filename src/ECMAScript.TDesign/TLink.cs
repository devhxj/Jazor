using Microsoft.AspNetCore.Components;

namespace ECMAScript.TDesign;

[VueLibraryComponent("tdesign-vue-next", "Link", StyleUrls = [TDesignLibraryAssets.StyleUrl])]
public sealed class TLink : TContentComponentBase
{
    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public TLinkDownloadValue? Download { get; set; }

    [Parameter]
    public TLinkHover? Hover { get; set; }

    [Parameter]
    public string? Href { get; set; }

    [Parameter]
    public TSize? Size { get; set; }

    [Parameter]
    public TTarget? Target { get; set; }

    [Parameter]
    [ECMAScriptName("content")]
    public string? Text { get; set; }

    [Parameter]
    public TLinkTheme? Theme { get; set; }

    [Parameter]
    public bool Underline { get; set; }

    [Parameter]
    public EventCallback<MouseEvent> OnClick { get; set; }

    [Parameter]
    [ECMAScriptName("prefixIcon")]
    public RenderFragment? PrefixIcon { get; set; }

    [Parameter]
    [ECMAScriptName("suffixIcon")]
    public RenderFragment? SuffixIcon { get; set; }
}
