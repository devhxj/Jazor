using Microsoft.AspNetCore.Components;

namespace ECMAScript.TDesign;

[VueLibraryComponent("tdesign-vue-next", "Space", StyleUrls = [TDesignLibraryAssets.StyleUrl])]
public sealed class TSpace : TContentComponentBase
{
    [Parameter]
    public TSpaceAlign? Align { get; set; }

    [Parameter]
    public bool BreakLine { get; set; }

    [Parameter]
    public TSpaceDirection? Direction { get; set; }

    [Parameter]
    [ECMAScriptName("separator")]
    public string? SeparatorText { get; set; }

    [Parameter]
    public TSpaceSize? Size { get; set; }
}
