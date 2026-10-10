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
    /// <remarks data-authoring="types">C# union VueClassValue?；值域为 string | string[] | VueProps | VueValue[]。变量用 CssClass="@value"，并保持声明的分支类型。字符串用 CssClass="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsStrings、value?.AsProps、value?.AsValues；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("class")]
    public VueClassValue? CssClass { get; set; }

    /// <summary>
    /// 组件的内联样式。
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStyleValue?；值域为 string | VueProps | VueStyleValue[]。变量用 CssStyle="@value"，并保持声明的分支类型。字符串用 CssStyle="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsProps、value?.AsValues；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
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