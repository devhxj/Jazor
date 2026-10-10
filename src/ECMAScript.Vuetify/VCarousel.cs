using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 轮播组件创作代理，用于分组幻灯片导航。
/// Vuetify carousel authoring proxy for grouped slide navigation.
/// </summary>
[ECMAScript("vuetify/components/VCarousel")]
public sealed class VCarousel : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 当前轮播激活项的绑定值。
    /// The bound value of the currently active carousel item.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyGroupModelValue?；值域为 string | Number | bool | Symbol | VueProps | VuetifyGroupModelValue[]。数值用 ModelValue="@(32)"；变量用 ModelValue="@value"，无需 double 后缀。字符串用 ModelValue="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsNumber、value?.AsBool、value?.AsSymbol、value?.AsObject、value?.AsValues；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public VuetifyGroupModelValue? ModelValue { get; set; }

    /// <summary>
    /// 激活项变更时的回调。
    /// Callback invoked when the active item changes.
    /// </summary>
    /// <remarks data-authoring="types">C# 回调参数为 VuetifyGroupModelValue?；保持与模型相同的强类型。投影：value?.AsString、value?.AsNumber、value?.AsBool、value?.AsSymbol、value?.AsObject、value?.AsValues；不匹配返回 null，0/false 保留。</remarks>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<VuetifyGroupModelValue?> ModelValueChanged { get; set; }

    /// <summary>
    /// 组件的主题色。
    /// The theme color of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// 是否启用自动轮播循环。
    /// Whether to enable automatic carousel cycling.
    /// </summary>
    [Parameter]
    [ECMAScriptName("cycle")]
    public bool Cycle { get; set; }

    /// <summary>
    /// 分隔符图标。
    /// The delimiter icon.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyIconValue?；值域为 bool | string | Symbol | VueProps。变量用 DelimiterIcon="@value"，并保持声明的分支类型。字符串用 DelimiterIcon="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsString、value?.AsSymbol、value?.AsComponent；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("delimiterIcon")]
    public VuetifyIconValue? DelimiterIcon { get; set; }

    /// <summary>
    /// 轮播高度。
    /// The height of the carousel.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Height="@(32)"；变量用 Height="@value"，无需 double 后缀。字符串用 Height="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("height")]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 是否隐藏分隔符。
    /// Whether to hide the delimiters.
    /// </summary>
    [Parameter]
    [ECMAScriptName("hideDelimiters")]
    public bool HideDelimiters { get; set; }

    /// <summary>
    /// 是否隐藏分隔符背景。
    /// Whether to hide the delimiter background.
    /// </summary>
    [Parameter]
    [ECMAScriptName("hideDelimiterBackground")]
    public bool HideDelimiterBackground { get; set; }

    /// <summary>
    /// 自动轮播间隔时间。
    /// The interval for automatic carousel cycling.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Interval="@(32)"；变量用 Interval="@value"，无需 double 后缀。字符串用 Interval="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("interval")]
    public VueStringNumberValue? Interval { get; set; }

    /// <summary>
    /// 进度条显示方式。
    /// The progress indicator display mode.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyBooleanStringValue?；值域为 bool | string。变量用 Progress="@value"，并保持声明的分支类型。字符串用 Progress="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("progress")]
    public VuetifyBooleanStringValue? Progress { get; set; }

    /// <summary>
    /// 垂直分隔符的位置。
    /// The position of vertical delimiters.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyCarouselVerticalDelimiters?；值域为 bool | VuetifyCarouselVerticalDelimiterPosition。变量用 VerticalDelimiters="@value"，并保持声明的分支类型。投影：value?.AsBool、value?.AsPosition；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("verticalDelimiters")]
    public VuetifyCarouselVerticalDelimiters? VerticalDelimiters { get; set; }

    /// <summary>
    /// 是否在末尾循环回开头。
    /// Whether to loop back to the start after the last item.
    /// </summary>
    [Parameter]
    [ECMAScriptName("continuous")]
    public bool Continuous { get; set; } = true;

    /// <summary>
    /// 下一项图标。
    /// The icon for the next control.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyIconValue?；值域为 bool | string | Symbol | VueProps。变量用 NextIcon="@value"，并保持声明的分支类型。字符串用 NextIcon="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsString、value?.AsSymbol、value?.AsComponent；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("nextIcon")]
    public VuetifyIconValue? NextIcon { get; set; }

    /// <summary>
    /// 上一项图标。
    /// The icon for the previous control.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyIconValue?；值域为 bool | string | Symbol | VueProps。变量用 PrevIcon="@value"，并保持声明的分支类型。字符串用 PrevIcon="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsString、value?.AsSymbol、value?.AsComponent；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("prevIcon")]
    public VuetifyIconValue? PrevIcon { get; set; }

    /// <summary>
    /// 是否反转轮播方向。
    /// Whether to reverse the carousel direction.
    /// </summary>
    [Parameter]
    [ECMAScriptName("reverse")]
    public bool Reverse { get; set; }

    /// <summary>
    /// 是否显示导航箭头。
    /// Whether to show navigation arrows.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyWindowShowArrowsValue?；值域为 bool | VuetifyWindowShowArrowsMode。变量用 ShowArrows="@value"，并保持声明的分支类型。投影：value?.AsBool、value?.AsMode；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("showArrows")]
    public VuetifyWindowShowArrowsValue? ShowArrows { get; set; } = true;

    /// <summary>
    /// 触摸交互配置。
    /// The touch interaction configuration.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyTouchValue?；值域为 bool | VuetifyTouchHandlers。变量用 Touch="@value"，并保持声明的分支类型。投影：value?.AsBool、value?.AsHandlers；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("touch")]
    public VuetifyTouchValue? Touch { get; set; }

    /// <summary>
    /// 轮播切换方向。
    /// The transition direction of the carousel.
    /// </summary>
    [Parameter]
    [ECMAScriptName("direction")]
    public VuetifyInputDirection? Direction { get; set; }

    /// <summary>
    /// 是否禁用轮播交互。
    /// Whether to disable carousel interaction.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 选中项应用的 CSS 类名。
    /// The CSS class applied to the selected item.
    /// </summary>
    [Parameter]
    [ECMAScriptName("selectedClass")]
    public string? SelectedClass { get; set; }

    /// <summary>
    /// 是否强制选择。
    /// Whether selection is mandatory.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyMandatoryValue?；值域为 bool | VuetifyMandatoryMode。变量用 Mandatory="@value"，并保持声明的分支类型。投影：value?.AsBool、value?.AsMode；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("mandatory")]
    public VuetifyMandatoryValue? Mandatory { get; set; } = VuetifyMandatoryMode.Force;

    /// <summary>
    /// 渲染的 HTML 标签名。
    /// The HTML tag name to render.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tag")]
    public string? Tag { get; set; }

    /// <summary>
    /// 组件主题名称。
    /// The component theme name.
    /// </summary>
    [Parameter]
    [ECMAScriptName("theme")]
    public string? Theme { get; set; }

    /// <summary>
    /// 附加的自定义属性。
    /// Additional custom attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽，轮播项内容。
    /// Default slot for carousel item content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment<VWindowSlotContext>? ChildContent { get; set; }

    /// <summary>
    /// 上一项控制按钮插槽。
    /// Slot for the previous control button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("prev")]
    public RenderFragment<VWindowControlSlotContext>? Prev { get; set; }

    /// <summary>
    /// 下一项控制按钮插槽。
    /// Slot for the next control button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("next")]
    public RenderFragment<VWindowControlSlotContext>? Next { get; set; }

    /// <summary>
    /// 轮播项插槽。
    /// Slot for each carousel item.
    /// </summary>
    [Parameter]
    [ECMAScriptName("item")]
    public RenderFragment<VCarouselItemSlotContext>? Item { get; set; }
}
