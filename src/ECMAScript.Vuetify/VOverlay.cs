using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VOverlay")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(AfterEnter), VueEmitKind.LibrarySpecific, Name = "afterEnter")]
[VueLibraryEmit(nameof(AfterLeave), VueEmitKind.LibrarySpecific, Name = "afterLeave")]
[VueLibraryEmit(nameof(ClickOutside), VueEmitKind.LibrarySpecific, Name = "click:outside")]
[VueLibraryProp(nameof(ZIndex), Name = "zIndex")]
[VueLibrarySlot(nameof(Activator), Name = "activator")]
public sealed class VOverlay : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public bool ModelValue { get; set; }

    [Parameter]
    public EventCallback<bool> ModelValueChanged { get; set; }

    [Parameter]
    public bool Absolute { get; set; }

    [Parameter]
    public VuetifyAttachTarget? Attach { get; set; }

    [Parameter]
    public bool Contained { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool Eager { get; set; }

    [Parameter]
    public bool NoClickAnimation { get; set; }

    [Parameter]
    public bool Persistent { get; set; }

    [Parameter]
    public bool CloseOnBack { get; set; }

    [Parameter]
    public bool CloseOnContentClick { get; set; }

    [Parameter]
    public bool CloseOnClick { get; set; }

    [Parameter]
    public bool OpenOnClick { get; set; }

    [Parameter]
    public bool OpenOnFocus { get; set; }

    [Parameter]
    public bool OpenOnHover { get; set; }

    [Parameter]
    public VueStringNumberValue? OpenDelay { get; set; }

    [Parameter]
    public VueStringNumberValue? CloseDelay { get; set; }

    [Parameter]
    public VueProps? ActivatorProps { get; set; }

    [Parameter]
    public VueProps? ContentProps { get; set; }

    [Parameter]
    public string? Id { get; set; }

    [Parameter]
    public VuetifyLocation? Location { get; set; }

    [Parameter]
    public VuetifyLocation? Origin { get; set; }

    [Parameter]
    public VueStringNumberValue? Offset { get; set; }

    [Parameter]
    public VuetifyLocationStrategy? LocationStrategy { get; set; }

    [Parameter]
    public VuetifyScrollStrategy? ScrollStrategy { get; set; }

    [Parameter]
    public VuetifyScrimValue? Scrim { get; set; }

    [Parameter]
    public string? Theme { get; set; }

    [Parameter]
    public VuetifyTransitionValue? Transition { get; set; }

    [Parameter]
    public VueStringNumberValue? ZIndex { get; set; }

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
    public EventCallback AfterEnter { get; set; }

    [Parameter]
    public EventCallback AfterLeave { get; set; }

    [Parameter]
    public EventCallback<MouseEvent> ClickOutside { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment<VOverlayActivatorContext>? Activator { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
