using Microsoft.AspNetCore.Components;

namespace ECMAScript.TDesign;

[VueLibraryComponent("tdesign-vue-next", "Link")]
[VueLibraryStyle("tdesign-vue-next/es/style/index.css")]
[VueLibraryPluginRequirement("tdesign")]
public sealed class TLink : TDesignContentComponentBase
{
    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public TDesignLinkDownloadValue? Download { get; set; }

    [Parameter]
    public TDesignLinkHover? Hover { get; set; }

    [Parameter]
    public string? Href { get; set; }

    [Parameter]
    public TDesignSize? Size { get; set; }

    [Parameter]
    public TDesignTarget? Target { get; set; }

    [Parameter]
    [ECMAScriptName("content")]
    public string? Text { get; set; }

    [Parameter]
    public TDesignLinkTheme? Theme { get; set; }

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
