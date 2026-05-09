using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VForm")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VForm : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool FastFail { get; set; }

    [Parameter]
    public bool Readonly { get; set; }

    [Parameter]
    public VuetifyValidateOn? ValidateOn { get; set; }

    [Parameter]
    public bool? ModelValue { get; set; }

    [Parameter]
    public EventCallback<bool?> ModelValueChanged { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
