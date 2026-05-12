using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 选择控件组件的编写代理。
/// Vuetify selection-control authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VSelectionControl")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
[VueLibrarySlot(nameof(LabelContent), Name = "label")]
[VueLibrarySlot(nameof(Input), Name = "input")]
public sealed class VSelectionControl : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 输入元素的唯一标识符。
    /// The unique identifier for the input element.
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
    /// The type of the input control.
    /// </summary>
    [Parameter]
    public string? Type { get; set; }

    /// <summary>
    /// 控件的标签文本。
    /// The label text of the control.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

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
    /// 控件的颜色。
    /// The color of the control.
    /// </summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// 控件的基础颜色。
    /// The base color of the control.
    /// </summary>
    [Parameter]
    public string? BaseColor { get; set; }

    /// <summary>
    /// 组件的紧凑程度。
    /// The density/compactness of the component.
    /// </summary>
    [Parameter]
    public VuetifyDensity? Density { get; set; }

    /// <summary>
    /// 是否禁用控件。
    /// Whether the control is disabled.
    /// </summary>
    [Parameter]
    public VuetifyNullableBoolean? Disabled { get; set; }

    /// <summary>
    /// 是否为只读状态。
    /// Whether the control is read-only.
    /// </summary>
    [Parameter]
    public VuetifyNullableBoolean? Readonly { get; set; }

    /// <summary>
    /// 是否处于错误状态。
    /// Whether the control is in an error state.
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
    /// 控件的当前绑定值。
    /// The current bound value of the control.
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
    /// 控件的提交值。
    /// The submission value of the control.
    /// </summary>
    [Parameter]
    public VuetifyGroupModelValue? Value { get; set; }

    /// <summary>
    /// 选中时对应的值。
    /// The value when the control is checked.
    /// </summary>
    [Parameter]
    public VuetifyGroupModelValue? TrueValue { get; set; }

    /// <summary>
    /// 未选中时对应的值。
    /// The value when the control is unchecked.
    /// </summary>
    [Parameter]
    public VuetifyGroupModelValue? FalseValue { get; set; }

    /// <summary>
    /// 附加到根元素上的额外属性。
    /// Additional attributes applied to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽内容。
    /// The default slot content.
    /// </summary>
    [Parameter]
    public RenderFragment<VSelectionControlDefaultSlotContext>? ChildContent { get; set; }

    /// <summary>
    /// 自定义标签内容的插槽。
    /// Custom content for the label slot.
    /// </summary>
    [Parameter]
    public RenderFragment<VSelectionControlLabelSlotContext>? LabelContent { get; set; }

    /// <summary>
    /// 自定义输入元素内容的插槽。
    /// Custom content for the input element slot.
    /// </summary>
    [Parameter]
    public RenderFragment<VSelectionControlInputSlotContext>? Input { get; set; }
}
