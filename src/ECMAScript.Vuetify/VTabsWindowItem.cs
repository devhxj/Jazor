using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 标签页窗口项目组件的编写代理。
/// Vuetify tabs-window-item authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VTabsWindowItem")]
public sealed class VTabsWindowItem : ComponentBase
{
    /// <summary>
    /// 值。
    /// The value used to identify this item.
    /// </summary>
    [Parameter]
    [ECMAScriptName("value")]
    public VuetifyGroupModelValue? Value { get; set; }

    /// <summary>
    /// 禁用。
    /// Disables the item.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 选中项CSS类。
    /// CSS class applied when selected.
    /// </summary>
    [Parameter]
    [ECMAScriptName("selectedClass")]
    public string? SelectedClass { get; set; }

    /// <summary>
    /// 急切加载。
    /// Forces the component to be eager-loaded.
    /// </summary>
    [Parameter]
    [ECMAScriptName("eager")]
    public bool Eager { get; set; }

    /// <summary>
    /// 过渡。
    /// Transition effect.
    /// </summary>
    [Parameter]
    [ECMAScriptName("transition")]
    public VuetifyBooleanStringValue? Transition { get; set; }

    /// <summary>
    /// 反向过渡。
    /// Reverse transition effect.
    /// </summary>
    [Parameter]
    [ECMAScriptName("reverseTransition")]
    public VuetifyBooleanStringValue? ReverseTransition { get; set; }

    /// <summary>
    /// 组选中事件。
    /// Group selected event.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onGroup:selected")]
    public EventCallback<VuetifyGroupSelectedEvent> OnGroupSelected { get; set; }

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
