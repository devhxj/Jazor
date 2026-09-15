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
    /// <summary>
    /// 要渲染的命名视图名称。
    /// The name of the named view to render.
    /// </summary>
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// 用于解析组件的自定义路由位置（覆盖当前路由）。
    /// Custom route location for resolving the component (overrides the current route).
    /// </summary>
    [Parameter]
    public RouteLocationNormalized? Route { get; set; }

    /// <summary>
    /// 传给 RouterLink/RouterView 的其他宿主属性；键使用 Vue/HTML 实际属性名。
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 当前匹配路由的显示内容插槽；用于自定义路由组件的渲染方式。
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment<RouterViewSlotScope>? ChildContent { get; set; }
}