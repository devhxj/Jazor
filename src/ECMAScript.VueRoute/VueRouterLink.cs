using System.Collections.Generic;
using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;
using static ECMAScript.Vue;

namespace ECMAScript;

/// <summary>
/// Razor authoring proxy for Vue Router's <c>RouterLink</c> component.
/// </summary>
[ECMAScript("vue-router", Transform.Component, "RouterLink")]
public sealed class VueRouterLink : ComponentBase, IVueComponent
{
    /// <summary>
    /// 链接元素的 CSS 类名，可使用字符串、条件映射或它们的数组。
    /// </summary>
    [Parameter]
    [ECMAScriptName("class")]
    public VueClassValue? CssClass { get; set; }

    /// <summary>
    /// 链接元素的内联 CSS 样式，可使用字符串或强类型样式对象。
    /// </summary>
    [Parameter]
    [ECMAScriptName("style")]
    public VueStyleValue? CssStyle { get; set; }

    /// <summary>
    /// 链接的目标路由位置。
    /// The target route location for the link.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public RouteLocationRaw To { get; set; }

    /// <summary>
    /// 是否在导航时替换当前历史条目而不是推入新条目。
    /// Whether to replace the current history entry instead of pushing a new one during navigation.
    /// </summary>
    [Parameter]
    public bool Replace { get; set; }

    /// <summary>
    /// 是否禁用默认链接渲染，完全由作用域插槽控制输出。
    /// Whether to disable default link rendering and fully control output via the scoped slot.
    /// </summary>
    [Parameter]
    public bool? Custom { get; set; }

    /// <summary>
    /// 活跃链接的 CSS 类名。
    /// The CSS class to apply to active links.
    /// </summary>
    [Parameter]
    public string? ActiveClass { get; set; }

    /// <summary>
    /// 精确活跃链接的 CSS 类名。
    /// The CSS class to apply to exactly active links.
    /// </summary>
    [Parameter]
    public string? ExactActiveClass { get; set; }

    /// <summary>
    /// aria-current 属性的值。
    /// The value for the aria-current attribute.
    /// </summary>
    [Parameter]
    public RouterLinkAriaCurrentValue? AriaCurrentValue { get; set; }

    /// <summary>
    /// 是否在导航期间启用 View Transition API。
    /// Whether to enable the View Transition API during navigation.
    /// </summary>
    [Parameter]
    public bool ViewTransition { get; set; }

    /// <summary>
    /// 链接的鼠标点击事件回调；事件参数为原生 MouseEvent。
    /// </summary>
    [Parameter]
    public EventCallback<MouseEvent> OnClick { get; set; }

    /// <summary>
    /// 传给 RouterLink/RouterView 的其他宿主属性；键使用 Vue/HTML 实际属性名。
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 链接显示内容；custom 为 true 时可利用 RouterLink 插槽自行渲染链接。
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment<RouterLinkSlotScope>? ChildContent { get; set; }
}