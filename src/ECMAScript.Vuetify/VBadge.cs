using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VBadge")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibrarySlot(nameof(BadgeContent), Name = "badge")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
/// <summary>
/// Vuetify 徽章组件。
/// Vuetify badge component.
/// </summary>
public sealed class VBadge : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public VuetifyTransitionValue? Transition { get; set; }

    [Parameter]
    public string? Theme { get; set; }

    [Parameter]
    public string? Tag { get; set; }

    [Parameter]
    public VuetifyRoundedValue? Rounded { get; set; }

    [Parameter]
    public bool Tile { get; set; }

    [Parameter]
    public VuetifyLocation? Location { get; set; }

    [Parameter]
    public VueClassValue? Class { get; set; }

    [Parameter]
    public VuetifyStyleValue? Style { get; set; }

    [Parameter]
    public bool Bordered { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public VueStringNumberValue? Content { get; set; }

    [Parameter]
    public bool Dot { get; set; }

    [Parameter]
    public bool Floating { get; set; }

    [Parameter]
    public VuetifyIconValue? Icon { get; set; }

    [Parameter]
    public bool Inline { get; set; }

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public VueStringNumberValue? Max { get; set; }

    [Parameter]
    public bool ModelValue { get; set; } = true;

    [Parameter]
    public EventCallback<bool> ModelValueChanged { get; set; }

    [Parameter]
    public VueStringNumberValue? OffsetX { get; set; }

    [Parameter]
    public VueStringNumberValue? OffsetY { get; set; }

    [Parameter]
    public string? TextColor { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? BadgeContent { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
