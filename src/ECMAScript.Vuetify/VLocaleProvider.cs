using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify locale-provider authoring proxy for scoped locale, fallback, RTL, and messages.
/// </summary>
[VueLibraryComponent("vuetify/components", "VLocaleProvider")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class VLocaleProvider : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Locale { get; set; }

    [Parameter]
    public string? FallbackLocale { get; set; }

    [Parameter]
    public VueProps? Messages { get; set; }

    [Parameter]
    public bool? Rtl { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
