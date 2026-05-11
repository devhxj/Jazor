using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// First-wave Vuetify tooltip stub for RazorVue authoring.
/// </summary>
[VueLibraryComponent("vuetify/components", "VTooltip")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibrarySlot(nameof(Activator), Name = "activator")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class VTooltip : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public bool ModelValue { get; set; }

    [Parameter]
    public EventCallback<bool> ModelValueChanged { get; set; }

    [Parameter]
    public string? Id { get; set; }

    [Parameter]
    public bool Interactive { get; set; }

    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public VuetifyLocation? Location { get; set; }

    [Parameter]
    public VuetifyLocation? Origin { get; set; }

    [Parameter]
    public VueStringNumberValue? Offset { get; set; }

    [Parameter]
    public bool OpenOnClick { get; set; }

    [Parameter]
    public bool OpenOnHover { get; set; }

    [Parameter]
    public bool OpenOnFocus { get; set; }

    [Parameter]
    public VueStringNumberValue? OpenDelay { get; set; }

    [Parameter]
    public VueStringNumberValue? CloseDelay { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool Eager { get; set; }

    [Parameter]
    public VueStringNumberValue? MinWidth { get; set; }

    [Parameter]
    public VueStringNumberValue? MaxWidth { get; set; }

    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    [Parameter]
    public VuetifyTransitionValue? Transition { get; set; }

    [Parameter]
    public VueProps? ActivatorProps { get; set; }

    [Parameter]
    public VueProps? ContentProps { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment<VOverlayActivatorContext>? Activator { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
