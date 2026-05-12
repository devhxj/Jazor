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
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class VTabsWindowItem : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public VuetifyGroupModelValue? Value { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public string? SelectedClass { get; set; }

    [Parameter]
    public bool Eager { get; set; }

    [Parameter]
    public VuetifyBooleanStringValue? Transition { get; set; }

    [Parameter]
    public VuetifyBooleanStringValue? ReverseTransition { get; set; }

    [Parameter]
    public EventCallback<VuetifyGroupSelectedEvent> GroupSelected { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}

