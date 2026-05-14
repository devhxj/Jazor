using Microsoft.AspNetCore.Components;

namespace ECMAScript.TDesign;

[VueLibraryComponent("tdesign-vue-next", "Divider")]
[VueLibraryStyle("tdesign-vue-next/es/style/index.css")]
[VueLibraryPluginRequirement("tdesign")]
[VueProp(nameof(CssClass), Name = "class")]
[VueProp(nameof(CssStyle), Name = "style")]
[VueProp(nameof(Text), Name = "content")]
[VueSlot(nameof(ChildContent), IsDefault = true)]
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
