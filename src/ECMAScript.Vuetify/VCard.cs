using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// First-wave Vuetify card stub for child-content composition.
/// </summary>
[VueLibraryComponent("vuetify/components", "VCard")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibrarySlot(nameof(TextContent), Name = "text")]
[VueLibrarySlot(nameof(TitleContent), Name = "title")]
[VueLibrarySlot(nameof(SubtitleContent), Name = "subtitle")]
[VueLibrarySlot(nameof(ImageContent), Name = "image")]
[VueLibrarySlot(nameof(Prepend), Name = "prepend")]
[VueLibrarySlot(nameof(Append), Name = "append")]
[VueLibrarySlot(nameof(Actions), Name = "actions")]
[VueLibrarySlot(nameof(Item), Name = "item")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class VCard : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public VuetifyTextValue? Title { get; set; }

    [Parameter]
    public VuetifyTextValue? Subtitle { get; set; }

    [Parameter]
    public VuetifyTextValue? Text { get; set; }

    [Parameter]
    public string? PrependIcon { get; set; }

    [Parameter]
    public string? AppendIcon { get; set; }

    [Parameter]
    public string? PrependAvatar { get; set; }

    [Parameter]
    public string? AppendAvatar { get; set; }

    [Parameter]
    public string? Image { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public VuetifyVariant? Variant { get; set; }

    [Parameter]
    public VuetifyDensity? Density { get; set; }

    [Parameter]
    public VueStringNumberValue? Elevation { get; set; }

    [Parameter]
    public VuetifyRoundedValue? Rounded { get; set; }

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
    public bool Disabled { get; set; }

    [Parameter]
    public bool Flat { get; set; }

    [Parameter]
    public bool Hover { get; set; }

    [Parameter]
    public bool Link { get; set; }

    [Parameter]
    public string? Href { get; set; }

    [Parameter]
    public string? To { get; set; }

    [Parameter]
    public bool Replace { get; set; }

    [Parameter]
    public bool Exact { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? TextContent { get; set; }

    [Parameter]
    public RenderFragment? TitleContent { get; set; }

    [Parameter]
    public RenderFragment? SubtitleContent { get; set; }

    [Parameter]
    public RenderFragment? ImageContent { get; set; }

    [Parameter]
    public RenderFragment? Prepend { get; set; }

    [Parameter]
    public RenderFragment? Append { get; set; }

    [Parameter]
    public RenderFragment? Actions { get; set; }

    [Parameter]
    public RenderFragment? Item { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
