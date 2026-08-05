using Microsoft.AspNetCore.Components;

namespace ECMAScript.TDesign;

[VueLibraryComponent("tdesign-vue-next", "Divider")]
[VueLibraryStyle("tdesign-vue-next/es/style/index.css")]
[VueLibraryPluginRequirement("tdesign")]
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
    [ECMAScriptName("content")]
    public string? Text { get; set; }
}
