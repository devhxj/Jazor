using Microsoft.AspNetCore.Components;

namespace ECMAScript.TDesign;

[VueLibraryComponent("tdesign-vue-next", "Badge")]
[VueLibraryStyle("tdesign-vue-next/es/style/index.css")]
[VueLibraryPluginRequirement("tdesign")]
[VueLibraryProp(nameof(CssClass), Name = "class")]
[VueLibraryProp(nameof(CssStyle), Name = "style")]
[VueLibraryProp(nameof(CountValue), Name = "count")]
[VueLibraryProp(nameof(Text), Name = "content")]
[VueLibrarySlot(nameof(Count), Name = "count")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class TBadge : TDesignContentComponentBase
{
    [Parameter]
    public string? Color { get; set; }

    [Parameter]
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
    public string? Text { get; set; }

    [Parameter]
    public RenderFragment? Count { get; set; }
}
