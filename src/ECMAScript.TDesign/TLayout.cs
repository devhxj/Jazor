using Microsoft.AspNetCore.Components;

namespace ECMAScript.TDesign;

[VueLibraryComponent("tdesign-vue-next", "Layout")]
[VueLibraryStyle("tdesign-vue-next/es/style/index.css")]
[VueLibraryPluginRequirement("tdesign")]
[VueLibraryProp(nameof(CssClass), Name = "class")]
[VueLibraryProp(nameof(CssStyle), Name = "style")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class TLayout : TDesignContentComponentBase
{
    [Parameter]
    public TDesignLayoutDirection? Direction { get; set; }
}

[VueLibraryComponent("tdesign-vue-next", "Header")]
[VueLibraryStyle("tdesign-vue-next/es/style/index.css")]
[VueLibraryPluginRequirement("tdesign")]
[VueLibraryProp(nameof(CssClass), Name = "class")]
[VueLibraryProp(nameof(CssStyle), Name = "style")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class THeader : TDesignContentComponentBase
{
    [Parameter]
    public string? Height { get; set; }
}

[VueLibraryComponent("tdesign-vue-next", "Aside")]
[VueLibraryStyle("tdesign-vue-next/es/style/index.css")]
[VueLibraryPluginRequirement("tdesign")]
[VueLibraryProp(nameof(CssClass), Name = "class")]
[VueLibraryProp(nameof(CssStyle), Name = "style")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class TAside : TDesignContentComponentBase
{
    [Parameter]
    public string? Width { get; set; }
}

[VueLibraryComponent("tdesign-vue-next", "Content")]
[VueLibraryStyle("tdesign-vue-next/es/style/index.css")]
[VueLibraryPluginRequirement("tdesign")]
[VueLibraryProp(nameof(CssClass), Name = "class")]
[VueLibraryProp(nameof(CssStyle), Name = "style")]
[VueLibraryProp(nameof(Text), Name = "content")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class TContent : TDesignContentComponentBase
{
    [Parameter]
    public string? Text { get; set; }
}

[VueLibraryComponent("tdesign-vue-next", "Footer")]
[VueLibraryStyle("tdesign-vue-next/es/style/index.css")]
[VueLibraryPluginRequirement("tdesign")]
[VueLibraryProp(nameof(CssClass), Name = "class")]
[VueLibraryProp(nameof(CssStyle), Name = "style")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class TFooter : TDesignContentComponentBase
{
    [Parameter]
    public string? Height { get; set; }
}
