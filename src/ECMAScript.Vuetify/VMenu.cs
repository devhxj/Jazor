using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 菜单组件。
/// Vuetify menu component.
/// </summary>
[VueLibraryComponent("vuetify/components", "VMenu")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibrarySlot(nameof(Activator), Name = "activator")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class VMenu : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public bool ModelValue { get; set; }

    [Parameter]
    public EventCallback<bool> ModelValueChanged { get; set; }

    [Parameter]
    public bool CloseOnContentClick { get; set; }

    [Parameter]
    public bool CloseOnBack { get; set; }

    [Parameter]
    public bool CloseOnClick { get; set; }

    [Parameter]
    public bool OpenOnClick { get; set; }

    [Parameter]
    public bool OpenOnHover { get; set; }

    [Parameter]
    public bool OpenOnFocus { get; set; }

    [Parameter]
    public VueStringNumberValue? OpenDelay { get; set; }

    [Parameter]
    public VueStringNumberValue? CloseDelay { get; set; }

    [Parameter]
    public VuetifyLocation? Location { get; set; }

    [Parameter]
    public VuetifyLocation? Origin { get; set; }

    [Parameter]
    public VueStringNumberValue? Offset { get; set; }

    [Parameter]
    public VuetifyScrollStrategy? ScrollStrategy { get; set; }

    [Parameter]
    public bool Persistent { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public VueStringNumberValue? MinWidth { get; set; }

    [Parameter]
    public VueStringNumberValue? MaxWidth { get; set; }

    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    [Parameter]
    public VuetifyTransitionValue? Transition { get; set; }

    [Parameter]
    public VueProps? ActivatorProps { get; set; }

    [Parameter]
    public VueProps? ContentProps { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment<VOverlayActivatorContext>? Activator { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
