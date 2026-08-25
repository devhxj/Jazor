using Microsoft.AspNetCore.Components;

namespace ECMAScript.ElementPlus;

public abstract class ElComponentBase : ComponentBase, ECMAScript.Vue.IVueComponent
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

public abstract class ElContentComponentBase : ElComponentBase
{
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }
}
