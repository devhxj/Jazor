using Microsoft.AspNetCore.Components;

namespace ECMAScript.ElementPlus;

/// <summary>
/// Element Plus Razor 组件的通用属性基类，提供 CSS 类、样式和未匹配属性转发。
/// </summary>
public abstract class ElComponentBase : ComponentBase, ECMAScript.Vue.IVueComponent
{
    /// <summary>
    /// 附加到组件或单元格的 CSS 类名。
    /// </summary>
    [Parameter]
    [ECMAScriptName("class")]
    public VueClassValue? CssClass { get; set; }

    /// <summary>
    /// 组件根元素的内联 CSS 样式。
    /// </summary>
    [Parameter]
    [ECMAScriptName("style")]
    public VueStyleValue? CssStyle { get; set; }

    /// <summary>
    /// 传给组件的其他属性；键使用上游 Vue/HTML 属性名。
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}

/// <summary>
/// 支持默认内容插槽的 Element Plus 组件基类。
/// </summary>
public abstract class ElContentComponentBase : ElComponentBase
{
    /// <summary>
    /// 组件默认插槽的内容。
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }
}