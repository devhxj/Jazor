using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 标签页窗口组件的编写代理，用于标签页面板内容。
/// Vuetify tabs-window authoring proxy for tab panel content.
/// </summary>
[ECMAScript("vuetify/components", Transform.Component, "VTabsWindow")]
public sealed class VTabsWindow : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 模型值。
    /// Model value.
    /// </summary>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public VuetifyGroupModelValue? ModelValue { get; set; }

    /// <summary>
    /// 模型值变化事件。
    /// Model value changed event.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<VuetifyGroupModelValue?> ModelValueChanged { get; set; }

    /// <summary>
    /// 反转。
    /// Reverses the transition direction.
    /// </summary>
    [Parameter]
    [ECMAScriptName("reverse")]
    public bool Reverse { get; set; }

    /// <summary>
    /// 滑动方向。
    /// Slide direction.
    /// </summary>
    [Parameter]
    [ECMAScriptName("direction")]
    public VuetifyInputDirection? Direction { get; set; }

    /// <summary>
    /// 禁用。
    /// Disables the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 选中项CSS类。
    /// CSS class applied to the selected item.
    /// </summary>
    [Parameter]
    [ECMAScriptName("selectedClass")]
    public string? SelectedClass { get; set; }

    /// <summary>
    /// 根标签。
    /// Root element tag.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tag")]
    public string? Tag { get; set; }

    /// <summary>
    /// 主题名。
    /// Theme name.
    /// </summary>
    [Parameter]
    [ECMAScriptName("theme")]
    public string? Theme { get; set; }

    /// <summary>
    /// 额外属性。
    /// Additional attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽。
    /// Default slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }
}
