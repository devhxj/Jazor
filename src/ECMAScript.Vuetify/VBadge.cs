using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VBadge")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class VBadge : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public VueStringNumberValue? Content { get; set; }

    [Parameter]
    public string? Icon { get; set; }

    [Parameter]
    public VueStringNumberValue? Max { get; set; }

    [Parameter]
    public VueStringNumberValue? OffsetX { get; set; }

    [Parameter]
    public VueStringNumberValue? OffsetY { get; set; }

    [Parameter]
    public bool ModelValue { get; set; } = true;

    [Parameter]
    public EventCallback<bool> ModelValueChanged { get; set; }

    [Parameter]
    public bool Dot { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
