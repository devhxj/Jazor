using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 单选按钮组组件。
/// Vuetify radio group component.
/// </summary>
[VueLibraryComponent("vuetify/components", "VRadioGroup")]
public sealed class VRadioGroup : ComponentBase
{
    /// <summary>
    /// 单选按钮组的标签文本。
    /// The label text of the radio group.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// 单选按钮组的颜色。
    /// The color of the radio group.
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
    /// 是否为只读状态。
    /// Whether the radio group is read-only.
    /// </summary>
    [Parameter]
    public bool Readonly { get; set; }

    /// <summary>
    /// 是否隐藏提示详细信息。
    /// Whether to hide the details/hints section.
    /// </summary>
    [Parameter]
    public VuetifyHideDetailsValue? HideDetails { get; set; }

    /// <summary>
    /// 显示的提示消息。
    /// The hint messages to display.
    /// </summary>
    [Parameter]
    public VuetifyMessagesValue? Messages { get; set; }

    /// <summary>
    /// 是否禁用整个单选按钮组。
    /// Whether the entire radio group is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// 是否将单选按钮横向排列。
    /// Whether to display radio buttons inline horizontally.
    /// </summary>
    [Parameter]
    public bool Inline { get; set; }

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

    /// <summary>
    /// 默认插槽内容，用于放置单选按钮。
    /// The default slot for placing radio buttons.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
