using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 标签页窗口项目组件的编写代理。
/// Vuetify tabs-window-item authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VTabsWindowItem")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(GroupSelected), VueEmitKind.LibrarySpecific, Name = "group:selected")]
[VueSlot(nameof(ChildContent), IsDefault = true)]
public sealed class VTabsWindowItem : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 值。
    /// The value used to identify this item.
    /// </summary>
    [Parameter]
    public VuetifyGroupModelValue? Value { get; set; }

    /// <summary>
    /// 禁用。
    /// Disables the item.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// 选中项CSS类。
    /// CSS class applied when selected.
    /// </summary>
    [Parameter]
    public string? SelectedClass { get; set; }

    /// <summary>
    /// 急切加载。
    /// Forces the component to be eager-loaded.
    /// </summary>
    [Parameter]
    public bool Eager { get; set; }

    /// <summary>
    /// 过渡。
    /// Transition effect.
    /// </summary>
    [Parameter]
    public VuetifyBooleanStringValue? Transition { get; set; }

    /// <summary>
    /// 反向过渡。
    /// Reverse transition effect.
    /// </summary>
    [Parameter]
    public VuetifyBooleanStringValue? ReverseTransition { get; set; }

    /// <summary>
    /// 组选中事件。
    /// Group selected event.
    /// </summary>
    [Parameter]
    public EventCallback<VuetifyGroupSelectedEvent> GroupSelected { get; set; }

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
