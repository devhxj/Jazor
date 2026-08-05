using Microsoft.AspNetCore.Components;

namespace ECMAScript.TDesign;

[VueLibraryComponent("tdesign-vue-next", "Space")]
public sealed class TSpace : TDesignContentComponentBase
{
    [Parameter]
    public TDesignSpaceAlign? Align { get; set; }

    [Parameter]
    public bool BreakLine { get; set; }

    [Parameter]
    public TDesignSpaceDirection? Direction { get; set; }

    [Parameter]
    [ECMAScriptName("separator")]
    public string? SeparatorText { get; set; }

    [Parameter]
    public TDesignSpaceSize? Size { get; set; }
}
