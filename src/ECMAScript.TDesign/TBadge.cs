using Microsoft.AspNetCore.Components;

namespace ECMAScript.TDesign;

[VueLibraryComponent("tdesign-vue-next", "Badge")]
[VueLibraryStyle("tdesign-vue-next/es/style/index.css")]
[VueLibraryPluginRequirement("tdesign")]
public sealed class TBadge : TDesignContentComponentBase
{
    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    [ECMAScriptName("count")]
    public TDesignBadgeCountValue? CountValue { get; set; }

    [Parameter]
    public bool Dot { get; set; }

    [Parameter]
    public int? MaxCount { get; set; }

    [Parameter]
    public TDesignBadgeOffset? Offset { get; set; }

    [Parameter]
    public TDesignBadgeShape? Shape { get; set; }

    [Parameter]
    public bool ShowZero { get; set; }

    [Parameter]
    public TDesignBadgeSize? Size { get; set; }

    [Parameter]
    [ECMAScriptName("content")]
    public string? Text { get; set; }

    [Parameter]
    public RenderFragment? Count { get; set; }
}
