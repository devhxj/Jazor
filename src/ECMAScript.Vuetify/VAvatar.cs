using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 头像组件。
/// Vuetify avatar component.
/// </summary>
[ECMAScript("vuetify/components/VAvatar")]
public sealed class VAvatar : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 组件的主题颜色。
    /// Theme color of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// 组件的视觉变体样式。
    /// Visual variant style.
    /// </summary>
    [Parameter]
    [ECMAScriptName("variant")]
    public VuetifyVariant? Variant { get; set; }

    /// <summary>
    /// 组件使用的主题名称。
    /// Theme name used by the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("theme")]
    public string? Theme { get; set; }

    /// <summary>
    /// 渲染的根 HTML 元素标签名。
    /// Root HTML element tag name.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tag")]
    public string? Tag { get; set; }

    /// <summary>
    /// 组件尺寸。
    /// Component size.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Size="@(32)"；变量用 Size="@value"，无需 double 后缀。字符串用 Size="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("size")]
    public VueStringNumberValue? Size { get; set; }

    /// <summary>
    /// 组件的圆角大小。
    /// Border radius size.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyRoundedValue?；值域为 bool | Number | string。数值用 Rounded="@(32)"；变量用 Rounded="@value"，无需 double 后缀。字符串用 Rounded="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("rounded")]
    public VuetifyRoundedValue? Rounded { get; set; }

    /// <summary>
    /// 是否移除圆角。
    /// Removes border radius.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tile")]
    public bool Tile { get; set; }

    /// <summary>
    /// 组件的紧凑程度。
    /// Component density level.
    /// </summary>
    [Parameter]
    [ECMAScriptName("density")]
    public VuetifyDensity? Density { get; set; }

    /// <summary>
    /// 自定义 CSS 类。
    /// Custom CSS class(es).
    /// </summary>
    /// <remarks data-authoring="types">C# union VueClassValue?；值域为 string | string[] | VueProps | VueValue[]。变量用 CssClass="@value"，并保持声明的分支类型。字符串用 CssClass="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsStrings、value?.AsProps、value?.AsValues；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("class")]
    public VueClassValue? CssClass { get; set; }

    /// <summary>
    /// 自定义行内样式。
    /// Custom inline style(s).
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyStyleValue?；值域为 string | VueProps | VuetifyStyleValue[]。变量用 CssStyle="@value"，并保持声明的分支类型。字符串用 CssStyle="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsProps、value?.AsValues；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("style")]
    public VuetifyStyleValue? CssStyle { get; set; }

    /// <summary>
    /// 边框设置。
    /// Border configuration.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyBorderValue?；值域为 bool | Number | string。数值用 Border="@(32)"；变量用 Border="@value"，无需 double 后缀。字符串用 Border="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("border")]
    public VuetifyBorderValue? Border { get; set; }

    /// <summary>
    /// 是否靠前排列。
    /// Positions at the start.
    /// </summary>
    [Parameter]
    [ECMAScriptName("start")]
    public bool Start { get; set; }

    /// <summary>
    /// 是否靠后排列。
    /// Positions at the end.
    /// </summary>
    [Parameter]
    [ECMAScriptName("end")]
    public bool End { get; set; }

    /// <summary>
    /// 显示的图标。
    /// Icon to display.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyIconValue?；值域为 bool | string | Symbol | VueProps。变量用 Icon="@value"，并保持声明的分支类型。字符串用 Icon="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsString、value?.AsSymbol、value?.AsComponent；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("icon")]
    public VuetifyIconValue? Icon { get; set; }

    /// <summary>
    /// 头像图片 URL。
    /// Avatar image URL.
    /// </summary>
    [Parameter]
    [ECMAScriptName("image")]
    public string? Image { get; set; }

    /// <summary>
    /// 文本内容。
    /// Text content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// 传递给根元素的额外 HTML 属性。
    /// Additional HTML attributes passed to root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽内容。
    /// Default slot content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }
}