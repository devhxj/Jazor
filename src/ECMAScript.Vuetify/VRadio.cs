using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 单选按钮组件。
/// Vuetify radio button component.
/// </summary>
[VueLibraryComponent("vuetify/components", "VRadio")]
public sealed class VRadio : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 单选按钮的标签文本。
    /// The label text of the radio button.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// 单选按钮选中时的颜色。
    /// The color when the radio button is selected.
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
    /// 是否禁用单选按钮。
    /// Whether the radio button is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// 是否为只读状态。
    /// Whether the radio button is read-only.
    /// </summary>
    [Parameter]
    public bool Readonly { get; set; }

    /// <summary>
    /// 未选中状态下显示的图标。
    /// The icon displayed when unchecked.
    /// </summary>
    [Parameter]
    public string? FalseIcon { get; set; }

    /// <summary>
    /// 选中状态下显示的图标。
    /// The icon displayed when checked.
    /// </summary>
    [Parameter]
    public string? TrueIcon { get; set; }

    /// <summary>
    /// 单选按钮的值。
    /// The value of the radio button.
    /// </summary>
    [Parameter]
    public string? Value { get; set; }

    /// <summary>
    /// 当前选中的值。
    /// The currently selected value.
    /// </summary>
    [Parameter]
    public string? ModelValue { get; set; }

    /// <summary>
    /// 选中值变更时触发的回调。
    /// Callback invoked when the selected value changes.
    /// </summary>
    [Parameter]
    public EventCallback<string?> ModelValueChanged { get; set; }

    /// <summary>
    /// 附加到根元素上的额外属性。
    /// Additional attributes applied to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
