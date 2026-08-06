using Microsoft.AspNetCore.Components;

namespace ECMAScript.TDesign;

[VueLibraryComponent("tdesign-vue-next", "Divider")]
public sealed class TDivider : TContentComponentBase
{
    [Parameter]
    public TDividerAlign? Align { get; set; }

    [Parameter]
    public bool Dashed { get; set; }

    [Parameter]
    public TDividerLayout? Layout { get; set; }

    [Parameter]
    public TDimensionValue? Size { get; set; }

    [Parameter]
    [ECMAScriptName("content")]
    public string? Text { get; set; }
}
