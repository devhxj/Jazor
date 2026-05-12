using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 空状态创作代理，用于无数据和引导界面。
/// Vuetify empty-state authoring proxy for no-data and onboarding surfaces.
/// </summary>
[VueLibraryComponent("vuetify/components", "VEmptyState")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ActionClick), VueEmitKind.LibrarySpecific, Name = "click:action")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
[VueLibrarySlot(nameof(Actions), Name = "actions")]
[VueLibrarySlot(nameof(HeadlineContent), Name = "headline")]
[VueLibrarySlot(nameof(TitleContent), Name = "title")]
[VueLibrarySlot(nameof(Media), Name = "media")]
[VueLibrarySlot(nameof(TextContent), Name = "text")]
public sealed class VEmptyState : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Theme { get; set; }

    [Parameter]
    public VueStringNumberValue? Size { get; set; }

    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    [Parameter]
    public VueStringNumberValue? MaxHeight { get; set; }

    [Parameter]
    public VueStringNumberValue? MaxWidth { get; set; }

    [Parameter]
    public VueStringNumberValue? MinHeight { get; set; }

    [Parameter]
    public VueStringNumberValue? MinWidth { get; set; }

    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    [Parameter]
    public string? ActionText { get; set; }

    [Parameter]
    public string? BgColor { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public VuetifyIconValue? Icon { get; set; }

    [Parameter]
    public string? Image { get; set; }

    [Parameter]
    public VuetifyJustify? Justify { get; set; }

    [Parameter]
    public string? Headline { get; set; }

    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public VueStringNumberValue? TextWidth { get; set; }

    [Parameter]
    public string? Href { get; set; }

    [Parameter]
    public string? To { get; set; }

    [Parameter]
    public EventCallback<Event> ActionClick { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public RenderFragment<VEmptyStateActionsSlotContext>? Actions { get; set; }

    [Parameter]
    public RenderFragment? HeadlineContent { get; set; }

    [Parameter]
    public RenderFragment? TitleContent { get; set; }

    [Parameter]
    public RenderFragment? Media { get; set; }

    [Parameter]
    public RenderFragment? TextContent { get; set; }
}
