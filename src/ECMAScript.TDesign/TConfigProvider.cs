using Microsoft.AspNetCore.Components;

namespace ECMAScript.TDesign;

[VueLibraryComponent("tdesign-vue-next", "ConfigProvider", StyleUrls = [TDesignLibraryAssets.StyleUrl])]
public sealed class TConfigProvider : TContentComponentBase
{
    [Parameter]
    public TGlobalConfig? GlobalConfig { get; set; }
}
