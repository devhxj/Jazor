using Microsoft.AspNetCore.Components;

namespace ECMAScript.ElementPlus;

public abstract class ElementPlusComponentBase : ComponentBase
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

public abstract class ElementPlusContentComponentBase : ElementPlusComponentBase
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
