using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 图片组件，支持懒加载、宽高比和响应式源。
/// Vuetify image component with lazy loading, aspect ratio, and responsive sources.
/// </summary>
[VueLibraryComponent("vuetify/components", "VImg")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(LoadStart), VueEmitKind.LibrarySpecific, Name = "loadstart")]
[VueLibraryEmit(nameof(Load), VueEmitKind.LibrarySpecific, Name = "load")]
[VueLibraryEmit(nameof(LoadError), VueEmitKind.LibrarySpecific, Name = "error")]
[VueLibraryProp(nameof(CrossOrigin), Name = "crossorigin")]
[VueLibraryProp(nameof(ReferrerPolicy), Name = "referrerpolicy")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
[VueLibrarySlot(nameof(Placeholder), Name = "placeholder")]
[VueLibrarySlot(nameof(ErrorContent), Name = "error")]
[VueLibrarySlot(nameof(Sources), Name = "sources")]
public sealed class VImg : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public VImgSource? Src { get; set; }

    [Parameter]
    public string? Alt { get; set; }

    [Parameter]
    public string? LazySrc { get; set; }

    [Parameter]
    public string? Srcset { get; set; }

    [Parameter]
    public string? Sizes { get; set; }

    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    [Parameter]
    public VueStringNumberValue? MaxHeight { get; set; }

    [Parameter]
    public VueStringNumberValue? MaxWidth { get; set; }

    [Parameter]
    public VueStringNumberValue? MinHeight { get; set; }

    [Parameter]
    public VueStringNumberValue? MinWidth { get; set; }

    [Parameter]
    public VueStringNumberValue? AspectRatio { get; set; }

    [Parameter]
    public VuetifyTransitionValue? Transition { get; set; }

    [Parameter]
    public bool Cover { get; set; }

    [Parameter]
    public bool Eager { get; set; }

    [Parameter]
    public bool Tile { get; set; }

    [Parameter]
    public bool Inline { get; set; }

    [Parameter]
    public bool Absolute { get; set; }

    [Parameter]
    public VuetifyRoundedValue? Rounded { get; set; }

    [Parameter]
    public VueClassValue? Class { get; set; }

    [Parameter]
    public VuetifyStyleValue? Style { get; set; }

    [Parameter]
    public VueClassValue? ContentClass { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public string? Gradient { get; set; }

    [Parameter]
    public VuetifyIntersectionObserverOptions? Options { get; set; }

    [Parameter]
    public string? Position { get; set; }

    [Parameter]
    public VImgDraggableValue? Draggable { get; set; }

    [Parameter]
    public VImgCrossOrigin? CrossOrigin { get; set; }

    [Parameter]
    public VImgReferrerPolicy? ReferrerPolicy { get; set; }

    [Parameter]
    public EventCallback<string?> LoadStart { get; set; }

    [Parameter]
    public EventCallback<string?> Load { get; set; }

    [Parameter]
    public EventCallback<string?> LoadError { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public RenderFragment? Placeholder { get; set; }

    [Parameter]
    public RenderFragment? ErrorContent { get; set; }

    [Parameter]
    public RenderFragment? Sources { get; set; }
}
