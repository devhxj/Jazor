using System.Collections.Generic;
using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;
using static ECMAScript.Vue;

namespace ECMAScript;

/// <summary>
/// Razor authoring proxy for Vue Router's <c>RouterView</c> component.
/// It preserves the scoped default slot so callers can render the matched
/// component with the route context supplied by Vue Router.
/// </summary>
[ECMAScript("vue-router", Transform.Component, "RouterView")]
public sealed class VueRouterView : ComponentBase, IVueComponent
{
    [Parameter]
    public string? Name { get; set; }

    [Parameter]
    public RouteLocationNormalized? Route { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment<RouterViewSlotScope>? ChildContent { get; set; }
}
