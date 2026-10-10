using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 线性进度条组件。
/// Vuetify linear progress bar component.
/// </summary>
[ECMAScript("vuetify/components/VProgressLinear")]
public sealed class VProgressLinear : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 组件使用的主题名称。
    /// The theme name used by the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("theme")]
    public string? Theme { get; set; }

    /// <summary>
    /// 渲染根元素时使用的 HTML 标签。
    /// The HTML tag used for the root element.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tag")]
    public string? Tag { get; set; }

    /// <summary>
    /// 圆角大小。
    /// The border radius size.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyRoundedValue?；值域为 bool | Number | string。数值用 Rounded="@(32)"；变量用 Rounded="@value"，无需 double 后缀。字符串用 Rounded="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("rounded")]
    public VuetifyRoundedValue? Rounded { get; set; }

    /// <summary>
    /// 是否移除圆角，使边角为直角。
    /// Whether to remove border radius for sharp corners.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tile")]
    public bool Tile { get; set; }

    /// <summary>
    /// 进度条的方位。
    /// The location of the progress bar.
    /// </summary>
    [Parameter]
    [ECMAScriptName("location")]
    public VuetifyLocation? Location { get; set; }

    /// <summary>
    /// 应用于根元素的 CSS 类。
    /// CSS classes applied to the root element.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueClassValue?；值域为 string | string[] | VueProps | VueValue[]。变量用 CssClass="@value"，并保持声明的分支类型。字符串用 CssClass="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsStrings、value?.AsProps、value?.AsValues；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("class")]
    public VueClassValue? CssClass { get; set; }

    /// <summary>
    /// 应用于根元素的内联样式。
    /// Inline styles applied to the root element.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyStyleValue?；值域为 string | VueProps | VuetifyStyleValue[]。变量用 CssStyle="@value"，并保持声明的分支类型。字符串用 CssStyle="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsProps、value?.AsValues；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("style")]
    public VuetifyStyleValue? CssStyle { get; set; }

    /// <summary>
    /// 是否使用绝对定位。
    /// Whether to use absolute positioning.
    /// </summary>
    [Parameter]
    [ECMAScriptName("absolute")]
    public bool Absolute { get; set; }

    /// <summary>
    /// 是否激活显示进度条。
    /// Whether the progress bar is active and visible.
    /// </summary>
    [Parameter]
    [ECMAScriptName("active")]
    public bool Active { get; set; } = true;

    /// <summary>
    /// 进度条的颜色。
    /// The color of the progress bar.
    /// </summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// 进度条的背景颜色。
    /// The background color of the progress bar.
    /// </summary>
    [Parameter]
    [ECMAScriptName("bgColor")]
    public string? BgColor { get; set; }

    /// <summary>
    /// 背景透明度。
    /// The background opacity.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 BgOpacity="@(32)"；变量用 BgOpacity="@value"，无需 double 后缀。字符串用 BgOpacity="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("bgOpacity")]
    public VueStringNumberValue? BgOpacity { get; set; }

    /// <summary>
    /// 缓冲区的进度值。
    /// The buffer progress value.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 BufferValue="@(32)"；变量用 BufferValue="@value"，无需 double 后缀。字符串用 BufferValue="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("bufferValue")]
    public VueStringNumberValue? BufferValue { get; set; }

    /// <summary>
    /// 缓冲区的颜色。
    /// The color of the buffer track.
    /// </summary>
    [Parameter]
    [ECMAScriptName("bufferColor")]
    public string? BufferColor { get; set; }

    /// <summary>
    /// 缓冲区的透明度。
    /// The opacity of the buffer track.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 BufferOpacity="@(32)"；变量用 BufferOpacity="@value"，无需 double 后缀。字符串用 BufferOpacity="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("bufferOpacity")]
    public VueStringNumberValue? BufferOpacity { get; set; }

    /// <summary>
    /// 是否允许点击进度条。
    /// Whether the progress bar is clickable.
    /// </summary>
    [Parameter]
    [ECMAScriptName("clickable")]
    public bool Clickable { get; set; }

    /// <summary>
    /// 进度条的高度。
    /// The height of the progress bar.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Height="@(32)"；变量用 Height="@value"，无需 double 后缀。字符串用 Height="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("height")]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 是否显示为不确定状态的动画。
    /// Whether to display an indeterminate animation.
    /// </summary>
    [Parameter]
    [ECMAScriptName("indeterminate")]
    public bool Indeterminate { get; set; }

    /// <summary>
    /// 进度的最大值。
    /// The maximum value of the progress.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Max="@(32)"；变量用 Max="@value"，无需 double 后缀。字符串用 Max="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("max")]
    public VueStringNumberValue? Max { get; set; }

    /// <summary>
    /// 当前进度值。
    /// The current progress value.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 ModelValue="@(32)"；变量用 ModelValue="@value"，无需 double 后缀。字符串用 ModelValue="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public VueStringNumberValue? ModelValue { get; set; }

    /// <summary>
    /// 进度值变更时触发的回调。
    /// Callback invoked when the progress value changes.
    /// </summary>
    /// <remarks data-authoring="types">C# 回调参数为 Number；保持与模型相同的强类型。</remarks>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<Number> ModelValueChanged { get; set; }

    /// <summary>
    /// 进度条的透明度。
    /// The opacity of the progress bar.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Opacity="@(32)"；变量用 Opacity="@value"，无需 double 后缀。字符串用 Opacity="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("opacity")]
    public VueStringNumberValue? Opacity { get; set; }

    /// <summary>
    /// 是否反转进度条方向。
    /// Whether to reverse the progress bar direction.
    /// </summary>
    [Parameter]
    [ECMAScriptName("reverse")]
    public bool Reverse { get; set; }

    /// <summary>
    /// 是否显示流式动画效果。
    /// Whether to show a streaming animation effect.
    /// </summary>
    [Parameter]
    [ECMAScriptName("stream")]
    public bool Stream { get; set; }

    /// <summary>
    /// 是否显示条纹效果。
    /// Whether to show a striped effect.
    /// </summary>
    [Parameter]
    [ECMAScriptName("striped")]
    public bool Striped { get; set; }

    /// <summary>
    /// 进度条是否显示圆角。
    /// Whether the progress bar track has rounded corners.
    /// </summary>
    [Parameter]
    [ECMAScriptName("roundedBar")]
    public bool RoundedBar { get; set; }

    /// <summary>
    /// 附加到根元素上的额外属性。
    /// Additional attributes applied to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽内容。
    /// The default slot content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment<VProgressLinearDefaultSlotContext>? ChildContent { get; set; }
}
