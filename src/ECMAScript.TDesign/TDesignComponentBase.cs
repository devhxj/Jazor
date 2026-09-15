using Microsoft.AspNetCore.Components;

namespace ECMAScript.TDesign;

public abstract class TComponentBase : ComponentBase, ECMAScript.Vue.IVueComponent
{
    /// <summary>
    /// CSS class or class map applied to the component root element.
    /// </summary>
    [Parameter]
    [ECMAScriptName("class")]
    public VueClassValue? CssClass { get; set; }

    /// <summary>
    /// Inline styles applied to the component root element.
    /// </summary>
    [Parameter]
    [ECMAScriptName("style")]
    public VueStyleValue? CssStyle { get; set; }

    /// <summary>
    /// Additional attributes forwarded to the component root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}

public abstract class TContentComponentBase : TComponentBase
{
    /// <summary>
    /// Content rendered in the component's default slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }
}
