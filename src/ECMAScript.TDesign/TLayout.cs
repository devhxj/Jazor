using Microsoft.AspNetCore.Components;

namespace ECMAScript.TDesign;

[VueLibraryComponent("tdesign-vue-next", "Layout", StyleUrls = [TDesignLibraryAssets.StyleUrl])]
public sealed class TLayout : TContentComponentBase
{
    [Parameter]
    public TLayoutDirection? Direction { get; set; }
}

[VueLibraryComponent("tdesign-vue-next", "Header", StyleUrls = [TDesignLibraryAssets.StyleUrl])]
public sealed class THeader : TContentComponentBase
{
    [Parameter]
    public string? Height { get; set; }
}

[VueLibraryComponent("tdesign-vue-next", "Aside", StyleUrls = [TDesignLibraryAssets.StyleUrl])]
public sealed class TAside : TContentComponentBase
{
    [Parameter]
    public string? Width { get; set; }
}

[VueLibraryComponent("tdesign-vue-next", "Content", StyleUrls = [TDesignLibraryAssets.StyleUrl])]
public sealed class TContent : TContentComponentBase
{
    [Parameter]
    [ECMAScriptName("content")]
    public string? Text { get; set; }
}

[VueLibraryComponent("tdesign-vue-next", "Footer", StyleUrls = [TDesignLibraryAssets.StyleUrl])]
public sealed class TFooter : TContentComponentBase
{
    [Parameter]
    public string? Height { get; set; }
}
