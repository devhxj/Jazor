using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 实验室图标按钮创作代理。
/// Vuetify labs icon-btn authoring proxy.
/// </summary>
[ECMAScript("vuetify/components/VIconBtn")]
public sealed class VIconBtn : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 按钮的主题颜色。
    /// Theme color of the button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// 按钮的外观变体。
    /// Visual variant of the button.
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
    /// 根元素使用的 HTML 标签。
    /// HTML tag used for the root element.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tag")]
    public string? Tag { get; set; }

    /// <summary>
    /// 按钮的圆角样式。
    /// Border radius style of the button.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyRoundedValue?；值域为 bool | Number | string。数值用 Rounded="@(32)"；变量用 Rounded="@value"，无需 double 后缀。字符串用 Rounded="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("rounded")]
    public VuetifyRoundedValue? Rounded { get; set; }

    /// <summary>
    /// 是否移除圆角。
    /// Whether to remove border radius.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tile")]
    public bool Tile { get; set; }

    /// <summary>
    /// 按钮的海拔阴影等级。
    /// Elevation shadow level of the button.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Elevation="@(32)"；变量用 Elevation="@value"，无需 double 后缀。字符串用 Elevation="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("elevation")]
    public VueStringNumberValue? Elevation { get; set; }

    /// <summary>
    /// 按钮的边框样式。
    /// Border style of the button.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyBorderValue?；值域为 bool | Number | string。数值用 Border="@(32)"；变量用 Border="@value"，无需 double 后缀。字符串用 Border="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("border")]
    public VuetifyBorderValue? Border { get; set; }

    /// <summary>
    /// 按钮是否处于激活状态。
    /// Whether the button is in the active state.
    /// </summary>
    [Parameter]
    [ECMAScriptName("active")]
    public bool Active { get; set; }

    /// <summary>
    /// 按钮激活状态变化时的回调。
    /// Callback when the button active state changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:active")]
    public EventCallback<bool> ActiveChanged { get; set; }

    /// <summary>
    /// 激活状态时的主题颜色。
    /// Theme color when the button is active.
    /// </summary>
    [Parameter]
    [ECMAScriptName("activeColor")]
    public string? ActiveColor { get; set; }

    /// <summary>
    /// 激活状态时显示的图标。
    /// Icon displayed when the button is active.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyIconValue?；值域为 bool | string | Symbol | VueProps。变量用 ActiveIcon="@value"，并保持声明的分支类型。字符串用 ActiveIcon="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsString、value?.AsSymbol、value?.AsComponent；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("activeIcon")]
    public VuetifyIconValue? ActiveIcon { get; set; }

    /// <summary>
    /// 激活状态时的外观变体。
    /// Visual variant when the button is active.
    /// </summary>
    [Parameter]
    [ECMAScriptName("activeVariant")]
    public VuetifyVariant? ActiveVariant { get; set; }

    /// <summary>
    /// 非激活状态时的外观变体。
    /// Visual variant when the button is inactive.
    /// </summary>
    [Parameter]
    [ECMAScriptName("baseVariant")]
    public VuetifyVariant? BaseVariant { get; set; }

    /// <summary>
    /// 是否禁用按钮。
    /// Whether the button is disabled.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 按钮的高度。
    /// Height of the button.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Height="@(32)"；变量用 Height="@value"，无需 double 后缀。字符串用 Height="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("height")]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 按钮的宽度。
    /// Width of the button.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Width="@(32)"；变量用 Width="@value"，无需 double 后缀。字符串用 Width="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("width")]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 是否隐藏叠加层。
    /// Whether to hide the overlay.
    /// </summary>
    [Parameter]
    [ECMAScriptName("hideOverlay")]
    public bool HideOverlay { get; set; }

    /// <summary>
    /// 按钮显示的图标。
    /// Icon displayed on the button.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyIconValue?；值域为 bool | string | Symbol | VueProps。变量用 Icon="@value"，并保持声明的分支类型。字符串用 Icon="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsString、value?.AsSymbol、value?.AsComponent；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("icon")]
    public VuetifyIconValue? Icon { get; set; }

    /// <summary>
    /// 图标的主题颜色。
    /// Theme color of the icon.
    /// </summary>
    [Parameter]
    [ECMAScriptName("iconColor")]
    public string? IconColor { get; set; }

    /// <summary>
    /// 图标的尺寸。
    /// Size of the icon.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 IconSize="@(32)"；变量用 IconSize="@value"，无需 double 后缀。字符串用 IconSize="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("iconSize")]
    public VueStringNumberValue? IconSize { get; set; }

    /// <summary>
    /// 不同尺寸下图标尺寸的映射。
    /// Icon size mapping for different sizes.
    /// </summary>
    /// <remarks data-authoring="types">C# union VIconBtnSizeMap?；值域为 VIconBtnSizeEntry[]。变量用 IconSizes="@value"，并保持声明的分支类型。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsArray；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("iconSizes")]
    public VIconBtnSizeMap? IconSizes { get; set; }

    /// <summary>
    /// 是否显示加载状态。
    /// Whether to show the loading state.
    /// </summary>
    [Parameter]
    [ECMAScriptName("loading")]
    public bool Loading { get; set; }

    /// <summary>
    /// 按钮的透明度。
    /// Opacity of the button.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Opacity="@(32)"；变量用 Opacity="@value"，无需 double 后缀。字符串用 Opacity="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("opacity")]
    public VueStringNumberValue? Opacity { get; set; }

    /// <summary>
    /// 是否将按钮设为只读。
    /// Whether the button is read-only.
    /// </summary>
    [Parameter]
    [ECMAScriptName("readonly")]
    public bool Readonly { get; set; }

    /// <summary>
    /// 图标的旋转角度。
    /// Rotation angle of the icon.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Rotate="@(32)"；变量用 Rotate="@value"，无需 double 后缀。字符串用 Rotate="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("rotate")]
    public VueStringNumberValue? Rotate { get; set; }

    /// <summary>
    /// 按钮的尺寸。
    /// Size of the button.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Size="@(32)"；变量用 Size="@value"，无需 double 后缀。字符串用 Size="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("size")]
    public VueStringNumberValue? Size { get; set; }

    /// <summary>
    /// 不同尺寸下按钮尺寸的映射。
    /// Button size mapping for different sizes.
    /// </summary>
    /// <remarks data-authoring="types">C# union VIconBtnSizeMap?；值域为 VIconBtnSizeEntry[]。变量用 Sizes="@value"，并保持声明的分支类型。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsArray；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("sizes")]
    public VIconBtnSizeMap? Sizes { get; set; }

    /// <summary>
    /// 按钮的文本内容。
    /// Text content of the button.
    /// </summary>
    /// <remarks data-authoring="types">C# union VIconBtnTextValue?；值域为 bool | Number | string。数值用 Text="@(32)"；变量用 Text="@value"，无需 double 后缀。字符串用 Text="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("text")]
    public VIconBtnTextValue? Text { get; set; }

    /// <summary>
    /// 捕获未匹配的额外 HTML 属性。
    /// Captures unmatched additional HTML attributes.
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

    /// <summary>
    /// 加载状态插槽内容。
    /// Slot content for the loading state.
    /// </summary>
    [Parameter]
    [ECMAScriptName("loader")]
    public RenderFragment? Loader { get; set; }
}
