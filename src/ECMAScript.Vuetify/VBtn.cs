using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace ECMAScript.Vuetify;

/// <summary>
/// First-wave Vuetify button stub for RazorVue authoring.
/// </summary>
[VueLibraryComponent("vuetify/components", "VBtn")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VBtn : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public VuetifyVariant? Variant { get; set; }

    [Parameter]
    public string? Size { get; set; }

    [Parameter]
    public bool Loading { get; set; }

    [Parameter]
    public bool Block { get; set; }

    [Parameter]
    public string? Href { get; set; }

    [Parameter]
    public string? Target { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public EventCallback OnClick { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
