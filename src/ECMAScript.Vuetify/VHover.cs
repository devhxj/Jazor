using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 悬停组件，用于检测和响应鼠标悬停状态。
/// Vuetify hover component for detecting and responding to mouse hover state.
/// </summary>
[VueLibraryComponent("vuetify/components", "VHover")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class VHover : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool ModelValue { get; set; }

    [Parameter]
    public EventCallback<bool> ModelValueChanged { get; set; }

    [Parameter]
    public VueStringNumberValue? OpenDelay { get; set; }

    [Parameter]
    public VueStringNumberValue? CloseDelay { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment<VHoverDefaultSlotContext>? ChildContent { get; set; }
}
