using Microsoft.AspNetCore.Components;

namespace ECMAScript.TDesign;

[VueLibraryComponent("tdesign-vue-next", "ConfigProvider")]
[VueLibraryStyle("tdesign-vue-next/es/style/index.css")]
[VueLibraryPluginRequirement("tdesign")]
[VueLibraryProp(nameof(CssClass), Name = "class")]
[VueLibraryProp(nameof(CssStyle), Name = "style")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class TConfigProvider : TDesignContentComponentBase
{
    [Parameter]
    public TDesignGlobalConfig? GlobalConfig { get; set; }
}
