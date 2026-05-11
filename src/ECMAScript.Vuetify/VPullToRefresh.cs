using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify labs pull-to-refresh authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/labs/components", "VPullToRefresh")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(Load), VueEmitKind.LibrarySpecific, Name = "load")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
[VueLibrarySlot(nameof(PullDownPanel), Name = "pullDownPanel")]
public sealed class VPullToRefresh : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public Number? PullDownThreshold { get; set; }

    [Parameter]
    public EventCallback<VPullToRefreshLoadOptions> Load { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public RenderFragment<VPullToRefreshPanelSlotContext>? PullDownPanel { get; set; }
}
