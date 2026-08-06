using System.Collections.Generic;
using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;
using static ECMAScript.Vue3;

namespace ECMAScript;

/// <summary>
/// Razor authoring proxy for Vue Router's <c>RouterLink</c> component.
/// </summary>
[VueLibraryComponent("vue-router", "RouterLink")]
public sealed class VueRouterLink : ComponentBase
{
    [Parameter]
    [ECMAScriptName("class")]
    public VueClassValue? CssClass { get; set; }

    [Parameter]
    [ECMAScriptName("style")]
    public VueStyleValue? CssStyle { get; set; }

    [Parameter]
    [EditorRequired]
    public RouteLocationRaw To { get; set; }

    [Parameter]
    public bool Replace { get; set; }

    [Parameter]
    public string? ActiveClass { get; set; }

    [Parameter]
    public string? ExactActiveClass { get; set; }

    [Parameter]
    public RouterLinkAriaCurrentValue? AriaCurrentValue { get; set; }

    [Parameter]
    public bool ViewTransition { get; set; }

    [Parameter]
    public EventCallback<MouseEvent> OnClick { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
