using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VChip")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class VChip : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public VuetifyTextValue? Text { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public bool Closable { get; set; }

    [Parameter]
    public string? PrependIcon { get; set; }

    [Parameter]
    public string? AppendIcon { get; set; }

    [Parameter]
    public VueStringNumberValue? Size { get; set; }

    [Parameter]
    public VuetifyRoundedValue? Rounded { get; set; }

    [Parameter]
    public VueStringNumberValue? Elevation { get; set; }

    [Parameter]
    public EventCallback OnClick { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
