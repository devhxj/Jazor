using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 滑动分组组件的编写代理，用于水平或垂直可滚动的分组内容。
/// Vuetify slide-group authoring proxy for horizontally or vertically scrollable grouped content.
/// </summary>
[ECMAScript("vuetify/components/VSlideGroup")]
public sealed class VSlideGroup : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 当前选中的值。
    /// Currently selected value.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyGroupModelValue?；值域为 string | Number | bool | Symbol | VueProps | VuetifyGroupModelValue[]。数值用 ModelValue="@(32)"；变量用 ModelValue="@value"，无需 double 后缀。字符串用 ModelValue="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsNumber、value?.AsBool、value?.AsSymbol、value?.AsObject、value?.AsValues；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public VuetifyGroupModelValue? ModelValue { get; set; }

    /// <summary>
    /// 选中值变更回调。
    /// Callback when the selected value changes.
    /// </summary>
    /// <remarks data-authoring="types">C# 回调参数为 VuetifyGroupModelValue?；保持与模型相同的强类型。投影：value?.AsString、value?.AsNumber、value?.AsBool、value?.AsSymbol、value?.AsObject、value?.AsValues；不匹配返回 null，0/false 保留。</remarks>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<VuetifyGroupModelValue?> ModelValueChanged { get; set; }

    /// <summary>
    /// 是否允许多选。
    /// Allows multiple selections.
    /// </summary>
    [Parameter]
    [ECMAScriptName("multiple")]
    public bool Multiple { get; set; }

    /// <summary>
    /// 是否强制选中。
    /// Whether selection is mandatory.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyMandatoryValue?；值域为 bool | VuetifyMandatoryMode。变量用 Mandatory="@value"，并保持声明的分支类型。投影：value?.AsBool、value?.AsMode；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("mandatory")]
    public VuetifyMandatoryValue? Mandatory { get; set; }

    /// <summary>
    /// 最大可选数量。
    /// Maximum number of selectable items.
    /// </summary>
    /// <remarks data-authoring="types">C# 类型为 int?；Razor 数值写 Max="@(32)" 或 Max="@value"，由 C#/Razor 检查转换。Number/double 遵循 JavaScript 双精度语义，大整数 ID 使用字符串。</remarks>
    [Parameter]
    [ECMAScriptName("max")]
    public int? Max { get; set; }

    /// <summary>
    /// 选中项应用的 CSS 类名。
    /// CSS class applied to selected items.
    /// </summary>
    [Parameter]
    [ECMAScriptName("selectedClass")]
    public string? SelectedClass { get; set; }

    /// <summary>
    /// 是否禁用组件。
    /// Disables the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 渲染的 HTML 标签名。
    /// HTML tag name to render.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tag")]
    public string? Tag { get; set; }

    /// <summary>
    /// 是否启用移动端布局。
    /// Whether mobile layout is active.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyMobileValue?；值域为 bool。变量用 Mobile="@value"，并保持声明的分支类型。投影：value?.AsBool；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("mobile")]
    public VuetifyMobileValue? Mobile { get; set; }

    /// <summary>
    /// 移动端断点阈值。
    /// Mobile breakpoint threshold.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyDisplayBreakpoint?；值域为 string | Number。数值用 MobileBreakpoint="@(32)"；变量用 MobileBreakpoint="@value"，无需 double 后缀。字符串用 MobileBreakpoint="text"；数字字符串保持 string。投影：value?.AsString、value?.AsNumber；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("mobileBreakpoint")]
    public VuetifyDisplayBreakpoint? MobileBreakpoint { get; set; }

    /// <summary>
    /// 是否始终将活动项居中显示。
    /// Always center the active item.
    /// </summary>
    [Parameter]
    [ECMAScriptName("centerActive")]
    public bool CenterActive { get; set; }

    /// <summary>
    /// 滑动方向。
    /// Slide direction.
    /// </summary>
    [Parameter]
    [ECMAScriptName("direction")]
    public VuetifyInputDirection? Direction { get; set; }

    /// <summary>
    /// 下一个导航图标。
    /// Icon for the next navigation arrow.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyIconValue?；值域为 bool | string | Symbol | VueProps。变量用 NextIcon="@value"，并保持声明的分支类型。字符串用 NextIcon="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsString、value?.AsSymbol、value?.AsComponent；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("nextIcon")]
    public VuetifyIconValue? NextIcon { get; set; }

    /// <summary>
    /// 上一个导航图标。
    /// Icon for the previous navigation arrow.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyIconValue?；值域为 bool | string | Symbol | VueProps。变量用 PrevIcon="@value"，并保持声明的分支类型。字符串用 PrevIcon="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsString、value?.AsSymbol、value?.AsComponent；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("prevIcon")]
    public VuetifyIconValue? PrevIcon { get; set; }

    /// <summary>
    /// 箭头显示条件。
    /// When to show navigation arrows.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyShowArrowsValue?；值域为 bool | VuetifyShowArrowsMode。变量用 ShowArrows="@value"，并保持声明的分支类型。投影：value?.AsBool、value?.AsMode；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("showArrows")]
    public VuetifyShowArrowsValue? ShowArrows { get; set; }

    /// <summary>
    /// 附加的额外 HTML 属性。
    /// Additional unmatched HTML attributes.
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
    public RenderFragment<VSlideGroupSlotContext>? ChildContent { get; set; }

    /// <summary>
    /// 上一个导航插槽。
    /// Previous navigation slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("prev")]
    public RenderFragment<VSlideGroupSlotContext>? Prev { get; set; }

    /// <summary>
    /// 下一个导航插槽。
    /// Next navigation slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("next")]
    public RenderFragment<VSlideGroupSlotContext>? Next { get; set; }
}
