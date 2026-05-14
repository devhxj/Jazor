using Microsoft.AspNetCore.Components;

namespace ECMAScript.TDesign;

[VueLibraryComponent("tdesign-vue-next", "Space")]
[VueLibraryStyle("tdesign-vue-next/es/style/index.css")]
[VueLibraryPluginRequirement("tdesign")]
[VueProp(nameof(CssClass), Name = "class")]
[VueProp(nameof(CssStyle), Name = "style")]
[VueProp(nameof(SeparatorText), Name = "separator")]
[VueSlot(nameof(ChildContent), IsDefault = true)]
public sealed class TSpace : TDesignContentComponentBase
{
    [Parameter]
    public TDesignSpaceAlign? Align { get; set; }

    [Parameter]
    public bool BreakLine { get; set; }

    [Parameter]
    public TDesignSpaceDirection? Direction { get; set; }

    [Parameter]
    public string? SeparatorText { get; set; }

    [Parameter]
    public TDesignSpaceSize? Size { get; set; }
}
