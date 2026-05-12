using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 条目组组件，用于管理一组可选项的选中状态。
/// Vuetify item group component for managing selection state across a group of items.
/// </summary>
[VueLibraryComponent("vuetify/components", "VItemGroup")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class VItemGroup : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public VuetifyGroupModelValue? ModelValue { get; set; }

    [Parameter]
    public EventCallback<VuetifyGroupModelValue?> ModelValueChanged { get; set; }

    [Parameter]
    public VuetifyMandatoryValue? Mandatory { get; set; }

    [Parameter]
    public VueStringNumberValue? Max { get; set; }

    [Parameter]
    public bool Multiple { get; set; }

    [Parameter]
    public string? SelectedClass { get; set; }

    [Parameter]
    public string? Tag { get; set; }

    [Parameter]
    public VuetifyValueComparator? ValueComparator { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment<VItemGroupDefaultSlotContext>? ChildContent { get; set; }
}
