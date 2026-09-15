using Microsoft.AspNetCore.Components;

namespace ECMAScript.TDesign;

/// <summary>
/// TDesign Razor 组件基类，提供通用样式和透传属性。
/// </summary>
public abstract class TComponentBase : ComponentBase, ECMAScript.Vue.IVueComponent
{
    /// <summary>
    /// 组件的 CSS 类名，支持字符串、类名数组和按条件启用的类名映射。
    /// </summary>
    [Parameter]
    [ECMAScriptName("class")]
    public VueClassValue? CssClass { get; set; }

    /// <summary>
    /// 组件的内联样式。
    /// </summary>
    [Parameter]
    [ECMAScriptName("style")]
    public VueStyleValue? CssStyle { get; set; }

    /// <summary>
    /// 未匹配组件参数的附加属性，按 Vue 属性透传规则传递给组件。
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}

/// <summary>
/// 支持默认插槽的 TDesign Razor 组件基类。
/// </summary>
public abstract class TContentComponentBase : TComponentBase
{
    /// <summary>
    /// 渲染到组件默认插槽的内容。
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }
}