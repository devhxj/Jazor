using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 芯片组组件创作代理。
/// Vuetify chip group component authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VChipGroup", StyleUrls = [VuetifyLibraryAssets.StyleUrl])]
public sealed class VChipGroup : ComponentBase
{
    /// <summary>
    /// 芯片组的绑定值。
    /// The bound value of the chip group.
    /// </summary>
    [Parameter]
    public VuetifyGroupModelValue? ModelValue { get; set; }

    /// <summary>
    /// 绑定值变更回调。
    /// Callback invoked when the bound value changes.
    /// </summary>
    [Parameter]
    public EventCallback<VuetifyGroupModelValue?> ModelValueChanged { get; set; }

    /// <summary>
    /// 未选中芯片的基础颜色。
    /// The base color for unselected chips.
    /// </summary>
    [Parameter]
    public string? BaseColor { get; set; }

    /// <summary>
    /// 是否将活动项居中显示。
    /// Whether to center the active item.
    /// </summary>
    [Parameter]
    public bool CenterActive { get; set; }

    /// <summary>
    /// 组件的主题色。
    /// The theme color of the component.
    /// </summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// 是否允许芯片换行排列。
    /// Whether to allow chips to wrap into multiple columns.
    /// </summary>
    [Parameter]
    public bool Column { get; set; }

    /// <summary>
    /// 是否显示为筛选样式。
    /// Whether to display chips in filter style.
    /// </summary>
    [Parameter]
    public bool Filter { get; set; }

    /// <summary>
    /// 芯片排列方向。
    /// The direction of chip layout.
    /// </summary>
    [Parameter]
    public VuetifyInputDirection? Direction { get; set; }

    /// <summary>
    /// 是否强制选择。
    /// Whether selection is mandatory.
    /// </summary>
    [Parameter]
    public VuetifyMandatoryValue? Mandatory { get; set; }

    /// <summary>
    /// 最大可选数量。
    /// The maximum number of selectable chips.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Max { get; set; }

    /// <summary>
    /// 是否允许多选。
    /// Whether to allow multiple selection.
    /// </summary>
    [Parameter]
    public bool Multiple { get; set; }

    /// <summary>
    /// 移动端显示配置。
    /// The mobile display configuration.
    /// </summary>
    [Parameter]
    public VuetifyMobileValue? Mobile { get; set; }

    /// <summary>
    /// 下一项图标。
    /// The icon for the next navigation control.
    /// </summary>
    [Parameter]
    public string? NextIcon { get; set; }

    /// <summary>
    /// 上一项图标。
    /// The icon for the previous navigation control.
    /// </summary>
    [Parameter]
    public string? PrevIcon { get; set; }

    /// <summary>
    /// 是否显示导航箭头。
    /// Whether to show navigation arrows.
    /// </summary>
    [Parameter]
    public VuetifyShowArrowsValue? ShowArrows { get; set; }

    /// <summary>
    /// 选中时应用的 CSS 类名。
    /// The CSS class applied when selected.
    /// </summary>
    [Parameter]
    public string? SelectedClass { get; set; }

    /// <summary>
    /// 芯片的视觉变体样式。
    /// The visual variant style of chips.
    /// </summary>
    [Parameter]
    public VuetifyVariant? Variant { get; set; }

    /// <summary>
    /// 渲染的 HTML 标签名。
    /// The HTML tag name to render.
    /// </summary>
    [Parameter]
    public string? Tag { get; set; }

    /// <summary>
    /// 值比较函数。
    /// The value comparator function.
    /// </summary>
    [Parameter]
    public VuetifyValueComparator? ValueComparator { get; set; }

    /// <summary>
    /// 附加的自定义属性。
    /// Additional custom attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 子内容插槽。
    /// Slot for child content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
