using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 选择控件分组组件的编写代理。
/// Vuetify selection-control group authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VSelectionControlGroup")]
public sealed class VSelectionControlGroup : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 分组的唯一标识符。
    /// The unique identifier for the group.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// 表单元素的 name 属性。
    /// The name attribute for the form element.
    /// </summary>
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// 输入控件的类型。
    /// The type of the input controls.
    /// </summary>
    [Parameter]
    public string? Type { get; set; }

    /// <summary>
    /// 组件使用的主题名称。
    /// The theme name used by the component.
    /// </summary>
    [Parameter]
    public string? Theme { get; set; }

    /// <summary>
    /// 默认提供者的目标属性名。
    /// The target property name for the defaults provider.
    /// </summary>
    [Parameter]
    public string? DefaultsTarget { get; set; }

    /// <summary>
    /// 控件组的颜色。
    /// The color of the control group.
    /// </summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// 组件的紧凑程度。
    /// The density/compactness of the component.
    /// </summary>
    [Parameter]
    public VuetifyDensity? Density { get; set; }

    /// <summary>
    /// 是否禁用整个控件组。
    /// Whether to disable the entire control group.
    /// </summary>
    [Parameter]
    public VuetifyNullableBoolean? Disabled { get; set; }

    /// <summary>
    /// 是否为只读状态。
    /// Whether the controls are read-only.
    /// </summary>
    [Parameter]
    public VuetifyNullableBoolean? Readonly { get; set; }

    /// <summary>
    /// 是否处于错误状态。
    /// Whether the group is in an error state.
    /// </summary>
    [Parameter]
    public bool Error { get; set; }

    /// <summary>
    /// 是否将控件横向排列。
    /// Whether to display controls inline horizontally.
    /// </summary>
    [Parameter]
    public bool Inline { get; set; }

    /// <summary>
    /// 是否支持多选。
    /// Whether to support multiple selections.
    /// </summary>
    [Parameter]
    public VuetifyNullableBoolean? Multiple { get; set; }

    /// <summary>
    /// 未选中状态下显示的图标。
    /// The icon displayed when unchecked.
    /// </summary>
    [Parameter]
    public VuetifyIconValue? FalseIcon { get; set; }

    /// <summary>
    /// 选中状态下显示的图标。
    /// The icon displayed when checked.
    /// </summary>
    [Parameter]
    public VuetifyIconValue? TrueIcon { get; set; }

    /// <summary>
    /// 是否启用涟漪效果。
    /// Whether to enable the ripple effect.
    /// </summary>
    [Parameter]
    public VuetifyRippleValue? Ripple { get; set; }

    /// <summary>
    /// 用于比较值的自定义比较器。
    /// The custom comparator used for value comparison.
    /// </summary>
    [Parameter]
    public VuetifyValueComparator? ValueComparator { get; set; }

    /// <summary>
    /// 控件组的当前绑定值。
    /// The current bound value of the control group.
    /// </summary>
    [Parameter]
    public VuetifyGroupModelValue? ModelValue { get; set; }

    /// <summary>
    /// 绑定值变更时触发的回调。
    /// Callback invoked when the bound value changes.
    /// </summary>
    [Parameter]
    public EventCallback<VuetifyGroupModelValue?> ModelValueChanged { get; set; }

    /// <summary>
    /// 附加到根元素上的额外属性。
    /// Additional attributes applied to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽内容，用于放置选择控件。
    /// The default slot for placing selection controls.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
