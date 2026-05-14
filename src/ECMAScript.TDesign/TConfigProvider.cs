using Microsoft.AspNetCore.Components;

namespace ECMAScript.TDesign;

[VueLibraryComponent("tdesign-vue-next", "ConfigProvider")]
[VueLibraryStyle("tdesign-vue-next/es/style/index.css")]
[VueLibraryPluginRequirement("tdesign")]
[VueProp(nameof(CssClass), Name = "class")]
[VueProp(nameof(CssStyle), Name = "style")]
[VueSlot(nameof(ChildContent), IsDefault = true)]
public sealed class TConfigProvider : TDesignContentComponentBase
{
    [Parameter]
    public TDesignGlobalConfig? GlobalConfig { get; set; }
}
