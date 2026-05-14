using Microsoft.AspNetCore.Components;

namespace ECMAScript.TDesign;

[VueLibraryComponent("tdesign-vue-next", "Divider")]
[VueLibraryStyle("tdesign-vue-next/es/style/index.css")]
[VueLibraryPluginRequirement("tdesign")]
[VueLibraryProp(nameof(CssClass), Name = "class")]
[VueLibraryProp(nameof(CssStyle), Name = "style")]
[VueLibraryProp(nameof(Text), Name = "content")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class TDivider : TDesignContentComponentBase
{
    [Parameter]
    public TDesignDividerAlign? Align { get; set; }

    [Parameter]
    public bool Dashed { get; set; }

    [Parameter]
    public TDesignDividerLayout? Layout { get; set; }

    [Parameter]
    public TDesignDimensionValue? Size { get; set; }

    [Parameter]
    public string? Text { get; set; }
}
