using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify infinite-scroll authoring proxy for incremental list loading.
/// </summary>
[VueLibraryComponent("vuetify/components", "VInfiniteScroll")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(Load), VueEmitKind.LibrarySpecific, Name = "load")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
[VueLibrarySlot(nameof(Loading), Name = "loading")]
[VueLibrarySlot(nameof(Error), Name = "error")]
[VueLibrarySlot(nameof(Empty), Name = "empty")]
[VueLibrarySlot(nameof(LoadMore), Name = "load-more")]
public sealed class VInfiniteScroll : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Tag { get; set; }

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
    public string? Color { get; set; }

    [Parameter]
    public VuetifyInputDirection? Direction { get; set; }

    [Parameter]
    public VuetifyInfiniteScrollSide? Side { get; set; }

    [Parameter]
    public VuetifyInfiniteScrollMode? Mode { get; set; }

    [Parameter]
    public VueStringNumberValue? Margin { get; set; }

    [Parameter]
    public string? LoadMoreText { get; set; }

    [Parameter]
    public string? EmptyText { get; set; }

    [Parameter]
    public EventCallback<VInfiniteScrollLoadOptions> Load { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public RenderFragment<VInfiniteScrollSlotContext>? Loading { get; set; }

    [Parameter]
    public RenderFragment<VInfiniteScrollSlotContext>? Error { get; set; }

    [Parameter]
    public RenderFragment<VInfiniteScrollSlotContext>? Empty { get; set; }

    [Parameter]
    public RenderFragment<VInfiniteScrollSlotContext>? LoadMore { get; set; }
}
