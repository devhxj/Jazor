using ECMAScript.VueContract;
using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 展开面板组件。
/// Vuetify expansion-panel component.
/// </summary>
[VueLibraryComponent("vuetify/components", "VExpansionPanel")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibrarySlot(nameof(TitleContent), Name = "title")]
[VueLibrarySlot(nameof(TextContent), Name = "text")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class VExpansionPanel : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? BgColor { get; set; }

    [Parameter]
    public string? CollapseIcon { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public VueStringNumberValue? Elevation { get; set; }

    [Parameter]
    public string? ExpandIcon { get; set; }

    [Parameter]
    public string? HideActions { get; set; }

    [Parameter]
    public bool Readonly { get; set; }

    [Parameter]
    public VuetifyRoundedValue? Rounded { get; set; }

    [Parameter]
    public VuetifyTextValue? Text { get; set; }

    [Parameter]
    public VuetifyTextValue? Title { get; set; }

    [Parameter]
    public VueValue? Value { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment<VExpansionPanelTitleSlotContext>? TitleContent { get; set; }

    [Parameter]
    public RenderFragment? TextContent { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
