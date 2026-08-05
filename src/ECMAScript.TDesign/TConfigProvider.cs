using Microsoft.AspNetCore.Components;

namespace ECMAScript.TDesign;

[VueLibraryComponent("tdesign-vue-next", "ConfigProvider")]
public sealed class TConfigProvider : TDesignContentComponentBase
{
    [Parameter]
    public TDesignGlobalConfig? GlobalConfig { get; set; }
}
