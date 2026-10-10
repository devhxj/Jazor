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
    /// <remarks data-authoring="types">C# union VueClassValue?；值域为 string | string[] | VueProps | VueValue[]。变量用 CssClass="@value"，并保持声明的分支类型。字符串用 CssClass="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsStrings、value?.AsProps、value?.AsValues；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("class")]
    public VueClassValue? CssClass { get; set; }

    /// <summary>
    /// 组件根元素的内联 CSS 样式。
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStyleValue?；值域为 string | VueProps | VueStyleValue[]。变量用 CssStyle="@value"，并保持声明的分支类型。字符串用 CssStyle="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsProps、value?.AsValues；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
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