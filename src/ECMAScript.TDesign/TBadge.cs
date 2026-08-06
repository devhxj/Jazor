using Microsoft.AspNetCore.Components;

namespace ECMAScript.TDesign;

[VueLibraryComponent("tdesign-vue-next", "Badge", StyleUrls = [TDesignLibraryAssets.StyleUrl])]
public sealed class TBadge : TContentComponentBase
{
    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    [ECMAScriptName("count")]
    public TBadgeCountValue? CountValue { get; set; }

    [Parameter]
    public bool Dot { get; set; }

    [Parameter]
    public int? MaxCount { get; set; }

    [Parameter]
    public TBadgeOffset? Offset { get; set; }

    [Parameter]
    public TBadgeShape? Shape { get; set; }

    [Parameter]
    public bool ShowZero { get; set; }

    [Parameter]
    public TBadgeSize? Size { get; set; }

    [Parameter]
    [ECMAScriptName("content")]
    public string? Text { get; set; }

    [Parameter]
    public RenderFragment? Count { get; set; }
}
