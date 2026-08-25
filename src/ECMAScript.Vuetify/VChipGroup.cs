using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 芯片组组件创作代理。
/// Vuetify chip group component authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VChipGroup")]
public sealed class VChipGroup : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 芯片组的绑定值。
    /// The bound value of the chip group.
    /// </summary>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public VuetifyGroupModelValue? ModelValue { get; set; }

    /// <summary>
    /// 绑定值变更回调。
    /// Callback invoked when the bound value changes.
    /// </summary>
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
    [Parameter]
    [ECMAScriptName("mandatory")]
    public VuetifyMandatoryValue? Mandatory { get; set; }

    /// <summary>
    /// 最大可选数量。
    /// The maximum number of selectable chips.
    /// </summary>
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
