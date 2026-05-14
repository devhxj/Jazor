using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 标签页窗口组件的编写代理，用于标签页面板内容。
/// Vuetify tabs-window authoring proxy for tab panel content.
/// </summary>
[VueLibraryComponent("vuetify/components", "VTabsWindow")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueSlot(nameof(ChildContent), IsDefault = true)]
public sealed class VTabsWindow : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 模型值。
    /// Model value.
    /// </summary>
    [Parameter]
    public VuetifyGroupModelValue? ModelValue { get; set; }

    /// <summary>
    /// 模型值变化事件。
    /// Model value changed event.
    /// </summary>
    [Parameter]
    public EventCallback<VuetifyGroupModelValue?> ModelValueChanged { get; set; }

    /// <summary>
    /// 反转。
    /// Reverses the transition direction.
    /// </summary>
    [Parameter]
    public bool Reverse { get; set; }

    /// <summary>
    /// 滑动方向。
    /// Slide direction.
    /// </summary>
    [Parameter]
    public VuetifyInputDirection? Direction { get; set; }

    /// <summary>
    /// 禁用。
    /// Disables the component.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// 选中项CSS类。
    /// CSS class applied to the selected item.
    /// </summary>
    [Parameter]
    public string? SelectedClass { get; set; }

    /// <summary>
    /// 根标签。
    /// Root element tag.
    /// </summary>
    [Parameter]
    public string? Tag { get; set; }

    /// <summary>
    /// 主题名。
    /// Theme name.
    /// </summary>
    [Parameter]
    public string? Theme { get; set; }

    /// <summary>
    /// 额外属性。
    /// Additional attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽。
    /// Default slot.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
