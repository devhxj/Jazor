using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VSnackbar")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class VSnackbar : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public bool ModelValue { get; set; }

    [Parameter]
    public EventCallback<bool> ModelValueChanged { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public VuetifyBooleanStringValue? Timer { get; set; }

    [Parameter]
    public VueStringNumberValue? Timeout { get; set; }

    [Parameter]
    public VuetifyLocation? Location { get; set; }

    [Parameter]
    public VuetifyRoundedValue? Rounded { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
