using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify tabs-window authoring proxy for tab panel content.
/// </summary>
[VueLibraryComponent("vuetify/components", "VTabsWindow")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class VTabsWindow : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public VuetifyGroupModelValue? ModelValue { get; set; }

    [Parameter]
    public EventCallback<VuetifyGroupModelValue?> ModelValueChanged { get; set; }

    [Parameter]
    public bool Reverse { get; set; }

    [Parameter]
    public VuetifyInputDirection? Direction { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public string? SelectedClass { get; set; }

    [Parameter]
    public string? Tag { get; set; }

    [Parameter]
    public string? Theme { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}

