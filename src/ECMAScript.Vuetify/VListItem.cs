using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 列表项组件，用于列表中的单个交互条目。
/// Vuetify list item component for a single interactive entry within a list.
/// </summary>
[VueLibraryComponent("vuetify/components", "VListItem")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibrarySlot(nameof(TitleContent), Name = "title")]
[VueLibrarySlot(nameof(SubtitleContent), Name = "subtitle")]
public sealed class VListItem : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public bool? Active { get; set; }

    [Parameter]
    public string? ActiveClass { get; set; }

    [Parameter]
    public string? ActiveColor { get; set; }

    [Parameter]
    public string? AppendAvatar { get; set; }

    [Parameter]
    public string? AppendIcon { get; set; }

    [Parameter]
    public string? BaseColor { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public VuetifyListLines? Lines { get; set; }

    [Parameter]
    public bool? Link { get; set; }

    [Parameter]
    public bool Nav { get; set; }

    [Parameter]
    public string? PrependAvatar { get; set; }

    [Parameter]
    public string? PrependIcon { get; set; }

    [Parameter]
    public VuetifyRippleValue? Ripple { get; set; }

    [Parameter]
    public bool Slim { get; set; }

    [Parameter]
    public VuetifyTextValue? Subtitle { get; set; }

    [Parameter]
    public VuetifyTextValue? Title { get; set; }

    [Parameter]
    public VueValue? Value { get; set; }

    [Parameter]
    public VuetifyDensity? Density { get; set; }

    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    [Parameter]
    public VueStringNumberValue? MinHeight { get; set; }

    [Parameter]
    public VueStringNumberValue? MinWidth { get; set; }

    [Parameter]
    public VueStringNumberValue? MaxHeight { get; set; }

    [Parameter]
    public VueStringNumberValue? MaxWidth { get; set; }

    [Parameter]
    public VueStringNumberValue? Elevation { get; set; }

    [Parameter]
    public VuetifyRoundedValue? Rounded { get; set; }

    [Parameter]
    public string? Href { get; set; }

    [Parameter]
    public string? To { get; set; }

    [Parameter]
    public bool Replace { get; set; }

    [Parameter]
    public bool Exact { get; set; }

    [Parameter]
    public VuetifyVariant? Variant { get; set; }

    [Parameter]
    public EventCallback OnClick { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment<VListItemSlotContext>? Prepend { get; set; }

    [Parameter]
    public RenderFragment<VListItemSlotContext>? Append { get; set; }

    [Parameter]
    public RenderFragment<VListItemTitleSlotContext>? TitleContent { get; set; }

    [Parameter]
    public RenderFragment<VListItemSubtitleSlotContext>? SubtitleContent { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
