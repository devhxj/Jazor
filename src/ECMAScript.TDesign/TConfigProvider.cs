using Microsoft.AspNetCore.Components;

namespace ECMAScript.TDesign;

[VueLibraryComponent("tdesign-vue-next", "ConfigProvider")]
public sealed class TConfigProvider : TContentComponentBase
{
    [Parameter]
    public TGlobalConfig? GlobalConfig { get; set; }
}
