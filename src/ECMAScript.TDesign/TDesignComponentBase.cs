using Microsoft.AspNetCore.Components;

namespace ECMAScript.TDesign;

public abstract class TComponentBase : ComponentBase, ECMAScript.Vue.IVueComponent
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

public abstract class TContentComponentBase : TComponentBase
{
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }
}
