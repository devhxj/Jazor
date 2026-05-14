using Microsoft.AspNetCore.Components;

namespace ECMAScript.TDesign;

[VueLibraryComponent("tdesign-vue-next", "Space")]
[VueLibraryStyle("tdesign-vue-next/es/style/index.css")]
[VueLibraryPluginRequirement("tdesign")]
[VueLibraryProp(nameof(CssClass), Name = "class")]
[VueLibraryProp(nameof(CssStyle), Name = "style")]
[VueLibraryProp(nameof(SeparatorText), Name = "separator")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
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
