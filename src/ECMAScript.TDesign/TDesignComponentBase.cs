using Microsoft.AspNetCore.Components;

namespace ECMAScript.TDesign;

public abstract class TDesignComponentBase : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    [ECMAScriptName("class")]
    public VueClassValue? CssClass { get; set; }

    [Parameter]
    [ECMAScriptName("style")]
    public VueStyleValue? CssStyle { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}

public abstract class TDesignContentComponentBase : TDesignComponentBase
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
