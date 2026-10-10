using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 芯片组组件创作代理。
/// Vuetify chip group component authoring proxy.
/// </summary>
[ECMAScript("vuetify/components/VChipGroup")]
public sealed class VChipGroup : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 芯片组的绑定值。
    /// The bound value of the chip group.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyGroupModelValue?；值域为 string | Number | bool | Symbol | VueProps | VuetifyGroupModelValue[]。数值用 ModelValue="@(32)"；变量用 ModelValue="@value"，无需 double 后缀。字符串用 ModelValue="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsNumber、value?.AsBool、value?.AsSymbol、value?.AsObject、value?.AsValues；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public VuetifyGroupModelValue? ModelValue { get; set; }

    /// <summary>
    /// 绑定值变更回调。
    /// Callback invoked when the bound value changes.
    /// </summary>
    /// <remarks data-authoring="types">C# 回调参数为 VuetifyGroupModelValue?；保持与模型相同的强类型。投影：value?.AsString、value?.AsNumber、value?.AsBool、value?.AsSymbol、value?.AsObject、value?.AsValues；不匹配返回 null，0/false 保留。</remarks>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<VuetifyGroupModelValue?> ModelValueChanged { get; set; }

    /// <summary>
    /// 未选中芯片的基础颜色。
    /// The base color for unselected chips.
    /// </summary>
    [Parameter]
    [ECMAScriptName("baseColor")]
    public string? BaseColor { get; set; }

    /// <summary>
    /// 是否将活动项居中显示。
    /// Whether to center the active item.
    /// </summary>
    [Parameter]
    [ECMAScriptName("centerActive")]
    public bool CenterActive { get; set; }

    /// <summary>
    /// 组件的主题色。
    /// The theme color of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// 是否允许芯片换行排列。
    /// Whether to allow chips to wrap into multiple columns.
    /// </summary>
    [Parameter]
    [ECMAScriptName("column")]
    public bool Column { get; set; }

    /// <summary>
    /// 是否显示为筛选样式。
    /// Whether to display chips in filter style.
    /// </summary>
    [Parameter]
    [ECMAScriptName("filter")]
    public bool Filter { get; set; }

    /// <summary>
    /// 芯片排列方向。
    /// The direction of chip layout.
    /// </summary>
    [Parameter]
    [ECMAScriptName("direction")]
    public VuetifyInputDirection? Direction { get; set; }

    /// <summary>
    /// 是否强制选择。
    /// Whether selection is mandatory.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyMandatoryValue?；值域为 bool | VuetifyMandatoryMode。变量用 Mandatory="@value"，并保持声明的分支类型。投影：value?.AsBool、value?.AsMode；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("mandatory")]
    public VuetifyMandatoryValue? Mandatory { get; set; }

    /// <summary>
    /// 最大可选数量。
    /// The maximum number of selectable chips.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Max="@(32)"；变量用 Max="@value"，无需 double 后缀。字符串用 Max="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("max")]
    public VueStringNumberValue? Max { get; set; }

    /// <summary>
    /// 是否允许多选。
    /// Whether to allow multiple selection.
    /// </summary>
    [Parameter]
    [ECMAScriptName("multiple")]
    public bool Multiple { get; set; }

    /// <summary>
    /// 移动端显示配置。
    /// The mobile display configuration.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyMobileValue?；值域为 bool。变量用 Mobile="@value"，并保持声明的分支类型。投影：value?.AsBool；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("mobile")]
    public VuetifyMobileValue? Mobile { get; set; }

    /// <summary>
    /// 下一项图标。
    /// The icon for the next navigation control.
    /// </summary>
    [Parameter]
    [ECMAScriptName("nextIcon")]
    public string? NextIcon { get; set; }

    /// <summary>
    /// 上一项图标。
    /// The icon for the previous navigation control.
    /// </summary>
    [Parameter]
    [ECMAScriptName("prevIcon")]
    public string? PrevIcon { get; set; }

    /// <summary>
    /// 是否显示导航箭头。
    /// Whether to show navigation arrows.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyShowArrowsValue?；值域为 bool | VuetifyShowArrowsMode。变量用 ShowArrows="@value"，并保持声明的分支类型。投影：value?.AsBool、value?.AsMode；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("showArrows")]
    public VuetifyShowArrowsValue? ShowArrows { get; set; }

    /// <summary>
    /// 选中时应用的 CSS 类名。
    /// The CSS class applied when selected.
    /// </summary>
    [Parameter]
    [ECMAScriptName("selectedClass")]
    public string? SelectedClass { get; set; }

    /// <summary>
    /// 芯片的视觉变体样式。
    /// The visual variant style of chips.
    /// </summary>
    [Parameter]
    [ECMAScriptName("variant")]
    public VuetifyVariant? Variant { get; set; }

    /// <summary>
    /// 渲染的 HTML 标签名。
    /// The HTML tag name to render.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tag")]
    public string? Tag { get; set; }

    /// <summary>
    /// 值比较函数。
    /// The value comparator function.
    /// </summary>
    [Parameter]
    [ECMAScriptName("valueComparator")]
    public VuetifyValueComparator? ValueComparator { get; set; }

    /// <summary>
    /// 附加的自定义属性。
    /// Additional custom attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 子内容插槽。
    /// Slot for child content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }
}
