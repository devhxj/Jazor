using Microsoft.AspNetCore.Components;

namespace ECMAScript.TDesign;

[VueLibraryComponent("tdesign-vue-next", "Layout")]
public sealed class TLayout : TDesignContentComponentBase
{
    [Parameter]
    public TDesignLayoutDirection? Direction { get; set; }
}

[VueLibraryComponent("tdesign-vue-next", "Header")]
public sealed class THeader : TDesignContentComponentBase
{
    [Parameter]
    public string? Height { get; set; }
}

[VueLibraryComponent("tdesign-vue-next", "Aside")]
public sealed class TAside : TDesignContentComponentBase
{
    [Parameter]
    public string? Width { get; set; }
}

[VueLibraryComponent("tdesign-vue-next", "Content")]
public sealed class TContent : TDesignContentComponentBase
{
    [Parameter]
    [ECMAScriptName("content")]
    public string? Text { get; set; }
}

[VueLibraryComponent("tdesign-vue-next", "Footer")]
public sealed class TFooter : TDesignContentComponentBase
{
    [Parameter]
    public string? Height { get; set; }
}
